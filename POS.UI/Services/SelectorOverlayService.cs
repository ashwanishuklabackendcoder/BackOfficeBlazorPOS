using Microsoft.AspNetCore.Components;

namespace POS.UI.Services;

public sealed class SelectorOverlayService
{
    private readonly object _sync = new();

    public event Action? OnChange;

    public SelectorOverlayState? Current { get; private set; }

    public async Task ShowAsync(
        string ownerKey,
        Type componentType,
        IDictionary<string, object> parameters,
        SelectorOverlayPlacement placement,
        Func<Task>? onDismissedAsync = null)
    {
        SelectorOverlayState? previous = null;

        lock (_sync)
        {
            if (Current is not null && !string.Equals(Current.OwnerKey, ownerKey, StringComparison.Ordinal))
            {
                previous = Current;
                Current = null;
            }

            Current = new SelectorOverlayState(ownerKey, componentType, parameters, placement, onDismissedAsync);
        }

        OnChange?.Invoke();

        if (previous?.OnDismissedAsync != null)
            await previous.OnDismissedAsync();
    }

    public async Task RequestCloseAsync()
    {
        SelectorOverlayState? current;

        lock (_sync)
        {
            current = Current;
            Current = null;
        }

        if (current == null)
            return;

        OnChange?.Invoke();

        if (current.OnDismissedAsync != null)
            await current.OnDismissedAsync();
    }
}

public sealed record SelectorOverlayState(
    string OwnerKey,
    Type ComponentType,
    IDictionary<string, object> Parameters,
    SelectorOverlayPlacement Placement,
    Func<Task>? OnDismissedAsync);

public sealed class SelectorOverlayPlacement
{
    public double Top { get; set; }

    public double Left { get; set; }

    public double Width { get; set; }

    public double MaxWidth { get; set; }
}
