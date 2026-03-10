using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;
using POSAPI.Services;

namespace POSAPI.Hubs
{
    public class PrintHub : Hub
    {
        private readonly IConfiguration _configuration;
        private readonly PrintAgentPresenceService _presence;
        private readonly IPrintJobService _printJobs;

        public PrintHub(
            IConfiguration configuration,
            PrintAgentPresenceService presence,
            IPrintJobService printJobs)
        {
            _configuration = configuration;
            _presence = presence;
            _printJobs = printJobs;
        }

        public static string TerminalGroup(string terminalCode) => $"terminal:{terminalCode}";

        public async Task<bool> RegisterAgent(string terminalCode, string locationCode, string sharedKey)
        {
            var expected = _configuration["PrintAgent:SharedKey"];
            if (string.IsNullOrWhiteSpace(expected) || !string.Equals(expected, sharedKey, StringComparison.Ordinal))
                return false;

            _presence.Register(terminalCode, locationCode, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, TerminalGroup(terminalCode));
            return true;
        }

        public async Task ReportJobStatus(PrintJobStatusUpdateDto dto)
        {
            await _printJobs.UpdateStatusAsync(dto);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _presence.UnregisterByConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
