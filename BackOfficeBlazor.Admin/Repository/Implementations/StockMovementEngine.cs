using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class StockMovementEngine : IStockMovementEngine
    {
        private readonly BackOfficeAdminContext _context;

        public StockMovementEngine(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public async Task SellMajorAsync(string partNumber, string stockNumber, string location)
        {
            ValidateSerialInput(partNumber, stockNumber);
            await ToggleSerialAvailabilityAsync(partNumber, stockNumber, false, location);
            await AdjustLevelAsync(partNumber, location, -1);
        }

        public async Task SellMinorAsync(string partNumber, int qty, string location)
        {
            ValidateQuantity(qty);
            await AdjustLevelAsync(partNumber, location, -qty);
        }

        public async Task ReturnMajorAsync(string partNumber, string stockNumber, string location)
        {
            ValidateSerialInput(partNumber, stockNumber);
            await ToggleSerialAvailabilityAsync(partNumber, stockNumber, true, location);
            await AdjustLevelAsync(partNumber, location, 1);
        }

        public async Task ReturnMinorAsync(string partNumber, int qty, string location)
        {
            ValidateQuantity(qty);
            await AdjustLevelAsync(partNumber, location, qty);
        }

        private static void ValidateSerialInput(string partNumber, string stockNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                throw new ArgumentException("Part number is required for serialized movement.", nameof(partNumber));

            if (string.IsNullOrWhiteSpace(stockNumber))
                throw new ArgumentException("Stock number is required for serialized movement.", nameof(stockNumber));
        }

        private static void ValidateQuantity(int qty)
        {
            if (qty <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(qty));
        }

        private async Task ToggleSerialAvailabilityAsync(string partNumber, string stockNumber, bool isAvailable, string location)
        {
            var normalizedLocation = NormalizeLocation(location);
            var serial = await _context._ProductsStock
                .FirstOrDefaultAsync(x =>
                    x.PartNumber == partNumber &&
                    x.StockNumber == stockNumber &&
                    x.LocationCode == normalizedLocation);

            if (serial == null)
                throw new InvalidOperationException($"Serial {stockNumber} not found for {partNumber} at {normalizedLocation}.");

            serial.IsAvailable = isAvailable;
        }

        private async Task AdjustLevelAsync(string partNumber, string location, int delta)
        {
            if (delta == 0)
                return;

            var normalizedLocation = NormalizeLocation(location);
            var level = await _context._ProductsStockLevels
                .FirstOrDefaultAsync(x => x.PartNumber == partNumber);

            if (level == null)
                throw new InvalidOperationException($"Stock level row not found for {partNumber}.");

            var propertyName = $"L{normalizedLocation}";
            var prop = typeof(StockLevels).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
                throw new InvalidOperationException($"Location column {propertyName} does not exist.");

            var current = (int)prop.GetValue(level)!;
            var updated = current + delta;

            if (updated < 0)
                throw new InvalidOperationException($"Stock level underflow for {partNumber} at L{normalizedLocation}.");

            prop.SetValue(level, updated);
            level.DateUpdated = DateTime.UtcNow;
        }

        private static string NormalizeLocation(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location code is required for stock movement.", nameof(location));

            var trimmed = location.Trim();

            if (trimmed.Length == 1)
                trimmed = $"0{trimmed}";
            else if (trimmed.Length > 2)
                trimmed = trimmed.Substring(trimmed.Length - 2);

            return trimmed.ToUpperInvariant();
        }
    }
}
