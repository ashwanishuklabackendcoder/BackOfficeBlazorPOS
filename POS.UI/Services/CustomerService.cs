using BackOfficeBlazor.Shared.DTOs;
using System.Net.Http.Json;

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
            var response = await _http.GetAsync($"api/customer/GetCustomers?accNo={Uri.EscapeDataString(accNo)}");
            var payload = await ApiResponseReader.ReadAsync<CustomerDto>(
                response,
                "Unable to load customer.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<CustomerDto>.Fail(payload.Message);
        }

        public async Task<ApiResponse<List<CustomerDto>>> GetAllAsync()
        {
            
            var response = await _http.GetAsync("api/customer/GetAllCustomers");
            var payload = await ApiResponseReader.ReadAsync<List<CustomerDto>>(
                response,
                "Unable to load customers.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<List<CustomerDto>>.Fail(payload.Message);
        }

        public async Task<ApiResponse<List<CustomerDto>>> SearchAsync(CustomerSearchRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("api/customer/SearchCustomers", request);
            var payload = await ApiResponseReader.ReadAsync<List<CustomerDto>>(
                response,
                "Unable to search customers.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<List<CustomerDto>>.Fail(payload.Message);
        }

        public async Task<ApiResponse<CustomerDto>?> SaveAsync(CustomerDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/customer/SaveCustomer", dto);
            var payload = await ApiResponseReader.ReadAsync<CustomerDto>(
                response,
                "Unable to save customer.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<CustomerDto>.Fail(payload.Message);
        }

        public async Task<ApiResponse<object>> DeleteAsync(string accNo)
        {
            var response = await _http.DeleteAsync($"api/customer/DeleteCustomer?accNo={accNo}");
            var payload = await ApiResponseReader.ReadAsync<object>(
                response,
                "Unable to delete customer.");

            return response.IsSuccessStatusCode
                ? payload
                : ApiResponse<object>.Fail(payload.Message);
        }
    }
}
