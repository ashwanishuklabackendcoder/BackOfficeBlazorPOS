
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using POSAPI.Config;


namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        private readonly UploadSettings _settings;

        public ImageController(IOptions<UploadSettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files, [FromForm] string? folder = null)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var uploadFolder = string.IsNullOrWhiteSpace(folder)
                ? "products"
                : folder.Trim();

            var cloudSettings = HttpContext.RequestServices
                .GetRequiredService<IOptions<CloudinarySettings>>().Value;

            var account = new Account(
                cloudSettings.CloudName,
                cloudSettings.ApiKey,
                cloudSettings.ApiSecret
            );

            var cloudinary = new Cloudinary(account);

            var paths = new List<string>();

            foreach (var file in files.Take(4))
            {
                if (file.Length == 0)
                    continue;

                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = uploadFolder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var result = await cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    return StatusCode(500, "Cloudinary upload failed");

                paths.Add(result.SecureUrl.ToString());
            }

            return Ok(paths);
        }
        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadImages(List<IFormFile> files)
        //{
        //    if (files == null || files.Count == 0)
        //        return BadRequest("No files uploaded.");

        //    if (string.IsNullOrWhiteSpace(_settings.UiUploadRoot))
        //        return StatusCode(500, "Upload path not configured.");

        //    var folderId = Guid.NewGuid().ToString();
        //    var uploadRoot = Path.Combine(_settings.UiUploadRoot, folderId);

        //    Directory.CreateDirectory(uploadRoot);

        //    var paths = new List<string>();

        //    foreach (var file in files.Take(4))
        //    {
        //        if (file.Length == 0)
        //            continue;

        //        var ext = Path.GetExtension(file.FileName);

        //        var fileName =
        //            $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}{ext}";

        //        var fullPath = Path.Combine(uploadRoot, fileName);

        //        await using var stream = new FileStream(fullPath, FileMode.Create);
        //        await file.CopyToAsync(stream);

        //        // Path returned to UI / saved in DB
        //        paths.Add($"/uploads/{folderId}/{fileName}");
        //    }

        //    return Ok(paths);
        //}
    }
}
