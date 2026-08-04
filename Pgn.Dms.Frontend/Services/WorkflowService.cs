using System.Net.Http.Json;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public class WorkflowService(HttpClient http) : IWorkflowService
{
    public async Task<bool> AdvanceStatusAsync(int subscriptionId)
    {
        var response = await http.PostAsync($"api/subscriptions/{subscriptionId}/advance", null);
        return response.IsSuccessStatusCode;
    }

    public async Task AssignReviewersAsync(int subscriptionId, string[] reviewerIds)
    {
        var response = await http.PostAsJsonAsync($"api/subscriptions/{subscriptionId}/assign-reviewers",
            new AssignReviewersRequest { ReviewerIds = reviewerIds });
        response.EnsureSuccessStatusCode();
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

    public async Task<List<SubscriptionDto>> GetPendingReviewAsync()
        => await http.GetFromJsonAsync<List<SubscriptionDto>>("api/subscriptions/pending-review") ?? [];
}
