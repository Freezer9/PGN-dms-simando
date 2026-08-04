using Pgn.Dms.Shared;

namespace Pgn.Dms.Web.Services;

public interface ISubscriptionService
{
    Task<List<SubscriptionDto>> GetAllAsync();
    Task<List<SubscriptionDto>> GetByAreaAsync(int areaId);
    Task<SubscriptionDto?> GetAsync(int id);
    Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request);
    Task<SubmissionRecordDto> UploadAsync(int subscriptionId, Stream fileStream, string fileName, string contentType);
    Task<List<RegionDto>> GetRegionsAsync();
    Task<List<AreaDto>> GetAreasAsync();
    Task<List<AreaDto>> GetAreasByRegionAsync(int regionId);
}

public interface IWorkflowService
{
    Task<bool> AdvanceStatusAsync(int subscriptionId);
    Task AssignReviewersAsync(int subscriptionId, string[] reviewerIds);
    Task<bool> SubmitReviewAsync(int subscriptionId, ReviewAction action, string comment);
    Task SignOffAsync(int subscriptionId);
    Task<List<SubscriptionDto>> GetPendingReviewAsync();
}

public interface IActivityService
{
    Task<List<ActivityLogDto>> GetRecentAsync(int count = 20);
    Task<List<ActivityLogDto>> GetForSubscriptionAsync(int subscriptionId);
}

public interface IEvaluationService
{
    Task<ResumeEvaluasiDto?> GetAsync(int subscriptionId);
    Task<ResumeEvaluasiDto> SaveAsync(int subscriptionId, string content);
}

public interface IUserService
{
    Task<List<UserInfo>> GetUsersAsync();
    Task<(bool Success, string? Error)> CreateUserAsync(CreateUserRequest request);
}

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password);
}
