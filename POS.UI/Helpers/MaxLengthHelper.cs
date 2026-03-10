using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace POS.UI.Helpers
{
    public static class MaxLengthHelper
    {
        public static int? For<T>(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return null;

            var stringLength = prop.GetCustomAttribute<StringLengthAttribute>();
            if (stringLength != null)
                return stringLength.MaximumLength;

            var maxLength = prop.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLength != null)
                return maxLength.Length;

            return null;
        }
    }
}
