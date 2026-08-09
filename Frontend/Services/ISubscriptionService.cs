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
    Task<bool> UpdateLocationAsync(int subscriptionId, double latitude, double longitude);
}

public interface IWorkflowService
{
    /// <summary>On failure, <see cref="AdvanceResult.Reason"/> names the blocking gate.</summary>
    Task<AdvanceResult> AdvanceStatusAsync(int subscriptionId);
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
    Task<NolEvaluationDto?> GetDetailAsync(int subscriptionId);
    Task<NolEvaluationDto> SaveDetailAsync(int subscriptionId, NolEvaluationDto detail);
}

/// <summary>Stage 2–6 satellite data. Get* returns null when the stage has not been started.</summary>
public interface IStageService
{
    Task<PlottingDto?> GetPlottingAsync(int subscriptionId);
    Task<PlottingDto> SavePlottingAsync(int subscriptionId, SavePlottingRequest request);

    Task<List<CompanyContactDto>> GetContactsAsync(int subscriptionId);
    Task<(bool Success, string? Error)> SaveContactAsync(int subscriptionId, SaveContactRequest request);
    Task<bool> DeleteContactAsync(int subscriptionId, int contactId);

    Task<SurveyDto?> GetSurveyAsync(int subscriptionId);
    Task<SurveyDto> SaveSurveyAsync(int subscriptionId, SurveyDto survey);

    Task<A1RegistrationDto?> GetA1Async(int subscriptionId);
    Task<A1RegistrationDto> SaveA1Async(int subscriptionId, A1RegistrationDto a1);

    Task<NolRequestDto?> GetNolRequestAsync(int subscriptionId);
    Task<NolRequestDto> SaveNolRequestAsync(int subscriptionId, NolRequestDto request);
}

public interface IIssuanceService
{
    Task<NolIssuanceDto?> GetAsync(int subscriptionId);
    Task<(bool Success, string? Error)> IssueAsync(int subscriptionId, IssueNolRequest request);
}

public interface IMasterDataService
{
    Task<List<MasterDataEntryDto>> GetAsync(MasterCategory? category = null);
    Task<(bool Success, string? Error)> SaveAsync(int? id, SaveMasterDataRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IBreakGlassService
{
    Task<List<BreakGlassGrantDto>> GetAsync();
    Task<(bool Success, string? Error)> GrantAsync(GrantBreakGlassRequest request);
    Task<bool> RevokeAsync(int grantId);
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
