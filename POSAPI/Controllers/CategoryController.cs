using BackOfficeBlazor.Admin.Services.Interfaces;
using BackOfficeBlazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace POSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }
        //[HttpGet("GetAllCategory/{level}")]
        //public async Task<IActionResult> GetAllCategory(string level)
        //{
        //    var data = await _service.GetAllCategory(level);
        //    return Ok(data);
        //}
        [HttpGet("GetAllCategory/{level}")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategory(string level)
        {
            var data = await _service.GetAllCategory(level);
            return Ok(data); // MUST be JSON
        }
        [HttpGet, Route("GetCategories")]
        public async Task<IActionResult> Get(string code)
        {
            var result = await _service.GetAsync(code);
            if (result == null)
                return Ok(ApiResponse<object>.Fail("Category not found"));

            return Ok(ApiResponse<CategoryDto>.Ok(result, "Success"));
        }


        [HttpPost, Route("SaveCategory")]
        public async Task<IActionResult> SaveCategory(CategoryDto dto)
        {
            var existing = await _service.GetAsync(dto.Code);

            if (existing == null)
            {
                // INSERT
                var created = await _service.CreateAsync(dto);
                return Ok(ApiResponse<CategoryDto>.Ok(created, "Category added successfully"));
            }
            else
            {
                // UPDATE
                var updated = await _service.UpdateAsync(dto);
                return Ok(ApiResponse<CategoryDto>.Ok(updated, "Category updated successfully"));
            }
        }


        [HttpDelete, Route("DeleteCategory/{code}")]
        public async Task<IActionResult> DeleteCategory(string code)
        {
            var deleted = await _service.DeleteAsync(code);

            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Fail("Category not found"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Category deleted successfully"));
        }


        [HttpGet, Route("suggest")]
     
        public async Task<IActionResult> Suggest(string type, string query)
        {
            if (string.IsNullOrWhiteSpace(type) || query.Length < 3)
            {
                return Ok(new ApiResponse<List<CategoryDto>>
                {
                    Success = true,
                    Data = new()
                });
            }

            var result = await _service.SuggestAsync(type, query);

            return Ok(new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = result
            });
        }

    }

}
