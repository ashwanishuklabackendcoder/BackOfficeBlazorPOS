using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Linq;
using System.Reflection;

namespace BackOfficeBlazor.Admin.Services
{
    internal static class SequenceHelper
    {
        public const int DefaultDigits = 5;

        public static int GetMaxLength<T>(string propertyName, int fallbackLength)
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            var attribute = property?
                .GetCustomAttribute<StringLengthAttribute>(inherit: true);

            if (attribute != null && attribute.MaximumLength > 0)
                return attribute.MaximumLength;

            return fallbackLength > 0 ? fallbackLength : 1;
        }

        public static string GenerateNextFiveDigitCode(string? lastValue) =>
            GenerateNext(lastValue, DefaultDigits);

        public static string GenerateNext(string? lastValue, int maxLength)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            var candidate = lastValue?.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(candidate))
                return GenerateInitial(maxLength);

            if (candidate.All(char.IsDigit))
            {
                return IncrementDigits(candidate, maxLength);
            }

            if (candidate.Length > 1 && char.IsLetter(candidate[0]))
            {
                var prefix = candidate[0];
                var numericPart = candidate.Substring(1);

                if (!string.IsNullOrEmpty(numericPart) && numericPart.All(char.IsDigit))
                {
                    var digitWidth = Math.Min(maxLength - 1, Math.Max(numericPart.Length, 1));
                    var nextNumeric = IncrementDigits(numericPart, digitWidth);
                    return $"{prefix}{nextNumeric}";
                }
            }

            return GenerateInitial(maxLength);
        }

        private static string GenerateInitial(int maxLength)
        {
            if (maxLength <= 1)
                return "1";

            return new string('0', maxLength - 1) + "1";
        }

        private static string IncrementDigits(string digits, int width)
        {
            if (width < 1)
                width = Math.Max(digits.Length, 1);

            var normalized = digits.PadLeft(width, '0');
            var value = BigInteger.Parse(normalized);
            value += BigInteger.One;
            var next = value.ToString();

            if (next.Length > width)
                return new string('0', Math.Max(width - 1, 0)) + "1";

            return next.PadLeft(width, '0');
        }

        public static string? GetHighestNumericCode(IEnumerable<string?> values)
        {
            return values
                .Select(v => v?.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!)
                .Where(v => v.All(char.IsDigit))
                .OrderByDescending(v => v.Length)
                .ThenByDescending(v => v, StringComparer.Ordinal)
                .FirstOrDefault();
        }
    }
}
