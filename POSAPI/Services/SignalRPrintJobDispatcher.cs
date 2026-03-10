using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;
using POSAPI.Hubs;

namespace POSAPI.Services
{
    public class SignalRPrintJobDispatcher : IPrintJobDispatcher
    {
        private readonly IHubContext<PrintHub> _hub;

        public SignalRPrintJobDispatcher(IHubContext<PrintHub> hub)
        {
            _hub = hub;
        }

        public Task DispatchAsync(PrintJobDto job)
            => _hub.Clients.Group(PrintHub.TerminalGroup(job.TerminalCode))
                .SendAsync("ReceivePrintJob", job);
    }
}
