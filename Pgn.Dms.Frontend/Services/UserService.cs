using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class UserService(HttpClient http) : IUserService
{
    public async Task<List<UserInfo>> GetUsersAsync()
        => await http.GetFromJsonAsync<List<UserInfo>>("api/users") ?? [];

    public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserRequest request)
    {
        var response = await http.PostAsJsonAsync("api/users", request);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await response.Content.ReadAsStringAsync());
    }
}
