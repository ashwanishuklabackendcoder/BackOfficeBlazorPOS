using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using POS.UI.Services;

namespace POS.UI.Layout;

public class AppErrorBoundary : ErrorBoundary
{
    [Inject]
    private AppSnackbarService Snackbar { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        _ = Snackbar.Show(
            "Unexpected error occurred. Please retry. If it persists, re-login.",
            SnackbarType.Error,
            5000);

        return Task.CompletedTask;
    }
}
