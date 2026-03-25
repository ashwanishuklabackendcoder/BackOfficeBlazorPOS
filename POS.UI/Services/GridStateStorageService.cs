using System.Text.Json;
using Microsoft.JSInterop;
using POS.UI.Models;

namespace POS.UI.Services
{
    public class GridStateStorageService
    {
        private readonly IJSRuntime _js;

        public GridStateStorageService(IJSRuntime js)
        {
            _js = js;
        }

        private static string BuildKey(string gridKey)
            => $"POS_GRID_STATE_{gridKey}";

        public async Task SaveStateAsync(string gridKey, GridState state)
        {
            if (string.IsNullOrWhiteSpace(gridKey))
                return;

            var json = JsonSerializer.Serialize(state);
            await _js.InvokeVoidAsync("localStorage.setItem", BuildKey(gridKey), json);
        }

        public async Task<GridState?> LoadStateAsync(string gridKey)
        {
            if (string.IsNullOrWhiteSpace(gridKey))
                return null;

            var json = await _js.InvokeAsync<string>("localStorage.getItem", BuildKey(gridKey));
            if (string.IsNullOrWhiteSpace(json))
                return new GridState();

            try
            {
                return JsonSerializer.Deserialize<GridState>(json);
            }
            catch
            {
                return new GridState();
            }
        }

        public async Task ClearStateAsync(string gridKey)
        {
            if (string.IsNullOrWhiteSpace(gridKey))
                return;

            await _js.InvokeVoidAsync("localStorage.removeItem", BuildKey(gridKey));
        }
    }
}
