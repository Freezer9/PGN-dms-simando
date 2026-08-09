using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class StageService(HttpClient http) : IStageService
{
    public async Task<PlottingDto?> GetPlottingAsync(int subscriptionId)
        => await GetOrNullAsync<PlottingDto>($"api/stages/{subscriptionId}/plotting");

    public async Task<PlottingDto> SavePlottingAsync(int subscriptionId, SavePlottingRequest request)
        => await PutAsync<SavePlottingRequest, PlottingDto>($"api/stages/{subscriptionId}/plotting", request);

    public async Task<List<CompanyContactDto>> GetContactsAsync(int subscriptionId)
        => await http.GetFromJsonAsync<List<CompanyContactDto>>($"api/stages/{subscriptionId}/contacts") ?? [];

    public async Task<(bool Success, string? Error)> SaveContactAsync(int subscriptionId, SaveContactRequest request)
    {
        var response = await http.PostAsJsonAsync($"api/stages/{subscriptionId}/contacts", request);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteContactAsync(int subscriptionId, int contactId)
    {
        var response = await http.DeleteAsync($"api/stages/{subscriptionId}/contacts/{contactId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<SurveyDto?> GetSurveyAsync(int subscriptionId)
        => await GetOrNullAsync<SurveyDto>($"api/stages/{subscriptionId}/survey");

    public async Task<SurveyDto> SaveSurveyAsync(int subscriptionId, SurveyDto survey)
        => await PutAsync<SurveyDto, SurveyDto>($"api/stages/{subscriptionId}/survey", survey);

    public async Task<A1RegistrationDto?> GetA1Async(int subscriptionId)
        => await GetOrNullAsync<A1RegistrationDto>($"api/stages/{subscriptionId}/a1");

    public async Task<A1RegistrationDto> SaveA1Async(int subscriptionId, A1RegistrationDto a1)
        => await PutAsync<A1RegistrationDto, A1RegistrationDto>($"api/stages/{subscriptionId}/a1", a1);

    public async Task<NolRequestDto?> GetNolRequestAsync(int subscriptionId)
        => await GetOrNullAsync<NolRequestDto>($"api/stages/{subscriptionId}/nol-request");

    public async Task<NolRequestDto> SaveNolRequestAsync(int subscriptionId, NolRequestDto request)
        => await PutAsync<NolRequestDto, NolRequestDto>($"api/stages/{subscriptionId}/nol-request", request);

    // A stage that has not been started yet 404s; that is expected, not an error.
    private async Task<T?> GetOrNullAsync<T>(string url) where T : class
    {
        var response = await http.GetAsync(url);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>() : null;
    }

    private async Task<TOut> PutAsync<TIn, TOut>(string url, TIn body)
    {
        var response = await http.PutAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TOut>())!;
    }
}

public class IssuanceService(HttpClient http) : IIssuanceService
{
    public async Task<NolIssuanceDto?> GetAsync(int subscriptionId)
    {
        var response = await http.GetAsync($"api/issuance/{subscriptionId}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<NolIssuanceDto>() : null;
    }

    public async Task<(bool Success, string? Error)> IssueAsync(int subscriptionId, IssueNolRequest request)
    {
        var response = await http.PostAsJsonAsync($"api/issuance/{subscriptionId}", request);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }
}

public class MasterDataService(HttpClient http) : IMasterDataService
{
    public async Task<List<MasterDataEntryDto>> GetAsync(MasterCategory? category = null)
    {
        var url = category.HasValue ? $"api/masterdata?category={category.Value}" : "api/masterdata";
        return await http.GetFromJsonAsync<List<MasterDataEntryDto>>(url) ?? [];
    }

    public async Task<(bool Success, string? Error)> SaveAsync(int? id, SaveMasterDataRequest request)
    {
        var response = id is > 0
            ? await http.PutAsJsonAsync($"api/masterdata/{id}", request)
            : await http.PostAsJsonAsync("api/masterdata", request);

        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/masterdata/{id}");
        return response.IsSuccessStatusCode;
    }
}

public class BreakGlassService(HttpClient http) : IBreakGlassService
{
    public async Task<List<BreakGlassGrantDto>> GetAsync()
        => await http.GetFromJsonAsync<List<BreakGlassGrantDto>>("api/breakglass") ?? [];

    public async Task<(bool Success, string? Error)> GrantAsync(GrantBreakGlassRequest request)
    {
        var response = await http.PostAsJsonAsync("api/breakglass", request);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> RevokeAsync(int grantId)
    {
        var response = await http.PostAsync($"api/breakglass/{grantId}/revoke", null);
        return response.IsSuccessStatusCode;
    }
}
