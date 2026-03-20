using System;

namespace BackOfficeBlazor.Shared.DTOs
{
    public static class ReturnStockModes
    {
        public const string AlwaysAddToStock = "ALWAYS_ADD_TO_STOCK";
        public const string AskUserEveryReturn = "ASK_USER_EVERY_RETURN";
        public const string NeverAddAutomatically = "NEVER_ADD_AUTOMATICALLY";
    }

    public static class ReturnConditions
    {
        public const string OkSellable = "OK / Re-Sellable";
        public const string NotReSellable = "NOT / Re-Sellable";


        public static readonly string[] All =
        {
            OkSellable,
            NotReSellable
        };

        public static bool IsSellable(string? condition) =>
            string.Equals(condition?.Trim(), OkSellable, StringComparison.OrdinalIgnoreCase);
    }

    public static class ReturnStockMovementStatuses
    {
        public const string AddedToSellableStock = "Added to sellable stock";
        public const string NotAddedToStock = "Return recorded - not added to stock";
        public const string MovedToFaultyStock = "Moved to faulty stock";
    }
}
