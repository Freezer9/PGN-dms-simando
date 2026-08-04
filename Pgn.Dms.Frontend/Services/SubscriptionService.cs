using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class SubscriptionService(HttpClient http) : ISubscriptionService
{
    public async Task<List<SubscriptionDto>> GetAllAsync()
        => await http.GetFromJsonAsync<List<SubscriptionDto>>("api/subscriptions") ?? [];

    public async Task<List<SubscriptionDto>> GetByAreaAsync(int areaId)
        => (await GetAllAsync()).Where(s => s.AreaId == areaId).ToList();

    public async Task<SubscriptionDto?> GetAsync(int id)
    {
        var response = await http.GetAsync($"api/subscriptions/{id}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
    }

    public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request)
    {
        var response = await http.PostAsJsonAsync("api/subscriptions", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>())!;
    }

    public async Task<SubmissionRecordDto> UploadAsync(int subscriptionId, Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await http.PostAsync($"api/subscriptions/{subscriptionId}/upload", content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SubmissionRecordDto>())!;
    }

    public async Task<List<RegionDto>> GetRegionsAsync()
        => await http.GetFromJsonAsync<List<RegionDto>>("api/subscriptions/regions") ?? [];

    public async Task<List<AreaDto>> GetAreasAsync()
        => await http.GetFromJsonAsync<List<AreaDto>>("api/subscriptions/areas") ?? [];

    public async Task<List<AreaDto>> GetAreasByRegionAsync(int regionId)
        => (await GetAreasAsync()).Where(a => a.RegionId == regionId).ToList();
}
