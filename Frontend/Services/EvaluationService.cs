using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class EvaluationService(HttpClient http) : IEvaluationService
{
    public async Task<ResumeEvaluasiDto?> GetAsync(int subscriptionId)
    {
        var response = await http.GetAsync($"api/evaluation/{subscriptionId}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ResumeEvaluasiDto>() : null;
    }

    public async Task<ResumeEvaluasiDto> SaveAsync(int subscriptionId, string content)
    {
        var response = await http.PostAsJsonAsync($"api/evaluation/{subscriptionId}", new SaveResumeRequest { Content = content });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ResumeEvaluasiDto>())!;
    }
}
