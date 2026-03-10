using System.Collections.Concurrent;
using BackOfficeBlazor.Admin.Services.Interfaces;

namespace POSAPI.Services
{
    public class PrintAgentPresenceService : IPrintAgentPresenceService
    {
        private readonly ConcurrentDictionary<string, string> _connectionByTerminal = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, string> _locationByTerminal = new(StringComparer.OrdinalIgnoreCase);

        public void Register(string terminalCode, string locationCode, string connectionId)
        {
            _connectionByTerminal[terminalCode] = connectionId;
            _locationByTerminal[terminalCode] = locationCode;
        }

        public void UnregisterByConnection(string connectionId)
        {
            var target = _connectionByTerminal.FirstOrDefault(x => x.Value == connectionId);
            if (string.IsNullOrWhiteSpace(target.Key))
                return;

            _connectionByTerminal.TryRemove(target.Key, out _);
            _locationByTerminal.TryRemove(target.Key, out _);
        }

        public bool IsTerminalOnline(string terminalCode)
            => _connectionByTerminal.ContainsKey(terminalCode);

        public bool IsLocationOnline(string locationCode)
            => _locationByTerminal.Any(x => string.Equals(x.Value, locationCode, StringComparison.OrdinalIgnoreCase));
    }
}
