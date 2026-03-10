using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;

namespace POS.UI.Services
{
    public class ImageApiService : IImageApiService
    {
        private readonly HttpClient _http;

        public ImageApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<HttpResponseMessage> UploadImage(MultipartFormDataContent content)
        {
            return await _http.PostAsync("api/images/upload", content);
        }



    }
}
