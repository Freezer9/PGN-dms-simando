using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Data;

public class Region
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<Area> Areas { get; set; } = [];

    public RegionDto ToDto() => new() { Id = Id, Name = Name, Areas = Areas.Select(a => a.ToDto()).ToList() };
}

public class Area
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int RegionId { get; set; }
    public Region Region { get; set; } = default!;
    public List<Subscription> Subscriptions { get; set; } = [];

    public AreaDto ToDto() => new() { Id = Id, Name = Name, RegionId = RegionId, RegionName = Region?.Name };
}

public class Subscription
{
    public int Id { get; set; }
    public required string CompanyName { get; set; }
    public string Address { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Directory;
    public int AreaId { get; set; }
    public Area Area { get; set; } = default!;
    public required string CreatedById { get; set; }
    public ApplicationUser CreatedBy { get; set; } = default!;
    public int CurrentReviewerIndex { get; set; } = -1;
    public string ReviewerIds { get; set; } = "";
    public bool SignedOff { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<SubmissionRecord> Submissions { get; set; } = [];
    public List<ReviewStep> ReviewSteps { get; set; } = [];
    public ResumeEvaluasi? ResumeEvaluasi { get; set; }
    public List<ActivityLog> ActivityLogs { get; set; } = [];

    public SubscriptionDto ToDto() => new()
    {
        Id = Id,
        CompanyName = CompanyName,
        Address = Address,
        Latitude = Latitude,
        Longitude = Longitude,
        Status = Status,
        AreaId = AreaId,
        AreaName = Area?.Name,
        RegionName = Area?.Region?.Name,
        CreatedById = CreatedById,
        CreatedByName = CreatedBy?.FullName,
        CurrentReviewerIndex = CurrentReviewerIndex,
        ReviewerIds = ReviewerIds,
        SignedOff = SignedOff,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt,
        Submissions = Submissions.Select(s => s.ToDto()).ToList(),
        ReviewSteps = ReviewSteps.Select(r => r.ToDto()).ToList(),
        ResumeEvaluasi = ResumeEvaluasi?.ToDto(),
        ActivityLogs = ActivityLogs.Select(a => a.ToDto()).ToList()
    };
}

public class SubmissionRecord
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;
    public SubscriptionStatus Stage { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string UploadedById { get; set; }
    public ApplicationUser UploadedBy { get; set; } = default!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public SubmissionRecordDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        Stage = Stage,
        FileName = FileName,
        FilePath = FilePath,
        UploadedById = UploadedById,
        UploadedByName = UploadedBy?.FullName,
        UploadedAt = UploadedAt
    };
}

public class ReviewStep
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;
    public required string ReviewerId { get; set; }
    public ApplicationUser Reviewer { get; set; } = default!;
    public int StepOrder { get; set; }
    public ReviewAction? Action { get; set; }
    public string Comment { get; set; } = "";
    public DateTime? ReviewedAt { get; set; }

    public ReviewStepDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        ReviewerId = ReviewerId,
        ReviewerName = Reviewer?.FullName,
        StepOrder = StepOrder,
        Action = Action,
        Comment = Comment,
        ReviewedAt = ReviewedAt
    };
}

public class ResumeEvaluasi
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;
    public string Content { get; set; } = "";
    public required string CreatedById { get; set; }
    public ApplicationUser CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ResumeEvaluasiDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        Content = Content,
        CreatedById = CreatedById,
        CreatedByName = CreatedBy?.FullName,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };
}

public class ActivityLog
{
    public int Id { get; set; }
    public int? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public required string ActorName { get; set; }
    public required string Action { get; set; }
    public string Details { get; set; } = "";
    public DateTime At { get; set; } = DateTime.UtcNow;

    public ActivityLogDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        ActorName = ActorName,
        Action = Action,
        Details = Details,
        At = At
    };
}
