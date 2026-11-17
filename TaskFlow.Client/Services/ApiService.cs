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

        public async Task<List<ProjectDto>> GetUserProjectsAsync(int userId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<ProjectDto>>($"api/projects/user/{userId}");
            return result ?? new List<ProjectDto>();
        }

        public async Task<List<ColumnDto>> GetProjectBoardAsync(int projectId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<ColumnDto>>($"api/projects/{projectId}/board");
            return result ?? new List<ColumnDto>();
        }

        public async Task<ProjectDto?> CreateProjectAsync(CreateProjectDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/projects", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProjectDto>();
        }

        public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/tasks", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TaskDto>();
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tasks/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<CommentDto?> AddCommentAsync(int taskId, CreateCommentDto dto)
        {
            var responce = await _httpClient.PostAsJsonAsync($"api/tasks/{taskId}/comments", dto);
            if (responce.IsSuccessStatusCode) return await responce.Content.ReadFromJsonAsync<CommentDto>();
            return null;
        }

        public async Task<ColumnDto?> CreateColumnAsync(CreateColumnDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/columns", dto);

            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ColumnDto>();

            }
            return null;
        }

        public async Task<bool> UpdateColumnAsync(int columnId, UpdateColumnDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/columns/{columnId}", dto);
            return response.IsSuccessStatusCode;
        }

    }
}
