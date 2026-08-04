namespace Pgn.Dms.Shared;

public enum SubscriptionStatus
{
    Directory,
    Plotting,
    Prospect,
    Survey,
    A1,
    PermohonanNOL,
    Disetujui,
    Ditolak
}

public enum ReviewAction
{
    Setuju,
    Tolak,
    Revisi
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AreaId { get; set; }
    public string? AreaName { get; set; }
}

public class RegionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<AreaDto> Areas { get; set; } = [];
}

public class AreaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int RegionId { get; set; }
    public string? RegionName { get; set; }
}

public class SubscriptionDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string Address { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public SubscriptionStatus Status { get; set; }
    public int AreaId { get; set; }
    public string? AreaName { get; set; }
    public string? RegionName { get; set; }
    public string CreatedById { get; set; } = "";
    public string? CreatedByName { get; set; }
    public int CurrentReviewerIndex { get; set; } = -1;
    public string ReviewerIds { get; set; } = "";
    public bool SignedOff { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SubmissionRecordDto> Submissions { get; set; } = [];
    public List<ReviewStepDto> ReviewSteps { get; set; } = [];
    public ResumeEvaluasiDto? ResumeEvaluasi { get; set; }
    public List<ActivityLogDto> ActivityLogs { get; set; } = [];
}

public class SubmissionRecordDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public SubscriptionStatus Stage { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string UploadedById { get; set; } = "";
    public string? UploadedByName { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ReviewStepDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string ReviewerId { get; set; } = "";
    public string? ReviewerName { get; set; }
    public int StepOrder { get; set; }
    public ReviewAction? Action { get; set; }
    public string Comment { get; set; } = "";
    public DateTime? ReviewedAt { get; set; }
}

public class ResumeEvaluasiDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string Content { get; set; } = "";
    public string CreatedById { get; set; } = "";
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public int? SubscriptionId { get; set; }
    public string ActorName { get; set; } = "";
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime At { get; set; }
}

public class CreateSubscriptionRequest
{
    public string CompanyName { get; set; } = "";
    public string Address { get; set; } = "";
    public int AreaId { get; set; }
}

public class SubmitReviewRequest
{
    public ReviewAction Action { get; set; }
    public string Comment { get; set; } = "";
}

public class SaveResumeRequest
{
    public string Content { get; set; } = "";
}

public class CreateUserRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AreaId { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public UserInfo User { get; set; } = new();
}

public class AssignReviewersRequest
{
    public string[] ReviewerIds { get; set; } = [];
}
