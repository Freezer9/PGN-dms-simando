using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class UserService(HttpClient http) : IUserService
{
    /// <summary>Callers without the admin role get an empty list rather than an exception —
    /// several pages use this only to populate an optional picker.</summary>
    public async Task<List<UserInfo>> GetUsersAsync()
    {
        var response = await http.GetAsync("api/users");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<UserInfo>>() ?? []
            : [];
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserRequest request)
    {
        var response = await http.PostAsJsonAsync("api/users", request);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await response.Content.ReadAsStringAsync());
    }
}
