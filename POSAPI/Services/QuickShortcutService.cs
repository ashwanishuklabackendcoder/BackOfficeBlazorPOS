using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace POSAPI.Services;

public interface IQuickShortcutService
{
    Task<ApiResponse<List<QuickShortcutItemDto>>> GetAdminAsync();
    Task<ApiResponse<bool>> SaveAdminAsync(List<QuickShortcutItemDto> items);
    Task<ApiResponse<List<QuickShortcutItemDto>>> GetForPosAsync(string locationCode);
}

public class QuickShortcutService : IQuickShortcutService
{
    private readonly BackOfficeAdminContext _db;

    public QuickShortcutService(BackOfficeAdminContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<List<QuickShortcutItemDto>>> GetAdminAsync()
    {
        var items = await _db.QuickShortcutItems
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .Select(x => new QuickShortcutItemDto
            {
                Id = x.Id,
                PartNumber = x.PartNumber,
                Title = x.Title,
                SubTitle = x.SubTitle,
                ColorHex = x.ColorHex,
                ForeColorHex = x.ForeColorHex,
                ShowAvailableStock = x.ShowAvailableStock,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync();

        foreach (var item in items)
        {
            item.ColorHex = NormalizeColor(item.ColorHex);
            item.ForeColorHex = NormalizeColor(item.ForeColorHex, "#212529");
        }

        return ApiResponse<List<QuickShortcutItemDto>>.Ok(items);
    }

    public async Task<ApiResponse<bool>> SaveAdminAsync(List<QuickShortcutItemDto> items)
    {
        items ??= new List<QuickShortcutItemDto>();

        var normalized = items
            .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
            .Select((x, idx) => new QuickShortcutItem
            {
                PartNumber = x.PartNumber.Trim().ToUpperInvariant(),
                Title = string.IsNullOrWhiteSpace(x.Title) ? x.PartNumber.Trim().ToUpperInvariant() : x.Title.Trim(),
                SubTitle = string.IsNullOrWhiteSpace(x.SubTitle) ? null : x.SubTitle.Trim(),
                ColorHex = NormalizeColor(x.ColorHex),
                ForeColorHex = NormalizeColor(x.ForeColorHex, "#212529"),
                ShowAvailableStock = x.ShowAvailableStock,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder <= 0 ? idx + 1 : x.DisplayOrder
            })
            .GroupBy(x => x.PartNumber, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        // Validate part numbers exist in product master.
        var partNumbers = normalized.Select(x => x.PartNumber).ToList();
        if (partNumbers.Count > 0)
        {
            var existingParts = await _db.ProductItems
                .AsNoTracking()
                .Where(p => partNumbers.Contains(p.PartNumber))
                .Select(p => p.PartNumber)
                .ToListAsync();

            var missing = partNumbers
                .Except(existingParts, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (missing.Count > 0)
                return ApiResponse<bool>.Fail($"Invalid product(s): {string.Join(", ", missing)}");
        }

        var current = await _db.QuickShortcutItems.ToListAsync();
        _db.QuickShortcutItems.RemoveRange(current);
        await _db.QuickShortcutItems.AddRangeAsync(normalized);
        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Quick items saved");
    }

    public async Task<ApiResponse<List<QuickShortcutItemDto>>> GetForPosAsync(string locationCode)
    {
        var loc = NormalizeLocation(locationCode);

        var items = await _db.QuickShortcutItems
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

        if (items.Count == 0)
            return ApiResponse<List<QuickShortcutItemDto>>.Ok(new List<QuickShortcutItemDto>());

        var partNumbers = items.Select(x => x.PartNumber).Distinct().ToList();
        var productMap = await _db.ProductItems
            .AsNoTracking()
            .Where(x => partNumbers.Contains(x.PartNumber))
            .ToDictionaryAsync(
                x => x.PartNumber,
                x => new { x.Details, x.Make, x.Search1, x.CostPrice, x.StorePrice, x.WebPrice },
                StringComparer.OrdinalIgnoreCase);

        var stockRows = await _db._ProductsStockLevels
            .AsNoTracking()
            .Where(x => partNumbers.Contains(x.PartNumber))
            .ToListAsync();

        var stockMap = stockRows.ToDictionary(
            x => x.PartNumber,
            x => GetLocationStock(x, loc),
            StringComparer.OrdinalIgnoreCase);

        var result = new List<QuickShortcutItemDto>(items.Count);
        foreach (var item in items)
        {
            productMap.TryGetValue(item.PartNumber, out var product);
            stockMap.TryGetValue(item.PartNumber, out var stock);

            result.Add(new QuickShortcutItemDto
            {
                Id = item.Id,
                PartNumber = item.PartNumber,
                Title = string.IsNullOrWhiteSpace(item.Title)
                    ? (product?.Details ?? item.PartNumber)
                    : item.Title,
                SubTitle = string.IsNullOrWhiteSpace(item.SubTitle)
                    ? (product?.Make ?? "")
                    : item.SubTitle,
                ColorHex = NormalizeColor(item.ColorHex),
                ForeColorHex = NormalizeColor(item.ForeColorHex, "#212529"),
                ShowAvailableStock = item.ShowAvailableStock,
                IsActive = item.IsActive,
                DisplayOrder = item.DisplayOrder,
                CurrentLocationStock = item.ShowAvailableStock ? stock : null
            });
        }

        return ApiResponse<List<QuickShortcutItemDto>>.Ok(result);
    }

    private static string NormalizeColor(string? color, string fallback = "#f8f9fa")
    {
        if (string.IsNullOrWhiteSpace(color))
            return fallback;

        var v = color.Trim();
        if (!v.StartsWith("#"))
            v = "#" + v;

        // support #RGB and #RRGGBB
        if (v.Length == 4 || v.Length == 7)
            return v;

        return fallback;
    }

    private static int NormalizeLocation(string? locationCode)
    {
        if (string.IsNullOrWhiteSpace(locationCode))
            return 1;

        if (int.TryParse(locationCode, out var parsed))
        {
            if (parsed < 1) return 1;
            if (parsed > 30) return 30;
            return parsed;
        }

        return 1;
    }

    private static int GetLocationStock(StockLevels levels, int location)
        => location switch
        {
            1 => levels.L01,
            2 => levels.L02,
            3 => levels.L03,
            4 => levels.L04,
            5 => levels.L05,
            6 => levels.L06,
            7 => levels.L07,
            8 => levels.L08,
            9 => levels.L09,
            10 => levels.L10,
            11 => levels.L11,
            12 => levels.L12,
            13 => levels.L13,
            14 => levels.L14,
            15 => levels.L15,
            16 => levels.L16,
            17 => levels.L17,
            18 => levels.L18,
            19 => levels.L19,
            20 => levels.L20,
            21 => levels.L21,
            22 => levels.L22,
            23 => levels.L23,
            24 => levels.L24,
            25 => levels.L25,
            26 => levels.L26,
            27 => levels.L27,
            28 => levels.L28,
            29 => levels.L29,
            30 => levels.L30,
            _ => levels.L01
        };
}
