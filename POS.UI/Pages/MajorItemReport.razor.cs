using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using POS.UI.Services;
using POS.UI.Shared;
using System.Globalization;
using System.Net;

namespace POS.UI.Pages
{
    public partial class MajorItemReport : ComponentBase, IAsyncDisposable
    {
        [Inject] public IReportService ReportService { get; set; } = default!;
        [Inject] public AppSnackbarService AppSnackbar { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public ILocationService LocationService { get; set; } = default!;
        [Inject] public ICompanyBrandingService CompanyBrandingService { get; set; } = default!;

        private List<MajorItemReportLineDto>? Rows;
        private bool IsLoading;
        private CompanyBrandingDto? _branding;
        private IReadOnlyDictionary<string, LocationDto> _locationLookup = new Dictionary<string, LocationDto>(StringComparer.OrdinalIgnoreCase);
        private int _requestVersion;
        private bool _disposed;

        private string PrintBy { get; set; } = "InStock";
        private string? FromLocation { get; set; } = "01";
        private string? ToLocation { get; set; } = "30";
        private string? FromDateText { get; set; }
        private string? ToDateText { get; set; }

        private string CategoryLevel { get; set; } = "A";
        private string? CategoryACode { get; set; }
        private string? CategoryAName { get; set; }
        private string? CategoryBCode { get; set; }
        private string? CategoryBName { get; set; }
        private string? CategoryCCode { get; set; }
        private string? CategoryCName { get; set; }

        private string HeadingMode { get; set; } = "Make";
        private string? HeadingMake { get; set; }
        private string? HeadingSupplier { get; set; }
        private string? HeadingCategoryACode { get; set; }
        private string? HeadingCategoryAName { get; set; }
        private string? HeadingCategoryBCode { get; set; }
        private string? HeadingCategoryBName { get; set; }
        private string? HeadingCategoryCCode { get; set; }
        private string? HeadingCategoryCName { get; set; }

        private bool HasRows => Rows is { Count: > 0 };
        private bool IsPrintByInStock => string.Equals(PrintBy, "InStock", StringComparison.OrdinalIgnoreCase);
        private bool IsPrintBySold => string.Equals(PrintBy, "Sold", StringComparison.OrdinalIgnoreCase);
        private bool IsCategoryLevelA => string.Equals(CategoryLevel, "A", StringComparison.OrdinalIgnoreCase);
        private bool IsCategoryLevelB => string.Equals(CategoryLevel, "B", StringComparison.OrdinalIgnoreCase);
        private bool IsCategoryLevelC => string.Equals(CategoryLevel, "C", StringComparison.OrdinalIgnoreCase);
        private bool IsHeadingMake => string.Equals(HeadingMode, "Make", StringComparison.OrdinalIgnoreCase);
        private bool IsHeadingSupplier => string.Equals(HeadingMode, "Supplier", StringComparison.OrdinalIgnoreCase);
        private bool IsHeadingCategoryA => string.Equals(HeadingMode, "CatA", StringComparison.OrdinalIgnoreCase);
        private bool IsHeadingCategoryB => string.Equals(HeadingMode, "CatB", StringComparison.OrdinalIgnoreCase);
        private bool IsHeadingCategoryC => string.Equals(HeadingMode, "CatC", StringComparison.OrdinalIgnoreCase);

        protected override async Task OnInitializedAsync()
        {
            await LoadLocationLookupAsync();
            _branding = await CompanyBrandingService.GetAsync();
        }

        private RenderFragment HeaderActions => builder =>
        {
            var seq = 0;
            builder.OpenElement(seq++, "div");
            builder.AddAttribute(seq++, "class", "d-flex align-items-center gap-2 flex-wrap justify-content-end");

            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "form-shell-meta");
            builder.AddContent(seq++, IsLoading ? "Loading..." : HasRows ? $"{Rows!.Count} rows" : "Ready");
            builder.CloseElement();

            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", "btn btn-outline-secondary btn-sm");
            builder.AddAttribute(seq++, "disabled", IsLoading);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, LoadReportAsync));
            builder.AddContent(seq++, "Load Report");
            builder.CloseElement();

