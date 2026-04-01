using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using POS.UI.Shared;
using POS.UI.Services;
using POS.UI.State;
using System.Globalization;
using System.Net;
using System.Linq;

namespace POS.UI.Pages
{
    public partial class LayawayReport : ComponentBase, IAsyncDisposable
    {
        private const string CustomerNotFoundText = "Customer not found";

        [Inject] public IReportService ReportService { get; set; } = default!;
        [Inject] public ICustomerService CustomerService { get; set; } = default!;
        [Inject] public ILocationService LocationService { get; set; } = default!;
        [Inject] public AppSnackbarService AppSnackbar { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public ICompanyBrandingService CompanyBrandingService { get; set; } = default!;

        private List<LayawayReportLineDto>? Rows;
        private bool IsLoading;
        private bool _disposed;
        private int _requestVersion;
        private CancellationTokenSource? _reloadCts;
        private DotNetObjectReference<LayawayReport>? _dotNetRef;
        private string? _shortcutRegistrationId;
        private CompanyBrandingDto? _branding;
        private IReadOnlyDictionary<string, LocationDto> _locationLookup = new Dictionary<string, LocationDto>(StringComparer.OrdinalIgnoreCase);
        private IReadOnlyDictionary<string, CustomerDto> _customerLookup = new Dictionary<string, CustomerDto>(StringComparer.OrdinalIgnoreCase);

        protected string? FromLocation { get; set; } = PosWorkflowState.PosLocation;
        protected string? CategoryACode { get; set; }
        protected string? CategoryAName { get; set; }
        protected string? CategoryBCode { get; set; }
        protected string? CategoryBName { get; set; }
        protected string? CategoryCCode { get; set; }
        protected string? CategoryCName { get; set; }
        protected string ProductType { get; set; } = "Both";
        protected string CustomerMode { get; set; } = "All";
        protected string? CustomerAccNo { get; set; }
        protected string CustomerName { get; set; } = string.Empty;
        protected string CustomerStatus { get; set; } = string.Empty;
        protected bool ShowCustomerSearch { get; set; }

        protected bool HasRows => Rows is { Count: > 0 };
        protected bool IsCustomerAll => string.Equals(CustomerMode, "All", StringComparison.OrdinalIgnoreCase);
        protected bool IsCustomerOne => string.Equals(CustomerMode, "One", StringComparison.OrdinalIgnoreCase);
        protected bool IsTypeBoth => string.Equals(ProductType, "Both", StringComparison.OrdinalIgnoreCase);
        protected bool IsTypeMajor => string.Equals(ProductType, "Major", StringComparison.OrdinalIgnoreCase);
        protected bool IsTypeMinor => string.Equals(ProductType, "Minor", StringComparison.OrdinalIgnoreCase);

        protected override async Task OnInitializedAsync()
        {
            await LoadLocationLookupAsync();
            _branding = await CompanyBrandingService.GetAsync();
            await LoadReportAsync(showWarning: false);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _dotNetRef = DotNetObjectReference.Create(this);
            _shortcutRegistrationId = await JS.InvokeAsync<string>(
                "blazorShortcuts.register",
                _dotNetRef,
                new
                {
                    scopeSelector = "#layaway-report-form",
                    enableEsc = true,
                    enableEnter = false,
                    enableF5 = true
                });
        }

        public async ValueTask DisposeAsync()
        {
            _disposed = true;
            _reloadCts?.Cancel();
            _reloadCts?.Dispose();

            if (!string.IsNullOrWhiteSpace(_shortcutRegistrationId))
            {
                try
                {
                    await JS.InvokeVoidAsync("blazorShortcuts.unregister", _shortcutRegistrationId);
                }
                catch
                {
                }
            }

            _dotNetRef?.Dispose();
        }

        protected RenderFragment HeaderActions => builder =>
        {
            var seq = 0;
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "d-flex align-items-center gap-2 flex-wrap justify-content-end");

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "form-shell-meta");
            builder.AddContent(seq++, IsLoading ? "Refreshing..." : HasRows ? $"{Rows!.Count} rows" : "Ready");
            builder.CloseElement();

            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", "btn btn-outline-secondary btn-sm");
            builder.AddAttribute(seq++, "disabled", IsLoading);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, RefreshNow));
            builder.AddContent(seq++, "Refresh");
            builder.CloseElement();

            builder.CloseElement();
        };

        [JSInvokable]
        public async Task OnF5()
        {
            if (!IsCustomerOne)
                CustomerMode = "One";

            ShowCustomerSearch = true;
            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public async Task OnEsc()
        {
            if (!ShowCustomerSearch)
                return;

            ShowCustomerSearch = false;
            await InvokeAsync(StateHasChanged);
        }

        protected Task RefreshNow() => LoadReportAsync(showWarning: true);

        protected async Task QueueReloadAsync()
        {
            if (_disposed)
                return;

            _reloadCts?.Cancel();
            _reloadCts?.Dispose();
            _reloadCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(250, _reloadCts.Token);
                await LoadReportAsync(showWarning: false);
            }
            catch (TaskCanceledException)
            {
            }
        }

        protected async Task LoadReportAsync(bool showWarning)
        {
            var requestVersion = Interlocked.Increment(ref _requestVersion);
            IsLoading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                if (IsCustomerOne)
                {
                    await ResolveCustomerAsync(false);
                    if (string.IsNullOrWhiteSpace(CustomerAccNo))
                    {
                        Rows = new();
                        if (showWarning)
                            await AppSnackbar.Show("Enter customer account number.", SnackbarType.Warning);
                        return;
                    }
                }

                var request = new LayawayReportRequestDto
                {
                    FromLocation = NormalizeLocation(FromLocation),
                    CategoryA = string.IsNullOrWhiteSpace(CategoryACode) ? null : CategoryACode.Trim(),
                    CategoryB = string.IsNullOrWhiteSpace(CategoryBCode) ? null : CategoryBCode.Trim(),
                    CategoryC = string.IsNullOrWhiteSpace(CategoryCCode) ? null : CategoryCCode.Trim(),
                    ProductType = ProductType,
                    CustomerMode = CustomerMode,
                    CustomerAccNo = string.IsNullOrWhiteSpace(CustomerAccNo) ? null : CustomerAccNo.Trim()
                };

                var res = await ReportService.GetLayawayReportAsync(request);
                if (requestVersion != _requestVersion)
                    return;

                if (!res.Success)
                {
                    if (showWarning)
                        await AppSnackbar.Show(res.Message ?? "Unable to load report.", SnackbarType.Error);

                    Rows = new();
                    return;
                }

                Rows = res.Data ?? new();
                await LoadCustomerLookupAsync(Rows);
            }
            catch (Exception ex) when (!(_disposed || ex is TaskCanceledException))
            {
                if (requestVersion == _requestVersion && showWarning)
                    await AppSnackbar.Show("Unable to load report.", SnackbarType.Error);

                if (requestVersion == _requestVersion)
                    Rows = new();
            }
            finally
            {
                if (requestVersion == _requestVersion)
                {
                    IsLoading = false;
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        protected static string? NormalizeLocation(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slot)
                ? slot.ToString("D2", CultureInfo.InvariantCulture)
                : trimmed.ToUpperInvariant();
        }

        protected Task OnFromLocationChanged(string? value) { FromLocation = value; return QueueReloadAsync(); }
        protected Task OnCategoryACodeChanged(string? value) { CategoryACode = value; return QueueReloadAsync(); }
        protected Task OnCategoryANameChanged(string? value) { CategoryAName = value; return QueueReloadAsync(); }
        protected Task OnCategoryBCodeChanged(string? value) { CategoryBCode = value; return QueueReloadAsync(); }
        protected Task OnCategoryBNameChanged(string? value) { CategoryBName = value; return QueueReloadAsync(); }
        protected Task OnCategoryCCodeChanged(string? value) { CategoryCCode = value; return QueueReloadAsync(); }
        protected Task OnCategoryCNameChanged(string? value) { CategoryCName = value; return QueueReloadAsync(); }

        protected async Task SetCustomerModeAsync(string mode)
        {
            CustomerMode = mode;
            if (IsCustomerAll)
            {
                CustomerAccNo = null;
                CustomerName = string.Empty;
                CustomerStatus = string.Empty;
                ShowCustomerSearch = false;
            }

            await QueueReloadAsync();
        }

        protected Task SetCustomerModeAllAsync() => SetCustomerModeAsync("All");
        protected Task SetCustomerModeOneAsync() => SetCustomerModeAsync("One");

        protected async Task SetProductTypeAsync(string type)
        {
            ProductType = type;
            await QueueReloadAsync();
        }

        protected Task SetProductTypeBothAsync() => SetProductTypeAsync("Both");
        protected Task SetProductTypeMajorAsync() => SetProductTypeAsync("Major");
        protected Task SetProductTypeMinorAsync() => SetProductTypeAsync("Minor");

        protected async Task OnCustomerAccChanged(ChangeEventArgs e)
        {
            CustomerAccNo = TrimOrNull(e.Value?.ToString());
            await ResolveCustomerAsync(false);
            await QueueReloadAsync();
        }

        protected async Task OpenCustomerSearchAsync()
        {
            if (!IsCustomerOne)
                CustomerMode = "One";

            ShowCustomerSearch = true;
            await InvokeAsync(StateHasChanged);
        }

        protected async Task CloseCustomerSearchAsync()
        {
            ShowCustomerSearch = false;
            await InvokeAsync(StateHasChanged);
        }

        protected async Task OnCustomerSelectedAsync(CustomerDto customer)
        {
            ShowCustomerSearch = false;
            CustomerMode = "One";
            CustomerAccNo = TrimOrNull(customer.AccNo);
            await ResolveCustomerAsync(false);
            await QueueReloadAsync();
        }

        protected async Task ResolveCustomerAsync(bool showSnackbar)
        {
            if (!IsCustomerOne || string.IsNullOrWhiteSpace(CustomerAccNo))
            {
                CustomerName = string.Empty;
                CustomerStatus = string.Empty;
                return;
            }

            var res = await CustomerService.GetAsync(CustomerAccNo.Trim());
            if (res.Success && res.Data != null)
            {
                CustomerName = GetCustomerDisplayName(res.Data);
                CustomerStatus = string.Empty;
                return;
            }

            CustomerName = string.Empty;
            CustomerStatus = CustomerNotFoundText;
            if (showSnackbar)
                await AppSnackbar.Show(CustomerNotFoundText, SnackbarType.Warning);
        }

        protected static string GetCustomerDisplayName(CustomerDto customer)
        {
            return DisplayLabelFormatter.GetCustomerName(customer);
        }

        protected Task ExportCsv()
            => HasRows
                ? JS.InvokeVoidAsync("reportExport.downloadCsv", $"layaway-report-{DateTime.Now:yyyyMMddHHmmss}.csv", BuildCsv(Rows ?? new())).AsTask()
                : Task.CompletedTask;

        protected Task ExportPdf()
            => HasRows
                ? ExportPdfAsync()
                : Task.CompletedTask;

        private async Task ExportPdfAsync()
        {
            _branding = await CompanyBrandingService.GetAsync();
            await JS.InvokeVoidAsync("reportExport.printHtml", "Layaway Report", BuildPrintHtml("Layaway Report", Rows ?? new()));
        }

        protected static string BuildCsv(List<LayawayReportLineDto> rows)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CUSTOMER ACC,DATE,LOC,SALES CODE,PART/STOCK NO,MFR,MAKE,SEARCH1,SEARCH2,CAT A,CAT B,CAT C,SUPPLIER,QTY,COST,TOTAL COST,PRICE,TOTAL PRICE,RESERVED,TYPE,REF ID/JOB NO,DELIVERY TYPE");
            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(row.CustomerAcc),
                    Csv(row.DateAndTime.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)),
                    Csv(row.Location),
                    Csv(row.SalesCode),
                    Csv(row.PartStockNo),
                    Csv(row.Mfr),
                    Csv(row.Make),
                    Csv(row.Search1),
                    Csv(row.Search2),
                    Csv(row.CatA),
                    Csv(row.CatB),
                    Csv(row.CatC),
                    Csv(row.Supplier),
                    Csv(row.Qty.ToString(CultureInfo.InvariantCulture)),
                    Csv(row.Cost.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.TotalCost.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.Price.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.Reserved),
                    Csv(row.Type),
                    Csv(row.RefIdJobNo),
                    Csv(row.DeliveryType)));
            }
            return sb.ToString();
        }

        protected string BuildPrintHtml(string title, List<LayawayReportLineDto> rows)
        {
            var location = WebUtility.HtmlEncode(GetLocationDisplay(FromLocation));
            var customer = WebUtility.HtmlEncode(IsCustomerOne
                ? (string.IsNullOrWhiteSpace(CustomerName) ? CustomerStatus : $"{CustomerName} ({CustomerAccNo})")
                : "All");
            var type = WebUtility.HtmlEncode(ProductType);
            var categoryA = WebUtility.HtmlEncode(CategoryAName ?? CategoryACode ?? "All");
            var categoryB = WebUtility.HtmlEncode(CategoryBName ?? CategoryBCode ?? "All");
            var categoryC = WebUtility.HtmlEncode(CategoryCName ?? CategoryCCode ?? "All");

            var sb = new System.Text.StringBuilder();
            sb.Append(PrintTheme.BuildDocumentStart(title, true, "table{width:100%;border-collapse:collapse;table-layout:fixed;}th,td{border:1px solid #cfcfcf;padding:2px 3px;vertical-align:top;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;font-size:6px;}th{background:#f4f6f8;font-weight:bold;text-transform:uppercase;}.num{text-align:right;}th:nth-child(1),td:nth-child(1){width:6%;}th:nth-child(2),td:nth-child(2){width:5%;}th:nth-child(3),td:nth-child(3){width:3%;}th:nth-child(4),td:nth-child(4){width:5%;}th:nth-child(5),td:nth-child(5){width:7%;}th:nth-child(6),td:nth-child(6){width:4%;}th:nth-child(7),td:nth-child(7){width:4%;}th:nth-child(8),td:nth-child(8){width:4%;}th:nth-child(9),td:nth-child(9){width:4%;}th:nth-child(10),td:nth-child(10){width:4%;}th:nth-child(11),td:nth-child(11){width:4%;}th:nth-child(12),td:nth-child(12){width:4%;}th:nth-child(13),td:nth-child(13){width:5%;}th:nth-child(14),td:nth-child(14){width:3%;}th:nth-child(15),td:nth-child(15){width:4%;}th:nth-child(16),td:nth-child(16){width:5%;}th:nth-child(17),td:nth-child(17){width:4%;}th:nth-child(18),td:nth-child(18){width:5%;}th:nth-child(19),td:nth-child(19){width:4%;}th:nth-child(20),td:nth-child(20){width:4%;}th:nth-child(21),td:nth-child(21){width:5%;}th:nth-child(22),td:nth-child(22){width:5%;}"));
            sb.Append("<div class=\"page\">");
            sb.Append(PrintTheme.BuildHeader(title, _branding, "Layaway report"));
            sb.Append($"<div class=\"meta section\">Location: {location}<br/>Customer: {customer}<br/>Type: {type}<br/>Categories: A={categoryA}, B={categoryB}, C={categoryC}</div>");
            sb.Append("<table><thead><tr><th>CUSTOMER</th><th>DATE</th><th>LOC</th><th>SALES CODE</th><th>PART/STOCK NO</th><th>MFR</th><th>MAKE</th><th>SEARCH1</th><th>SEARCH2</th><th>CAT A</th><th>CAT B</th><th>CAT C</th><th>SUPPLIER</th><th class=\"num\">QTY</th><th class=\"num\">COST</th><th class=\"num\">TOTAL COST</th><th class=\"num\">PRICE</th><th class=\"num\">TOTAL PRICE</th><th>RESERVED</th><th>TYPE</th><th>REF ID/JOB NO</th><th>DELIVERY TYPE</th></tr></thead><tbody>");
            foreach (var row in rows)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{WebUtility.HtmlEncode(GetCustomerDisplay(row.CustomerAcc))}</td><td>{row.DateAndTime:dd-MM-yyyy}</td><td>{WebUtility.HtmlEncode(GetLocationDisplay(row.Location))}</td><td>{WebUtility.HtmlEncode(row.SalesCode)}</td><td>{WebUtility.HtmlEncode(row.PartStockNo)}</td><td>{WebUtility.HtmlEncode(row.Mfr)}</td><td>{WebUtility.HtmlEncode(row.Make)}</td><td>{WebUtility.HtmlEncode(row.Search1)}</td><td>{WebUtility.HtmlEncode(row.Search2)}</td><td>{WebUtility.HtmlEncode(row.CatA)}</td><td>{WebUtility.HtmlEncode(row.CatB)}</td><td>{WebUtility.HtmlEncode(row.CatC)}</td><td>{WebUtility.HtmlEncode(row.Supplier)}</td><td class=\"num\">{row.Qty}</td><td class=\"num\">{row.Cost:0.00}</td><td class=\"num\">{row.TotalCost:0.00}</td><td class=\"num\">{row.Price:0.00}</td><td class=\"num\">{row.TotalPrice:0.00}</td><td>{WebUtility.HtmlEncode(row.Reserved)}</td><td>{WebUtility.HtmlEncode(row.Type)}</td><td>{WebUtility.HtmlEncode(row.RefIdJobNo)}</td><td>{WebUtility.HtmlEncode(row.DeliveryType)}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div>");
            sb.Append(PrintTheme.BuildDocumentEnd());
            return sb.ToString();
        }

        private async Task LoadLocationLookupAsync()
        {
            try
            {
                var locations = await LocationService.GetAllAsync();
                _locationLookup = locations
                    .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                    .GroupBy(x => DisplayLabelFormatter.NormalizeLocationCode(x.Code), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                _locationLookup = new Dictionary<string, LocationDto>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task LoadCustomerLookupAsync(IEnumerable<LayawayReportLineDto> rows)
        {
            var accounts = rows
                .Select(x => x.CustomerAcc)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (accounts.Count == 0)
            {
                _customerLookup = new Dictionary<string, CustomerDto>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            var lookups = await Task.WhenAll(accounts.Select(async account =>
            {
                try
                {
                    var res = await CustomerService.GetAsync(account);
                    if (res.Success && res.Data != null)
                        return new KeyValuePair<string, CustomerDto>?(
                            new KeyValuePair<string, CustomerDto>(DisplayLabelFormatter.NormalizeCustomerAccountNo(account), res.Data));
                }
                catch
                {
                }

                return null;
            }));

            _customerLookup = lookups
                .Where(x => x.HasValue && !string.IsNullOrWhiteSpace(x.Value.Key))
                .Select(x => x!.Value)
                .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);
        }

        private string GetLocationDisplay(string? code)
            => DisplayLabelFormatter.FormatLocation(code, _locationLookup);

        private string GetCustomerDisplay(string? accountNo)
            => DisplayLabelFormatter.FormatCustomer(accountNo, _customerLookup);

        protected static string Csv(string? value)
        {
            var text = value ?? string.Empty;
            return text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r')
                ? $"\"{text.Replace("\"", "\"\"")}\""
                : text;
        }

        protected static string GetToggleClass(bool active)
            => active ? "btn btn-sm btn-primary" : "btn btn-sm btn-outline-secondary";

        protected static string? TrimOrNull(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
