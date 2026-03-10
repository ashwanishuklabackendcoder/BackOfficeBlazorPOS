using System.Drawing;
using System.Drawing.Printing;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocalPrintAgent.Sample;

public sealed class PrintAgentWorker : BackgroundService
{
    private readonly AgentOptions _options;
    private readonly ILogger<PrintAgentWorker> _logger;
    private readonly HttpClient _http;
    private HubConnection? _hub;

    public PrintAgentWorker(IOptions<AgentOptions> options, ILogger<PrintAgentWorker> logger)
    {
        _options = options.Value;
        _logger = logger;

        Directory.CreateDirectory(_options.FileOutputDirectory);

        _http = new HttpClient
        {
            BaseAddress = new Uri(_options.ApiBaseUrl.TrimEnd('/') + "/")
        };
        _http.DefaultRequestHeaders.Add("X-Agent-Key", _options.AgentKey);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _hub = new HubConnectionBuilder()
            .WithUrl(_options.ApiBaseUrl.TrimEnd('/') + "/hubs/print")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<PrintJobDto>("ReceivePrintJob", async job =>
        {
            await UpdateStatus(job.Id, "Processing");
            try
            {
                var printer = await GetPrinter(job.PrinterConfigId);
                if (printer == null)
                    throw new InvalidOperationException("Printer config unavailable");

                await ExecutePrintAsync(job, printer);
                await UpdateStatus(job.Id, "Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Print job {JobId} failed", job.Id);
                await UpdateStatus(job.Id, "Failed");
            }
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_hub.State != HubConnectionState.Connected)
                {
                    await _hub.StartAsync(stoppingToken);
                    var registered = await _hub.InvokeAsync<bool>(
                        "RegisterAgent",
                        _options.TerminalCode,
                        _options.LocationCode,
                        _options.AgentKey,
                        stoppingToken);

                    if (!registered)
                    {
                        _logger.LogError("Agent registration failed. Check terminal/location/key.");
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue;
                    }

                    _logger.LogInformation("Print agent connected. Terminal={TerminalCode} Location={LocationCode}",
                        _options.TerminalCode, _options.LocationCode);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agent connection loop error");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hub != null)
        {
            try
            {
                await _hub.StopAsync(cancellationToken);
                await _hub.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to stop SignalR hub cleanly");
            }
        }

        _http.Dispose();
        await base.StopAsync(cancellationToken);
    }

    private async Task<PrinterConfigDto?> GetPrinter(int printerId)
    {
        var url = $"api/print-agent/printers/{printerId}?locationCode={_options.LocationCode}";
        var response = await _http.GetFromJsonAsync<ApiResponse<PrinterConfigDto>>(url);
        if (response?.Success != true)
            return null;

        return response.Data;
    }

    private async Task ExecutePrintAsync(PrintJobDto job, PrinterConfigDto printer)
    {
        switch (printer.Mode)
        {
            case (int)PrinterMode.Windows:
                await PrintWindows(job, printer);
                break;
            case (int)PrinterMode.Tcp:
                await PrintTcp(job, printer);
                break;
            case (int)PrinterMode.File:
                await PrintFile(job, printer);
                break;
            case (int)PrinterMode.Email:
                await PrintEmail(job, printer);
                break;
            default:
                throw new InvalidOperationException("Unsupported printer mode");
        }
    }

    private Task PrintWindows(PrintJobDto job, PrinterConfigDto printer)
    {
        var text = NormalizePayload(job);
        var pd = new PrintDocument();
        if (!string.IsNullOrWhiteSpace(printer.LocalPortName))
            pd.PrinterSettings.PrinterName = printer.LocalPortName;

        pd.PrintPage += (_, e) =>
        {
            using var font = new Font("Consolas", 10);
            e.Graphics?.DrawString(text, font, Brushes.Black, 20, 20);
        };

        pd.Print();
        return Task.CompletedTask;
    }

    private async Task PrintTcp(PrintJobDto job, PrinterConfigDto printer)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(printer.IpAddress, printer.TcpPort);
        using var stream = client.GetStream();

        byte[] bytes;
        if (job.JobType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
            bytes = Convert.FromBase64String(job.Payload);
        else
            bytes = Encoding.ASCII.GetBytes(job.Payload);

        await stream.WriteAsync(bytes);
        await stream.FlushAsync();
    }

    private Task PrintFile(PrintJobDto job, PrinterConfigDto printer)
    {
        var extension = string.IsNullOrWhiteSpace(printer.FileExtension) ? "txt" : printer.FileExtension.TrimStart('.');
        var name = string.IsNullOrWhiteSpace(printer.Filename) ? $"printjob-{job.Id}" : printer.Filename;
        if (printer.FileEnsureUnique)
            name = $"{name}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var path = Path.Combine(_options.FileOutputDirectory, $"{name}.{extension}");
        File.WriteAllText(path, NormalizePayload(job));
        return Task.CompletedTask;
    }

    private async Task PrintEmail(PrintJobDto job, PrinterConfigDto printer)
    {
        var tempFile = Path.Combine(_options.FileOutputDirectory, $"mail-{job.Id}.txt");
        await File.WriteAllTextAsync(tempFile, NormalizePayload(job));

        using var message = new MailMessage
        {
            From = new MailAddress(_options.EmailFrom),
            Subject = $"POS Print Job {job.Id}",
            Body = "Auto-generated print output attached."
        };
        message.To.Add(string.IsNullOrWhiteSpace(printer.EmailDestination) ? _options.EmailFrom : printer.EmailDestination);
        message.Attachments.Add(new Attachment(tempFile));

        using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpEnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
            smtp.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);

        await smtp.SendMailAsync(message);
    }

    private string NormalizePayload(PrintJobDto job)
    {
        if (job.JobType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
            return DecodeEscPosToPrintableText(job.Payload);

        return job.Payload;
    }

    private static string DecodeEscPosToPrintableText(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(payload);
            var sb = new StringBuilder(bytes.Length);
            foreach (var b in bytes)
            {
                if (b == 9 || b == 10 || b == 13 || (b >= 32 && b <= 126))
                    sb.Append((char)b);
            }

            var text = sb.ToString().Replace("\r\n", "\n").Replace('\r', '\n');
            return string.IsNullOrWhiteSpace(text) ? "[Receipt data received]" : text;
        }
        catch
        {
            return payload;
        }
    }

    private async Task UpdateStatus(int id, string status)
    {
        await _http.PutAsJsonAsync("api/print-jobs/status", new PrintJobStatusUpdateDto
        {
            JobId = id,
            Status = status
        });
    }
}
