using BackOfficeBlazor.Admin.Entities;

namespace BackOfficeBlazor.Admin.Repository.Interfaces
{
    public interface ITerminalOptionRepository
    {
        Task<TerminalOption?> GetByTerminalCodeAsync(string terminalCode);
        Task<List<string>> GetTerminalCodesAsync(string? defaultBranch = null);
        Task EnsureTerminalExistsAsync(string terminalCode, string defaultBranch);
        Task UpsertAsync(TerminalOption option);
        Task SaveChangesAsync();
    }
}
