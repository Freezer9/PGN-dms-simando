using Microsoft.EntityFrameworkCore;
using Pgn.Dms.Api.Data;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Services;

public class WorkflowService(ApplicationDbContext db)
{
    private static readonly SubscriptionStatus[] StageOrder =
    [
        SubscriptionStatus.Directory, SubscriptionStatus.Plotting, SubscriptionStatus.Prospect,
        SubscriptionStatus.Survey, SubscriptionStatus.A1, SubscriptionStatus.PermohonanNOL
    ];

    public async Task<bool> AdvanceStatusAsync(int subscriptionId, string actorName)
    {
        var sub = await db.Subscriptions.Include(s => s.Submissions).FirstOrDefaultAsync(s => s.Id == subscriptionId);
        if (sub is null) return false;

        var currentIdx = Array.IndexOf(StageOrder, sub.Status);
        if (currentIdx < 0 || currentIdx >= StageOrder.Length - 1) return false;
        if (!sub.Submissions.Any(s => s.Stage == sub.Status)) return false;
        if (sub.Status == SubscriptionStatus.A1 && !sub.SignedOff) return false;

        sub.Status = StageOrder[currentIdx + 1];
        sub.UpdatedAt = DateTime.UtcNow;

        if (sub.Status == SubscriptionStatus.PermohonanNOL && !string.IsNullOrWhiteSpace(sub.ReviewerIds))
        {
            var existing = await db.ReviewSteps.AnyAsync(r => r.SubscriptionId == sub.Id);
            if (!existing)
            {
                var reviewerIds = sub.ReviewerIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < reviewerIds.Length; i++)
                    db.ReviewSteps.Add(new ReviewStep { SubscriptionId = sub.Id, ReviewerId = reviewerIds[i].Trim(), StepOrder = i + 1 });
                sub.CurrentReviewerIndex = 1;
            }
        }

        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = actorName, Action = $"Status naik ke {sub.Status}", At = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task SignOffAsync(int subscriptionId, string actorName)
    {
        var sub = await db.Subscriptions.FindAsync(subscriptionId);
        if (sub is null) return;
        sub.SignedOff = true;
        sub.UpdatedAt = DateTime.UtcNow;
        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = actorName, Action = "Dokumen A1 ditandatangani", At = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    public async Task AssignReviewersAsync(int subscriptionId, string[] reviewerIds)
    {
        var sub = await db.Subscriptions.FindAsync(subscriptionId);
        if (sub is null) return;
        sub.ReviewerIds = string.Join(",", reviewerIds);
        sub.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<bool> SubmitReviewAsync(int subscriptionId, string reviewerId, SubmitReviewRequest req, string actorName)
    {
        var sub = await db.Subscriptions.Include(s => s.ReviewSteps).FirstOrDefaultAsync(s => s.Id == subscriptionId);
        if (sub is null) return false;

        var step = sub.ReviewSteps.FirstOrDefault(r => r.ReviewerId == reviewerId && r.StepOrder == sub.CurrentReviewerIndex);
        if (step is null) return false;

        step.Action = req.Action;
        step.Comment = req.Comment;
        step.ReviewedAt = DateTime.UtcNow;

        switch (req.Action)
        {
            case ReviewAction.Setuju:
                var next = sub.ReviewSteps.FirstOrDefault(r => r.StepOrder == sub.CurrentReviewerIndex + 1 && r.Action == null);
                if (next is not null) sub.CurrentReviewerIndex++;
                else sub.Status = SubscriptionStatus.Disetujui;
                break;
            case ReviewAction.Tolak:
                sub.Status = SubscriptionStatus.Ditolak;
                sub.CurrentReviewerIndex = -1;
                break;
            case ReviewAction.Revisi:
                if (sub.CurrentReviewerIndex > 1)
                {
                    var prev = sub.ReviewSteps.FirstOrDefault(r => r.StepOrder == sub.CurrentReviewerIndex - 1);
                    if (prev is not null) { prev.Action = null; prev.Comment = ""; prev.ReviewedAt = null; }
                    sub.CurrentReviewerIndex--;
                }
                break;
        }

        sub.UpdatedAt = DateTime.UtcNow;
        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = actorName, Action = $"Review: {req.Action}", Details = req.Comment, At = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<SubscriptionDto>> GetPendingReviewAsync(string reviewerId)
    {
        return await db.Subscriptions
            .Include(s => s.Area).Include(s => s.CreatedBy).Include(s => s.ReviewSteps)
            .Where(s => s.Status == SubscriptionStatus.PermohonanNOL && s.ReviewSteps.Any(r => r.ReviewerId == reviewerId && r.Action == null))
            .Select(s => s.ToDto())
            .ToListAsync();
    }

    public async Task<ResumeEvaluasiDto?> GetResumeAsync(int subscriptionId)
    {
        var r = await db.ResumeEvaluasi.Include(x => x.CreatedBy).FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return r?.ToDto();
    }

    public async Task<ResumeEvaluasiDto> SaveResumeAsync(int subscriptionId, string content, string userId, string actorName)
    {
        var existing = await db.ResumeEvaluasi.FirstOrDefaultAsync(r => r.SubscriptionId == subscriptionId);
        if (existing is not null)
        {
            existing.Content = content;
            existing.UpdatedAt = DateTime.UtcNow;
            db.ActivityLogs.Add(new ActivityLog { SubscriptionId = subscriptionId, ActorName = actorName, Action = "Memperbarui resume evaluasi", At = DateTime.UtcNow });
            await db.SaveChangesAsync();
            return existing.ToDto();
        }

        var resume = new ResumeEvaluasi { SubscriptionId = subscriptionId, Content = content, CreatedById = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.ResumeEvaluasi.Add(resume);
        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = subscriptionId, ActorName = actorName, Action = "Menyimpan resume evaluasi", At = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return resume.ToDto();
    }
}
