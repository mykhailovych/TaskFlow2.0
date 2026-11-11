using System.Net.Http;
using System.Net.Http.Json;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7273/")
            };
        }

        public async Task<UserDto?> RegisterAsync(RegisterDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<UserDto?> LoginAsync(LoginDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }


    }
}