            builder.OpenElement(seq++, "button");
            builder.AddAttribute(seq++, "type", "button");
            builder.AddAttribute(seq++, "class", "btn btn-outline-secondary btn-sm");
            builder.AddAttribute(seq++, "disabled", !HasRows || IsLoading);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, RefreshNow));
            builder.AddContent(seq++, "Refresh");
            builder.CloseElement();

            builder.CloseElement();
        };

        private Task RefreshNow() => LoadReportAsync();

        private async Task LoadReportAsync()
        {
            var requestVersion = Interlocked.Increment(ref _requestVersion);
            IsLoading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                var request = new MajorItemReportRequestDto
                {
                    PrintBy = PrintBy,
                    FromLocation = NormalizeLocation(FromLocation),
                    ToLocation = NormalizeLocation(ToLocation),
                    FromDate = ParseDate(FromDateText),
                    ToDate = ParseDate(ToDateText),
                    CategoryLevel = CategoryLevel,
                    CategoryCode = GetSelectedCategoryCode(),
                    HeadingMode = HeadingMode,
                    HeadingValue = GetSelectedHeadingValue()
                };

                var res = await ReportService.GetMajorItemReportAsync(request);

                if (requestVersion != _requestVersion)
                    return;

                if (!res.Success)
                {
                    await AppSnackbar.Show(res.Message ?? "Unable to load report.", SnackbarType.Error);
                    Rows = new();
                    return;
                }

                Rows = res.Data ?? new();
            }
            catch (Exception ex) when (!(_disposed || ex is TaskCanceledException))
            {
                if (requestVersion == _requestVersion)
                {
                    await AppSnackbar.Show("Unable to load report.", SnackbarType.Error);
                    Rows = new();
                }
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

        private static string? NormalizeLocation(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slot)
                ? slot.ToString("D2", CultureInfo.InvariantCulture)
                : trimmed.ToUpperInvariant();
        }

        private static DateTime? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date.Date
                : null;
        }

        private Task OnFromLocationChanged(string? value) { FromLocation = value; return Task.CompletedTask; }
        private Task OnToLocationChanged(string? value) { ToLocation = value; return Task.CompletedTask; }
        private Task OnFromDateChanged(ChangeEventArgs e) { FromDateText = e.Value?.ToString(); return Task.CompletedTask; }
        private Task OnToDateChanged(ChangeEventArgs e) { ToDateText = e.Value?.ToString(); return Task.CompletedTask; }

        private Task SetPrintByInStock() { PrintBy = "InStock"; return Task.CompletedTask; }
        private Task SetPrintBySold() { PrintBy = "Sold"; return Task.CompletedTask; }
        private Task SetCategoryLevelA() { CategoryLevel = "A"; return Task.CompletedTask; }
        private Task SetCategoryLevelB() { CategoryLevel = "B"; return Task.CompletedTask; }
        private Task SetCategoryLevelC() { CategoryLevel = "C"; return Task.CompletedTask; }
        private Task SetHeadingMake() { HeadingMode = "Make"; return Task.CompletedTask; }
        private Task SetHeadingSupplier() { HeadingMode = "Supplier"; return Task.CompletedTask; }
        private Task SetHeadingCategoryA() { HeadingMode = "CatA"; return Task.CompletedTask; }
        private Task SetHeadingCategoryB() { HeadingMode = "CatB"; return Task.CompletedTask; }
        private Task SetHeadingCategoryC() { HeadingMode = "CatC"; return Task.CompletedTask; }

        private Task OnCategoryACodeChanged(string? value) { CategoryACode = value; return Task.CompletedTask; }
        private Task OnCategoryANameChanged(string? value) { CategoryAName = value; return Task.CompletedTask; }
        private Task OnCategoryBCodeChanged(string? value) { CategoryBCode = value; return Task.CompletedTask; }
        private Task OnCategoryBNameChanged(string? value) { CategoryBName = value; return Task.CompletedTask; }
        private Task OnCategoryCCodeChanged(string? value) { CategoryCCode = value; return Task.CompletedTask; }
        private Task OnCategoryCNameChanged(string? value) { CategoryCName = value; return Task.CompletedTask; }
        private Task OnHeadingMakeChanged(string? value) { HeadingMake = value; return Task.CompletedTask; }
        private Task OnHeadingSupplierChanged(string? value) { HeadingSupplier = value; return Task.CompletedTask; }
        private Task OnHeadingCategoryACodeChanged(string? value) { HeadingCategoryACode = value; return Task.CompletedTask; }
        private Task OnHeadingCategoryANameChanged(string? value) { HeadingCategoryAName = value; return Task.CompletedTask; }
        private Task OnHeadingCategoryBCodeChanged(string? value) { HeadingCategoryBCode = value; return Task.CompletedTask; }
        private Task OnHeadingCategoryBNameChanged(string? value) { HeadingCategoryBName = value; return Task.CompletedTask; }
        private Task OnHeadingCategoryCCodeChanged(string? value) { HeadingCategoryCCode = value; return Task.CompletedTask; }
        private Task OnHeadingCategoryCNameChanged(string? value) { HeadingCategoryCName = value; return Task.CompletedTask; }

        private string? GetSelectedCategoryCode()
            => CategoryLevel.ToUpperInvariant() switch
            {
                "A" => string.IsNullOrWhiteSpace(CategoryACode) ? null : CategoryACode.Trim(),
                "B" => string.IsNullOrWhiteSpace(CategoryBCode) ? null : CategoryBCode.Trim(),
                "C" => string.IsNullOrWhiteSpace(CategoryCCode) ? null : CategoryCCode.Trim(),
                _ => null
            };

        private string? GetSelectedHeadingValue()
            => HeadingMode.ToUpperInvariant() switch
            {
                "MAKE" => string.IsNullOrWhiteSpace(HeadingMake) ? null : HeadingMake.Trim(),
                "SUPPLIER" => string.IsNullOrWhiteSpace(HeadingSupplier) ? null : HeadingSupplier.Trim(),
                "CATA" => string.IsNullOrWhiteSpace(HeadingCategoryACode) ? null : HeadingCategoryACode.Trim(),
                "CATB" => string.IsNullOrWhiteSpace(HeadingCategoryBCode) ? null : HeadingCategoryBCode.Trim(),
                "CATC" => string.IsNullOrWhiteSpace(HeadingCategoryCCode) ? null : HeadingCategoryCCode.Trim(),
                _ => null
            };

        private string GetCategoryFilterDisplay()
            => CategoryLevel.ToUpperInvariant() switch
            {
                "A" => CategoryAName ?? CategoryACode ?? "All",
                "B" => CategoryBName ?? CategoryBCode ?? "All",
                "C" => CategoryCName ?? CategoryCCode ?? "All",
                _ => "All"
            };

        private string GetHeadingDisplay()
            => HeadingMode.ToUpperInvariant() switch
            {
                "MAKE" => HeadingMake ?? "All",
                "SUPPLIER" => HeadingSupplier ?? "All",
                "CATA" => HeadingCategoryAName ?? HeadingCategoryACode ?? "All",
                "CATB" => HeadingCategoryBName ?? HeadingCategoryBCode ?? "All",
                "CATC" => HeadingCategoryCName ?? HeadingCategoryCCode ?? "All",
                _ => "All"
            };

        private string GetHeadingLabel()
            => HeadingMode.ToUpperInvariant() switch
            {
                "MAKE" => "Make",
                "SUPPLIER" => "Supplier",
                "CATA" => "CAT A",
                "CATB" => "CAT B",
                "CATC" => "CAT C",
                _ => "Heading"
            };

        private string GetPrintByLabel()
            => IsPrintBySold ? "Sold" : "In Stock";

        private async Task ExportCsv()
        {
            if (!HasRows)
                return;

            var filename = $"major-item-report-{DateTime.Now:yyyyMMddHHmmss}.csv";
            var csv = BuildCsv(Rows ?? new());
            await JS.InvokeVoidAsync("reportExport.downloadCsv", filename, csv);
        }

        private async Task ExportPdf()
        {
            if (!HasRows)
                return;

            _branding = await CompanyBrandingService.GetAsync();
            var title = "Major Item Report";
            var html = BuildPrintHtml(title, Rows ?? new());
            await JS.InvokeVoidAsync("reportExport.printHtml", title, html);
        }

        private static string BuildCsv(List<MajorItemReportLineDto> rows)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Stock No,Part No,Make,Model,Detail,Cost,RRP,Promo,Date In,Size,Colour,Bin,Frame Number,Mfr SKU");

            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(row.StockNo),
                    Csv(row.PartNo),
                    Csv(row.Make),
                    Csv(row.Model),
                    Csv(row.Detail),
                    Csv(row.Cost.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.Rrp.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.Promo.ToString("0.00", CultureInfo.InvariantCulture)),
                    Csv(row.DateIn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                    Csv(row.Size),
                    Csv(row.Colour),
                    Csv(row.Bin),
                    Csv(row.FrameNumber),
                    Csv(row.MfrSku)));
            }

            return sb.ToString();
        }

        private string BuildPrintHtml(string title, List<MajorItemReportLineDto> rows)
        {
            var sb = new System.Text.StringBuilder();
            var fromLocation = WebUtility.HtmlEncode(GetLocationDisplay(FromLocation));
            var toLocation = WebUtility.HtmlEncode(GetLocationDisplay(ToLocation));
            var dateLabel = WebUtility.HtmlEncode(GetDateDisplay());
            var categoryFilter = WebUtility.HtmlEncode(GetCategoryFilterDisplay());
            var headingLabel = WebUtility.HtmlEncode(GetHeadingLabel());
            var headingValue = WebUtility.HtmlEncode(GetHeadingDisplay());
            var printBy = WebUtility.HtmlEncode(GetPrintByLabel());

            sb.Append(PrintTheme.BuildDocumentStart(title, true, "table{width:100%;border-collapse:collapse;table-layout:fixed;}th,td{border:1px solid #cfcfcf;padding:2px 3px;vertical-align:top;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;font-size:6px;}th{background:#f4f6f8;font-weight:bold;}.num{text-align:right;}th:nth-child(1),td:nth-child(1){width:6%;}th:nth-child(2),td:nth-child(2){width:6%;}th:nth-child(3),td:nth-child(3){width:7%;}th:nth-child(4),td:nth-child(4){width:8%;}th:nth-child(5),td:nth-child(5){width:13%;white-space:normal;}th:nth-child(6),td:nth-child(6){width:5%;}th:nth-child(7),td:nth-child(7){width:5%;}th:nth-child(8),td:nth-child(8){width:5%;}th:nth-child(9),td:nth-child(9){width:7%;}th:nth-child(10),td:nth-child(10){width:5%;}th:nth-child(11),td:nth-child(11){width:5%;}th:nth-child(12),td:nth-child(12){width:6%;}th:nth-child(13),td:nth-child(13){width:7%;}th:nth-child(14),td:nth-child(14){width:10%;}"));
            sb.Append("<div class=\"page\">");
            sb.Append(PrintTheme.BuildHeader(title, _branding, "Major item detail report"));
            sb.Append("<div class=\"meta section\">");
            sb.Append($"Print By: {printBy}<br/>");
            sb.Append($"Locations: {fromLocation} to {toLocation}<br/>");
            sb.Append($"Date: {dateLabel}<br/>");
            sb.Append($"Category Filter: CAT {WebUtility.HtmlEncode(CategoryLevel.ToUpperInvariant())} = {categoryFilter}<br/>");
            sb.Append($"Print Heading: {headingLabel} = {headingValue}");
            sb.Append("</div>");
            sb.Append("<table><thead><tr>");
            sb.Append("<th>Stock No</th><th>Part No</th><th>Make</th><th>Model</th><th>Detail</th><th class=\"num\">Cost</th><th class=\"num\">RRP</th><th class=\"num\">Promo</th><th>Date In</th><th>Size</th><th>Colour</th><th>Bin</th><th>Frame Number</th><th>Mfr SKU</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var row in rows)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.StockNo)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.PartNo)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.Make)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.Model)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.Detail)}</td>");
                sb.Append($"<td class=\"num\">{row.Cost.ToString("0.00", CultureInfo.InvariantCulture)}</td>");
                sb.Append($"<td class=\"num\">{row.Rrp.ToString("0.00", CultureInfo.InvariantCulture)}</td>");
                sb.Append($"<td class=\"num\">{row.Promo.ToString("0.00", CultureInfo.InvariantCulture)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.DateIn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture))}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.Size)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.Colour)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.Bin)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.FrameNumber)}</td>");
                sb.Append($"<td>{WebUtility.HtmlEncode(row.MfrSku)}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div>");
            sb.Append(PrintTheme.BuildDocumentEnd());
            return sb.ToString();
        }

        private string GetDateDisplay()
        {
            if (!string.IsNullOrWhiteSpace(FromDateText) && !string.IsNullOrWhiteSpace(ToDateText))
                return $"{FromDateText} to {ToDateText}";

            if (!string.IsNullOrWhiteSpace(FromDateText))
                return FromDateText!;

            if (!string.IsNullOrWhiteSpace(ToDateText))
                return ToDateText!;

            return "All";
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

        private string GetLocationDisplay(string? code)
            => DisplayLabelFormatter.FormatLocation(code, _locationLookup);

        private static string Csv(string? value)
        {
            var text = value ?? string.Empty;
            return text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r')
                ? $"\"{text.Replace("\"", "\"\"")}\""
                : text;
        }

        private static string GetToggleClass(bool active)
            => active ? "btn btn-sm btn-primary" : "btn btn-sm btn-outline-secondary";

        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
