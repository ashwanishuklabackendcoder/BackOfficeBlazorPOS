namespace BackOfficeBlazor.Admin.Services.Interfaces
{
    public interface IPrintAgentPresenceService
    {
        bool IsLocationOnline(string locationCode);
    }
}
