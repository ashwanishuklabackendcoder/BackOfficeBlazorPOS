using BackOfficeBlazor.Admin.Context;
using BackOfficeBlazor.Admin.Entities;
using BackOfficeBlazor.Admin.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeBlazor.Admin.Repository.Implementations
{
    public class TerminalOptionRepository : ITerminalOptionRepository
    {
        private readonly BackOfficeAdminContext _context;

        public TerminalOptionRepository(BackOfficeAdminContext context)
        {
            _context = context;
        }

        public Task<TerminalOption?> GetByTerminalCodeAsync(string terminalCode)
            => _context.TerminalOptions.FirstOrDefaultAsync(x => x.TerminalCode == terminalCode.Trim());

        public async Task<List<string>> GetTerminalCodesAsync(string? defaultBranch = null)
        {
            var query = _context.TerminalOptions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(defaultBranch))
                query = query.Where(x => x.DefaultBranch == defaultBranch);

            return await query
                .Select(x => x.TerminalCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task EnsureTerminalExistsAsync(string terminalCode, string defaultBranch)
        {
            terminalCode = terminalCode.Trim();
            defaultBranch = defaultBranch.Trim();

            var exists = await _context.TerminalOptions
                .AnyAsync(x => x.TerminalCode == terminalCode);
            if (exists)
                return;

            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText =
                    """
                    INSERT INTO dbo.TerminalOptions
                    (
                        TerminalCode, Enabled, RemoteTerminal, DefaultBranch, Currency,
                        AllowNegativeStock, PrintInvoice, PrintReceipt, EReceipt, CashDrawer,
                        ReceiptCutter, LogReferal, ShowMarginLabels, ShowCost, UsePromoPriceInStore,
                        AutoLogoutTimeout, AutoLogout, PrintCompanyRegistration, PrintVatNo, PrintBranchEmail,
                        PrintVatAmount, PrintAmountPaid, PrintCustomerDetails, PrintSizeColour, PrintSoldTicket,
                        PrintPoints, PrintBinLocation, PrintTotalSaved, AskEnterCustomerMin, AskEnterSerial,
                        AskReceipt, AskCollectionDateMajorSale, AllowDiscount, AllowDiscountWithSuperPassword,
                        AllowPriceChange, AllowPartNoRepeatOnSale, PaymentSenseEnabled, ReceiptPrinterId, PSConnectId,
                        LabelPrinterId, A4PrinterId
                    )
                    VALUES
                    (
                        @terminalCode, 1, 0, @defaultBranch, N'GBP',
                        0, 1, 1, 0, 0,
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0, 0,
                        NULL, NULL
                    );
                    """;

                var p1 = command.CreateParameter();
                p1.ParameterName = "@terminalCode";
                p1.Value = terminalCode;
                command.Parameters.Add(p1);

                var p2 = command.CreateParameter();
                p2.ParameterName = "@defaultBranch";
                p2.Value = defaultBranch;
                command.Parameters.Add(p2);

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        public async Task UpsertAsync(TerminalOption option)
        {
            var existing = await _context.TerminalOptions
                .FirstOrDefaultAsync(x => x.TerminalCode == option.TerminalCode.Trim());

            if (existing == null)
                return;

            existing.ReceiptPrinterId = option.ReceiptPrinterId;
            existing.A4PrinterId = option.A4PrinterId;
            existing.LabelPrinterId = option.LabelPrinterId;
        }

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
