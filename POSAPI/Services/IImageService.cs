namespace POSAPI.Services
{
    public interface IImageApiService
    {
        Task<HttpResponseMessage> UploadImage(MultipartFormDataContent content);
    }

}
