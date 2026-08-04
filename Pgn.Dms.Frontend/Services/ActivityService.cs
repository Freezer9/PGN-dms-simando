using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class ActivityService(HttpClient http) : IActivityService
{
    public async Task<List<ActivityLogDto>> GetRecentAsync(int count = 20)
        => await http.GetFromJsonAsync<List<ActivityLogDto>>($"api/subscriptions/activity/recent?count={count}") ?? [];

    public async Task<List<ActivityLogDto>> GetForSubscriptionAsync(int subscriptionId)
        => await http.GetFromJsonAsync<List<ActivityLogDto>>($"api/subscriptions/{subscriptionId}/activity") ?? [];
}
