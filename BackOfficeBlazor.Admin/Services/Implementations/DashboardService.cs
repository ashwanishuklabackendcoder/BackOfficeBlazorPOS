using System.Globalization;
using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private static readonly string[] DashboardPalette =
        {
            "#2563eb",
            "#f97316",
            "#10b981",
            "#8b5cf6",
            "#ef4444",
            "#14b8a6",
            "#64748b",
            "#d97706"
        };

        private readonly BackOfficeAdminContext _context;

        public DashboardService(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(DashboardRequestDto request)
        {
            request ??= new DashboardRequestDto();

            var currentRange = ResolveCurrentRange(request);
            var todayRange = ResolveTodayRange();
            var previousCurrentRange = ResolvePreviousRange(currentRange.From, currentRange.ToExclusive, request.Preset);
            var previousTodayRange = ResolvePreviousRange(todayRange.From, todayRange.ToExclusive, DashboardDatePreset.Today);

            var selectedLocations = NormalizeLocationCodes(request.Locations);
            var selectedTerminal = string.IsNullOrWhiteSpace(request.TerminalCode)
                ? null
                : request.TerminalCode.Trim();

            var salesBase = BuildSalesQuery(selectedLocations, selectedTerminal);
            var returnsBase = BuildReturnsQuery(selectedLocations, selectedTerminal);

            var currentSales = salesBase.Where(x => x.DateAndTime != null && x.DateAndTime >= currentRange.From && x.DateAndTime < currentRange.ToExclusive);
            var previousSales = salesBase.Where(x => x.DateAndTime != null && x.DateAndTime >= previousCurrentRange.From && x.DateAndTime < previousCurrentRange.ToExclusive);
            var todaySales = salesBase.Where(x => x.DateAndTime != null && x.DateAndTime >= todayRange.From && x.DateAndTime < todayRange.ToExclusive);
            var previousTodaySales = salesBase.Where(x => x.DateAndTime != null && x.DateAndTime >= previousTodayRange.From && x.DateAndTime < previousTodayRange.ToExclusive);

            var currentReturns = returnsBase.Where(x => x.DateAndTime != null && x.DateAndTime >= currentRange.From && x.DateAndTime < currentRange.ToExclusive);
            var previousReturns = returnsBase.Where(x => x.DateAndTime != null && x.DateAndTime >= previousCurrentRange.From && x.DateAndTime < previousCurrentRange.ToExclusive);
            var todayReturns = returnsBase.Where(x => x.DateAndTime != null && x.DateAndTime >= todayRange.From && x.DateAndTime < todayRange.ToExclusive);
            var previousTodayReturns = returnsBase.Where(x => x.DateAndTime != null && x.DateAndTime >= previousTodayRange.From && x.DateAndTime < previousTodayRange.ToExclusive);

            var currentSalesTotal = await SumCurrencyAsync(currentSales.Select(x => x.Net));
            var previousSalesTotal = await SumCurrencyAsync(previousSales.Select(x => x.Net));
            var todaySalesTotal = await SumCurrencyAsync(todaySales.Select(x => x.Net));
            var previousTodaySalesTotal = await SumCurrencyAsync(previousTodaySales.Select(x => x.Net));

            var currentReturnsTotal = await SumCurrencyAsync(currentReturns.Select(x => -x.Net));
            var previousReturnsTotal = await SumCurrencyAsync(previousReturns.Select(x => -x.Net));
            var todayReturnsTotal = await SumCurrencyAsync(todayReturns.Select(x => -x.Net));
            var previousTodayReturnsTotal = await SumCurrencyAsync(previousTodayReturns.Select(x => -x.Net));

            var invoiceCount = await currentSales
                .Select(x => x.InvoiceNumber)
                .Distinct()
                .CountAsync();

            var currentSalesTrend = await BuildSalesTrendAsync(currentSales, currentRange);
            var salesVsReturns = BuildSalesVsReturns(currentSalesTotal, currentReturnsTotal);
            var paymentMethods = await BuildPaymentMethodsAsync(currentSales);
            var topCategories = await BuildTopCategoriesAsync(currentSales);

            var netSales = Math.Max(0m, currentSalesTotal - currentReturnsTotal);
            var averageTicket = invoiceCount > 0 ? currentSalesTotal / invoiceCount : 0m;
            var returnRate = currentSalesTotal > 0m ? (currentReturnsTotal / currentSalesTotal) * 100m : 0m;

            var peakDay = currentSalesTrend
                .Where(x => x.Value > 0m)
                .OrderByDescending(x => x.Value)
                .FirstOrDefault();

            var topCategory = topCategories.FirstOrDefault(x => x.Value > 0m);
            var topPayment = paymentMethods
                .Where(x => x.Value > 0m)
                .OrderByDescending(x => x.Value)
                .FirstOrDefault();

            var response = new DashboardResponseDto
            {
                GeneratedAtUtc = DateTime.UtcNow,
                CurrentRange = new DashboardRangeDto
                {
                    Preset = request.Preset,
                    Label = BuildRangeLabel(request.Preset, currentRange.From, currentRange.ToExclusive),
                    From = currentRange.From,
                    To = currentRange.ToExclusive.AddTicks(-1)
                },
                TodayRange = new DashboardRangeDto
                {
                    Preset = DashboardDatePreset.Today,
                    Label = "Today",
                    From = todayRange.From,
                    To = todayRange.ToExclusive.AddTicks(-1)
                },
                Kpis = new List<DashboardKpiDto>
                {
                    BuildKpi("sales-current", "Total Sales", currentRange.Label, currentSalesTotal, previousSalesTotal, previousCurrentRange.Label),
                    BuildKpi("sales-today", "Total Sales Today", "Today", todaySalesTotal, previousTodaySalesTotal, "yesterday"),
                    BuildKpi("returns-current", "Total Returns", currentRange.Label, currentReturnsTotal, previousReturnsTotal, previousCurrentRange.Label),
                    BuildKpi("returns-today", "Total Returns Today", "Today", todayReturnsTotal, previousTodayReturnsTotal, "yesterday")
                },
                SalesTrend = currentSalesTrend,
                SalesVsReturns = salesVsReturns,
                PaymentMethods = paymentMethods,
                TopCategories = topCategories,
                Summary = BuildSummaryItems(netSales, invoiceCount, averageTicket, returnRate, peakDay, topCategory, topPayment, currentRange.Label),
                Locations = await BuildLocationOptionsAsync(selectedLocations)
            };

            return response;
        }

        private IQueryable<FTT05> BuildSalesQuery(IReadOnlyCollection<string> locations, string? terminalCode)
        {
            var query = _context.FTT05.AsNoTracking()
                .Where(x => x.InOut == "O" && x.DateAndTime != null);

            if (locations.Count > 0)
            {
                var locationCodes = locations.ToArray();
                query = query.Where(x => locationCodes.Contains(x.Location));
            }

            if (!string.IsNullOrWhiteSpace(terminalCode))
            {
                var terminal = terminalCode.Trim();
                query = query.Where(x => x.Terminal == terminal);
            }

            return query;
        }

        private IQueryable<FTT05> BuildReturnsQuery(IReadOnlyCollection<string> locations, string? terminalCode)
        {
            var query = _context.FTT05.AsNoTracking()
                .Where(x => x.InOut == "R" && x.DateAndTime != null);

            if (locations.Count > 0)
            {
                var locationCodes = locations.ToArray();
                query = query.Where(x => locationCodes.Contains(x.Location));
            }

            if (!string.IsNullOrWhiteSpace(terminalCode))
            {
                var terminal = terminalCode.Trim();
                query = query.Where(x => x.Terminal == terminal);
            }

            return query;
        }

        private static async Task<decimal> SumCurrencyAsync(IQueryable<decimal> query)
            => await query.Select(x => (decimal?)x).SumAsync() ?? 0m;

        private static async Task<List<DashboardTrendPointDto>> BuildSalesTrendAsync(IQueryable<FTT05> sales, ResolvedRange range)
        {
            var grouped = await sales
                .GroupBy(x => x.DateAndTime!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Value = g.Sum(x => x.Net)
                })
                .ToListAsync();

            var map = grouped.ToDictionary(x => x.Date, x => x.Value);
            var points = new List<DashboardTrendPointDto>();

            for (var date = range.From.Date; date < range.ToExclusive.Date; date = date.AddDays(1))
            {
                map.TryGetValue(date, out var value);
                points.Add(new DashboardTrendPointDto
                {
                    Date = date,
                    Label = date.Day.ToString(),
                    Value = value
                });
            }

            return points;
        }

        private static List<DashboardDistributionItemDto> BuildSalesVsReturns(decimal sales, decimal returns)
        {
            var total = sales + returns;
            if (total <= 0m)
            {
                return new List<DashboardDistributionItemDto>
                {
                    new() { Label = "Sales", Value = 0m, Percentage = 0m, Color = DashboardPalette[0] },
                    new() { Label = "Returns", Value = 0m, Percentage = 0m, Color = DashboardPalette[4] }
                };
            }

            return new List<DashboardDistributionItemDto>
            {
                new()
                {
                    Label = "Sales",
                    Value = sales,
                    Percentage = Math.Round((sales / total) * 100m, 2),
                    Color = DashboardPalette[0]
                },
                new()
                {
                    Label = "Returns",
                    Value = returns,
                    Percentage = Math.Round((returns / total) * 100m, 2),
                    Color = DashboardPalette[4]
                }
            };
        }

        private async Task<List<DashboardDistributionItemDto>> BuildPaymentMethodsAsync(
            IQueryable<FTT05> currentSales)
        {
            var invoiceNumbers = currentSales
                .Select(x => x.InvoiceNumber)
                .Distinct();

            var payments = _context.FTT11.AsNoTracking()
                .Where(x => invoiceNumbers.Contains(x.InvoiceNumber) && x.Amount > 0m);

            var grouped = await payments
                .Select(x => new
                {
                    x.Amount,
                    Bucket = x.Cash > 0m && x.Cheque == 0m && x.Type3 == 0m && x.Type4 == 0m && x.Credit == 0m
                        ? "Cash"
                        : x.Cheque > 0m && x.Cash == 0m && x.Type3 == 0m && x.Type4 == 0m && x.Credit == 0m
                            ? "Cheque"
                            : (x.Type3 > 0m || x.Type4 > 0m) && x.Cash == 0m && x.Cheque == 0m && x.Credit == 0m
                                ? "Card"
                                : x.Credit > 0m && x.Cash == 0m && x.Cheque == 0m && x.Type3 == 0m && x.Type4 == 0m
                                    ? "Credit"
                                    : "Mixed"
                })
                .GroupBy(x => x.Bucket)
                .Select(g => new
                {
                    Bucket = g.Key,
                    Value = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            var total = grouped.Sum(x => x.Value);
            var order = new[] { "Cash", "Card", "Cheque", "Credit", "Mixed" };
            var colorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Cash"] = DashboardPalette[2],
                ["Card"] = DashboardPalette[0],
                ["Cheque"] = DashboardPalette[1],
                ["Credit"] = DashboardPalette[3],
                ["Mixed"] = DashboardPalette[4]
            };

            return order
                .Select(bucket =>
                {
                    var value = grouped.FirstOrDefault(x => x.Bucket == bucket)?.Value ?? 0m;
                    return new DashboardDistributionItemDto
                    {
                        Label = bucket,
                        Value = value,
                        Percentage = total > 0m ? Math.Round((value / total) * 100m, 2) : 0m,
                        Color = colorMap[bucket]
                    };
                })
                .Where(x => x.Value > 0m || x.Label == "Mixed")
                .ToList();
        }

        private async Task<List<DashboardBarItemDto>> BuildTopCategoriesAsync(IQueryable<FTT05> currentSales)
        {
            var categoryRows = await
                (from sale in currentSales
                 where sale.Sell > 0m
                    && string.IsNullOrWhiteSpace(sale.DiscountCode)
                 join product in _context.ProductItems.AsNoTracking()
                    on sale.PartNumber equals product.PartNumber into productGroup
                 from product in productGroup.DefaultIfEmpty()
                 select new
                 {
                     Category = product != null && !string.IsNullOrWhiteSpace(product.CatA)
                         ? product.CatA!
                         : "Uncategorized",
                     sale.Net
                 })
                .GroupBy(x => x.Category)
                .Select(g => new DashboardBarItemDto
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.Net)
                })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToListAsync();

            var total = categoryRows.Sum(x => x.Value);
            foreach (var row in categoryRows)
            {
                row.Percentage = total > 0m ? Math.Round((row.Value / total) * 100m, 2) : 0m;
            }

            return categoryRows;
        }

        private async Task<List<DashboardLocationOptionDto>> BuildLocationOptionsAsync(IReadOnlyCollection<string> selectedLocations)
        {
            var allLocations = await _context._Locations
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted && x.KeyLocation)
                .Select(x => new DashboardLocationOptionDto
                {
                    Code = x.Code,
                    Name = x.Name,
                    Selected = selectedLocations.Count == 0 || selectedLocations.Contains(x.Code)
                })
                .ToListAsync();

            return allLocations
                .OrderBy(x => GetLocationSortKey(x.Code))
                .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int GetLocationSortKey(string? code)
            => int.TryParse(code?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : int.MaxValue;

        private static DashboardKpiDto BuildKpi(
            string key,
            string title,
            string subtitle,
            decimal current,
            decimal previous,
            string comparisonLabel)
        {
            var delta = current - previous;
            var direction = delta > 0m
                ? DashboardTrendDirection.Up
                : delta < 0m
                    ? DashboardTrendDirection.Down
                    : DashboardTrendDirection.Flat;

            var deltaPercent = previous == 0m
                ? (current == 0m ? 0m : 100m)
                : Math.Round((delta / previous) * 100m, 2);

            return new DashboardKpiDto
            {
                Key = key,
                Title = title,
                Subtitle = subtitle,
                ComparisonLabel = comparisonLabel,
                Value = current,
                PreviousValue = previous,
                Delta = delta,
                DeltaPercent = deltaPercent,
                TrendDirection = direction,
                IsPositiveTrend = delta >= 0m
            };
        }

        private static List<DashboardSummaryItemDto> BuildSummaryItems(
            decimal netSales,
            int invoiceCount,
            decimal averageTicket,
            decimal returnRate,
            DashboardTrendPointDto? peakDay,
            DashboardBarItemDto? topCategory,
            DashboardDistributionItemDto? topPayment,
            string currentPeriodLabel)
        {
            var items = new List<DashboardSummaryItemDto>
            {
                new()
                {
                    Label = "Net Sales",
                    Value = FormatPound(netSales),
                    Kind = DashboardValueKind.Currency,
                    Subtext = $"Selected period: {currentPeriodLabel}"
                },
                new()
                {
                    Label = "Invoices",
                    Value = invoiceCount.ToString("N0"),
                    Kind = DashboardValueKind.Count
                },
                new()
                {
                    Label = "Average Ticket",
                    Value = FormatPound(averageTicket),
                    Kind = DashboardValueKind.Currency
                },
                new()
                {
                    Label = "Return Rate",
                    Value = returnRate.ToString("0.##") + "%",
                    Kind = DashboardValueKind.Percent
                }
            };

            if (peakDay != null)
            {
                items.Add(new DashboardSummaryItemDto
                {
                    Label = "Peak Day",
                    Value = peakDay.Label,
                    Kind = DashboardValueKind.Text,
                    Subtext = FormatPound(peakDay.Value)
                });
            }

            if (topCategory != null)
            {
                items.Add(new DashboardSummaryItemDto
                {
                    Label = "Top Category",
                    Value = topCategory.Label,
                    Kind = DashboardValueKind.Text,
                    Subtext = FormatPound(topCategory.Value)
                });
            }

            if (topPayment != null)
            {
                items.Add(new DashboardSummaryItemDto
                {
                    Label = "Top Payment",
                    Value = topPayment.Label,
                    Kind = DashboardValueKind.Text,
                    Subtext = FormatPound(topPayment.Value)
                });
            }

            return items;
        }

        private static ResolvedRange ResolveCurrentRange(DashboardRequestDto request)
        {
            var now = DateTime.Now;
            var todayStart = now.Date;

            return request.Preset switch
            {
                DashboardDatePreset.Today => new ResolvedRange(
                    DashboardDatePreset.Today,
                    "Today",
                    todayStart,
                    todayStart.AddDays(1)),
                DashboardDatePreset.ThisWeek => new ResolvedRange(
                    DashboardDatePreset.ThisWeek,
                    "This Week",
                    todayStart.AddDays(-(int)todayStart.DayOfWeek),
                    todayStart.AddDays(-(int)todayStart.DayOfWeek).AddDays(7)),
                DashboardDatePreset.Custom => ResolveCustomRange(request),
                _ => new ResolvedRange(
                    DashboardDatePreset.ThisMonth,
                    "This Month",
                    new DateTime(now.Year, now.Month, 1),
                    new DateTime(now.Year, now.Month, 1).AddMonths(1))
            };
        }

        private static ResolvedRange ResolveTodayRange()
        {
            var todayStart = DateTime.Now.Date;
            return new ResolvedRange(DashboardDatePreset.Today, "Today", todayStart, todayStart.AddDays(1));
        }

        private static ResolvedRange ResolveCustomRange(DashboardRequestDto request)
        {
            var from = request.From?.Date ?? DateTime.Now.Date;
            var to = request.To?.Date ?? from;
            if (to < from)
                (from, to) = (to, from);

            return new ResolvedRange(
                DashboardDatePreset.Custom,
                $"{from:dd MMM yyyy} - {to:dd MMM yyyy}",
                from,
                to.AddDays(1));
        }

        private static ResolvedRange ResolvePreviousRange(DateTime from, DateTime toExclusive, DashboardDatePreset preset)
        {
            if (preset == DashboardDatePreset.ThisMonth)
            {
                var previousStart = new DateTime(from.Year, from.Month, 1).AddMonths(-1);
                return new ResolvedRange(
                    DashboardDatePreset.ThisMonth,
                    "previous month",
                    previousStart,
                    previousStart.AddMonths(1));
            }

            var duration = toExclusive - from;
            var previousFrom = from - duration;
            return new ResolvedRange(
                preset,
                preset switch
                {
                    DashboardDatePreset.Today => "yesterday",
                    DashboardDatePreset.ThisWeek => "previous week",
                    _ => "previous period"
                },
                previousFrom,
                from);
        }

        private static string BuildRangeLabel(DashboardDatePreset preset, DateTime from, DateTime toExclusive)
        {
            return preset switch
            {
                DashboardDatePreset.Today => "Today",
                DashboardDatePreset.ThisWeek => "This Week",
                DashboardDatePreset.ThisMonth => "This Month",
                DashboardDatePreset.Custom => $"{from:dd MMM yyyy} - {toExclusive.AddDays(-1):dd MMM yyyy}",
                _ => "Selected Period"
            };
        }

        private static IReadOnlyCollection<string> NormalizeLocationCodes(IEnumerable<string>? locations)
            => locations?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
               ?? Array.Empty<string>();

        private sealed record ResolvedRange(
            DashboardDatePreset Preset,
            string Label,
            DateTime From,
            DateTime ToExclusive);

        private static string FormatPound(decimal value)
            => $"£{value:N2}";
    }
}
