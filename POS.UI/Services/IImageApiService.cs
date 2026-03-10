using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace POS.UI.Services
{
    public interface IImageApiService
    {
        Task<HttpResponseMessage> UploadImage(MultipartFormDataContent content);
    }

}
