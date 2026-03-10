namespace POS.UI.Services
{
    public class AppSnackbarService
    {
        public event Action? OnChange;

        public List<SnackbarMessage> Messages { get; } = new();

        private readonly object _sync = new();
        private readonly Queue<SnackbarMessage> _queue = new();
        private bool _workerRunning;

        public Task Show(string message, SnackbarType type = SnackbarType.Info, int timeoutMs = 3000)
        {
            var snackbar = new SnackbarMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Type = type,
                Timeout = timeoutMs
            };

            lock (_sync)
            {
                if (IsDuplicateLocked(snackbar))
                    return Task.CompletedTask;

                _queue.Enqueue(snackbar);
                if (!_workerRunning)
                {
                    _workerRunning = true;
                    _ = ProcessQueueAsync();
                }
            }

            return Task.CompletedTask;
        }

        private bool IsDuplicateLocked(SnackbarMessage candidate)
        {
            var normalized = candidate.Message.Trim();

            var sameVisible = Messages.Any(x =>
                x.Type == candidate.Type &&
                string.Equals(x.Message.Trim(), normalized, StringComparison.OrdinalIgnoreCase));

            if (sameVisible)
                return true;

            return _queue.Any(x =>
                x.Type == candidate.Type &&
                string.Equals(x.Message.Trim(), normalized, StringComparison.OrdinalIgnoreCase));
        }

        private async Task ProcessQueueAsync()
        {
            while (true)
            {
                SnackbarMessage? current = null;

                lock (_sync)
                {
                    if (_queue.Count > 0)
                    {
                        current = _queue.Dequeue();
                        Messages.Clear();
                        Messages.Add(current);
                    }
                    else
                    {
                        _workerRunning = false;
                        Messages.Clear();
                    }
                }

                if (current == null)
                {
                    OnChange?.Invoke();
                    return;
                }

                OnChange?.Invoke();
                await Task.Delay(current.Timeout);
                Remove(current.Id);
            }
        }

        public void Remove(Guid id)
        {
            var removed = false;

            lock (_sync)
            {
                var msg = Messages.FirstOrDefault(x => x.Id == id);
                if (msg != null)
                {
                    Messages.Remove(msg);
                    removed = true;
                }
            }

            if (removed)
                OnChange?.Invoke();
        }
    }

    public enum SnackbarType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class SnackbarMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = "";
        public SnackbarType Type { get; set; }
        public int Timeout { get; set; }
    }
}
