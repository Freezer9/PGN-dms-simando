using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class WorkflowService(HttpClient http) : IWorkflowService
{
    public async Task<AdvanceResult> AdvanceStatusAsync(int subscriptionId)
    {
        var response = await http.PostAsync($"api/subscriptions/{subscriptionId}/advance", null);
        if (!response.IsSuccessStatusCode)
            return new AdvanceResult { Ok = false, Reason = "Gagal menghubungi server." };

        return await response.Content.ReadFromJsonAsync<AdvanceResult>()
            ?? new AdvanceResult { Ok = false, Reason = "Respons tidak dikenali." };
    }

    public async Task AssignReviewersAsync(int subscriptionId, string[] reviewerIds)
    {
        var response = await http.PostAsJsonAsync($"api/subscriptions/{subscriptionId}/assign-reviewers",
            new AssignReviewersRequest { ReviewerIds = reviewerIds });
        response.EnsureSuccessStatusCode();
    }

    public async Task<(bool Success, string? Error)> ReassignStepAsync(int subscriptionId, string reviewerId)
    {
        var response = await http.PostAsJsonAsync($"api/subscriptions/{subscriptionId}/reassign-step",
            new ReassignStepRequest { ReviewerId = reviewerId });

        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> SubmitReviewAsync(int subscriptionId, ReviewAction action, string comment)
    {
        var response = await http.PostAsJsonAsync($"api/subscriptions/{subscriptionId}/review",
            new SubmitReviewRequest { Action = action, Comment = comment });
        return response.IsSuccessStatusCode;
    }

    public async Task SignOffAsync(int subscriptionId)
    {
        var response = await http.PostAsync($"api/subscriptions/{subscriptionId}/signoff", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>An empty inbox and a forbidden one look the same to the caller — neither is an error.</summary>
    public async Task<List<SubscriptionDto>> GetPendingReviewAsync()
    {
        var response = await http.GetAsync("api/subscriptions/pending-review");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<SubscriptionDto>>() ?? []
            : [];
    }
}
