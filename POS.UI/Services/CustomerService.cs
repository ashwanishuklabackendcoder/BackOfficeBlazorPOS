using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _http;
        public CustomerService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<CustomerDto>> GetAsync(string accNo)
        {
            return await _http.GetFromJsonAsync<ApiResponse<CustomerDto>>
                ($"api/customer/GetCustomers?accNo={accNo}")
                ?? ApiResponse<CustomerDto>.Fail("Null response");
        }
        public async Task<ApiResponse<List<CustomerDto>>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<ApiResponse<List<CustomerDto>>>
                ($"api/customer/GetAllCustomers")
                ?? ApiResponse<List<CustomerDto>>.Fail("Null response");
        }
        public async Task<ApiResponse<CustomerDto>?> SaveAsync(CustomerDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/customer/SaveCustomer", dto);

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<CustomerDto>.Fail(raw);
            }

            try
            {
                return JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(raw,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                return ApiResponse<CustomerDto>.Fail("Invalid server response: " + raw);
            }
        }

        //public async Task<ApiResponse<CustomerDto>> SaveAsync(CustomerDto dto)
        //{
        //    var response = await _http.PostAsJsonAsync("api/customer/SaveCustomer", dto);
        //    return await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>
        //        () ?? ApiResponse<CustomerDto>.Fail("Null response");
        //}

        public async Task<ApiResponse<object>> DeleteAsync(string accNo)
        {
            var response = await _http.DeleteAsync($"api/customer/DeleteCustomer?accNo={accNo}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<object>>
                () ?? ApiResponse<object>.Fail("Null response");
        }
    }
}
