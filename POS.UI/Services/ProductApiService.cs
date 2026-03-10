using BackOfficeBlazor.Shared.DTOs;

using System.Net.Http.Json;

namespace POS.UI.Services
{
    public class ProductApiService : IProductApiService
    {
        private readonly HttpClient _http;
        public ProductApiService(HttpClient http) => _http = http;

        public async Task<ApiResponse<ProductDto>> GetProduct(string partNumber)
        {
            return await _http.GetFromJsonAsync<ApiResponse<ProductDto>>
                ($"api/product/GetProduct?partNumber={partNumber}")
                ?? ApiResponse<ProductDto>.Fail("Null response");
        }

        public async Task<ApiResponse<ProductDto>> SaveProduct(ProductDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/product/SaveProduct", dto);
            return await resp.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>
                () ?? ApiResponse<ProductDto>.Fail("Null response");
        }
        public async Task<ApiResponse<bool>> SaveGroupProduct(GroupProductDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/product/SaveGroupProduct", dto);

            return await resp.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? ApiResponse<bool>.Fail("Null response");
        }

        public async Task<ApiResponse<List<ProductDto>>> GetProductsAsync(ProductFilterDto filter)
        {
            var query = new Dictionary<string, string?>();

            foreach (var prop in typeof(ProductFilterDto).GetProperties())
            {
                var val = prop.GetValue(filter);
                if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                    query[prop.Name] = val.ToString();
            }

            //var url = QueryHelpers.AddQueryString("api/product/GetProducts", query!);
            var queryString = string.Join("&",
            query.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));

            var url = $"api/product/GetProducts?{queryString}";


            return await _http.GetFromJsonAsync<ApiResponse<List<ProductDto>>>(url)
                   ?? ApiResponse<List<ProductDto>>.Fail("Null response");
        }

        //public async Task<ApiResponse<List<ProductDto>>> GetProductsAsync(ProductFilterDto filter)
        //{
        //    var query = new Dictionary<string, string>();

        //    //if (!string.IsNullOrWhiteSpace(filter.PartNumber))
        //    //    query["PartNumber"] = filter.PartNumber;

        //    //if (!string.IsNullOrWhiteSpace(filter.Barcode))
        //    //    query["Barcode"] = filter.Barcode;

        //    if (!string.IsNullOrWhiteSpace(filter.Year))
        //        query["Year"] = filter.Year;
        //    if (!string.IsNullOrWhiteSpace(filter.MfrPartNumber))
        //        query["MfrPartNumber"] = filter.MfrPartNumber;
        //    if (!string.IsNullOrWhiteSpace(filter.Supplier1Code))
        //        query["Supplier1Code"] = filter.Supplier1Code;
        //    if (!string.IsNullOrWhiteSpace(filter.Search1))
        //        query["Search1"] = filter.Search1;
        //    if (!string.IsNullOrWhiteSpace(filter.Size))
        //        query["Size"] = filter.Size;
        //    if (!string.IsNullOrWhiteSpace(filter.Color))
        //        query["Color"] = filter.Color;
        //    if (!string.IsNullOrWhiteSpace(filter.Search1))
        //        query["Search1"] = filter.Search1;
        //    if (!string.IsNullOrWhiteSpace(filter.Search2))
        //        query["Search2"] = filter.Search2;
        //    if (!string.IsNullOrWhiteSpace(filter.Details))
        //        query["Details"] = filter.Details;
        //    if (!string.IsNullOrWhiteSpace(filter.Search2))
        //        query["Search2"] = filter.Search2;
        //    if (!string.IsNullOrWhiteSpace(filter.CatA))
        //        query["CatA"] = filter.CatA;
        //    if (!string.IsNullOrWhiteSpace(filter.CatACode))
        //        query["CatACode"] = filter.CatACode;

        //    if (!string.IsNullOrWhiteSpace(filter.CatB))
        //        query["CatB"] = filter.CatB;
        //    if (!string.IsNullOrWhiteSpace(filter.CatBCode))
        //        query["CatBCode"] = filter.CatBCode;

        //    if (!string.IsNullOrWhiteSpace(filter.CatC))
        //        query["CatC"] = filter.CatC;
        //    if (!string.IsNullOrWhiteSpace(filter.CatCCode))
        //        query["CatCCode"] = filter.CatCCode;
        //    if (!string.IsNullOrWhiteSpace(filter.Make))
        //        query["Make"] = filter.Make;

        //    if (filter.Website.HasValue)
        //        query["Website"] = filter.Website.Value.ToString();
        //    if (filter.Gender.HasValue)
        //        query["Gender"] = filter.Gender.Value.ToString();

        //    var url = QueryHelpers.AddQueryString("api/product/GetProducts", query);

        //    return await _http.GetFromJsonAsync<ApiResponse<List<ProductDto>>>(url)
        //           ?? ApiResponse<List<ProductDto>>.Fail("Null response");
        //}


    }
}
