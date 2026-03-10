using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _http;

        public LocationService(HttpClient http)
        {
            _http = http;
        }
        public async Task<List<LocationDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<List<LocationDto>>(
                "api/location/GetAllLocations");

            return result ?? new List<LocationDto>();
        }

    }
}
