using Microsoft.EntityFrameworkCore;
using Pgn.Dms.Api.Data;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Services;

public class WorkflowService(ApplicationDbContext db)
{
    public async Task<AdvanceResult> AdvanceStatusAsync(int subscriptionId, string actorName)
    {
        var sub = await db.Subscriptions.Include(s => s.Submissions).FirstOrDefaultAsync(s => s.Id == subscriptionId);
        if (sub is null) return Blocked("Perusahaan tidak ditemukan.");

        var next = SubscriptionStages.Next(sub.Status);
        if (next is null)
            return Blocked(SubscriptionStages.IsTerminal(sub.Status)
                ? "Record sudah final dan tidak dapat dilanjutkan."
                : "Sudah berada di tahap terakhir.");

        var blockedBy = await CheckGateAsync(sub);
        if (blockedBy is not null) return Blocked(blockedBy);

        // Stamp the submission before moving on — the record leaves stage 6 exactly once.
        if (sub.Status == SubscriptionStatus.PermohonanNOL)
        {
            var request = await db.NolRequests.FirstOrDefaultAsync(n => n.SubscriptionId == sub.Id);
            if (request is not null) request.SubmittedAt ??= DateTime.UtcNow;
        }

        sub.Status = next.Value;
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

        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = actorName, Action = $"Status naik ke {SubscriptionStages.Label(sub.Status)}", At = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return new AdvanceResult { Ok = true, NewStatus = sub.Status };
    }

    private static AdvanceResult Blocked(string reason) => new() { Ok = false, Reason = reason };

    /// <summary>
    /// Stage gates from Docs/14-role-navigation-guide.md. Returns the blocking reason,
    /// or null when the record may advance out of its current stage.
    /// </summary>
    private async Task<string?> CheckGateAsync(Subscription sub)
    {
        switch (sub.Status)
        {
            case SubscriptionStatus.Plotting:
            {
                var plotting = await db.Plottings.FirstOrDefaultAsync(p => p.SubscriptionId == sub.Id);
                if (plotting is null || plotting.SalesUserId is null
                    || plotting.PosisiPelanggan is null || plotting.Kawasan is null)
                    return "Lengkapi Plotting By, Posisi Pelanggan, dan Kawasan terlebih dahulu.";
                return null;
            }

            case SubscriptionStatus.Prospect:
            {
                var hasContact = await db.CompanyContacts.AnyAsync(c =>
                    c.SubscriptionId == sub.Id && c.Nama != "" && c.Jabatan != "");
                return hasContact ? null : "Tambahkan minimal 1 kontak PIC dengan nama dan jabatan.";
            }

            case SubscriptionStatus.Survey:
                return sub.Submissions.Any(s => s.Stage == SubscriptionStatus.Survey)
                    ? null
                    : "Unggah dokumen KK0 yang sudah ditandatangani.";

            case SubscriptionStatus.A1:
            {
                if (!sub.Submissions.Any(s => s.Stage == SubscriptionStatus.A1))
                    return "Unggah dokumen A1 yang sudah ditandatangani.";
                if (!sub.SignedOff)
                    return "Bukti Kelayakan belum ditandatangani.";

                var a1 = await db.A1Registrations.FirstOrDefaultAsync(a => a.SubscriptionId == sub.Id);
                if (a1?.SkemaHarga == SkemaHarga.SiGas && !a1.MomSigasTersedia)
                    return "Skema SiGas memerlukan MOM SiGas.";
                return null;
            }

            case SubscriptionStatus.PermohonanNOL:
            {
                var nol = await db.NolRequests.FirstOrDefaultAsync(n => n.SubscriptionId == sub.Id);
                if (nol?.CapexPreGr3 is null)
                    return "Isi Capex Pre GR3 pada permohonan NOL.";
                if (!sub.Submissions.Any(s => s.Stage == SubscriptionStatus.Survey))
                    return "Dokumen KK0 belum terunggah.";
                if (!sub.Submissions.Any(s => s.Stage == SubscriptionStatus.A1))
                    return "Dokumen A1 belum terunggah.";
                return null;
            }

            // Directory → Plotting and Evaluasi → Penerbitan carry no document gate.
            default:
                return null;
        }
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
                // The Reviewer is the final executor, so the last approval closes the record.
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
