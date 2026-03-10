using BackOfficeBlazor.Shared.DTOs;

namespace POSAPI.Services
{
    public interface IPrintJobDispatcher
    {
        Task DispatchAsync(PrintJobDto job);
    }
}
