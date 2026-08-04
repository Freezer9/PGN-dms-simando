using Microsoft.EntityFrameworkCore;
using Pgn.Dms.Api.Data;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Services;

public class SubscriptionService(ApplicationDbContext db)
{
    public async Task<List<SubscriptionDto>> GetAllAsync()
    {
        return await db.Subscriptions
            .Include(s => s.Area).ThenInclude(a => a.Region)
            .Include(s => s.CreatedBy)
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => s.ToDto())
            .ToListAsync();
    }

    public async Task<SubscriptionDto?> GetAsync(int id)
    {
        var sub = await db.Subscriptions
            .Include(s => s.Area).ThenInclude(a => a.Region)
            .Include(s => s.CreatedBy)
            .Include(s => s.Submissions).ThenInclude(r => r.UploadedBy)
            .Include(s => s.ReviewSteps).ThenInclude(r => r.Reviewer)
            .Include(s => s.ResumeEvaluasi).ThenInclude(r => r.CreatedBy)
            .Include(s => s.ActivityLogs)
            .FirstOrDefaultAsync(s => s.Id == id);
        return sub?.ToDto();
    }

    public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest req, string userId, string actorName)
    {
        var sub = new Subscription
        {
            CompanyName = req.CompanyName,
            Address = req.Address,
            AreaId = req.AreaId,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = sub.Id, ActorName = actorName, Action = $"Membuat berlangganan {req.CompanyName}", At = DateTime.UtcNow });
        await db.SaveChangesAsync();

        return (await GetAsync(sub.Id))!;
    }

    public async Task<SubmissionRecordDto> AddSubmissionAsync(int subscriptionId, string fileName, string filePath, string userId, string actorName)
    {
        var sub = await db.Subscriptions.FindAsync(subscriptionId);
        if (sub is null) throw new InvalidOperationException("Subscription not found");

        var record = new SubmissionRecord
        {
            SubscriptionId = subscriptionId,
            Stage = sub.Status,
            FileName = fileName,
            FilePath = filePath,
            UploadedById = userId,
            UploadedAt = DateTime.UtcNow
        };
        db.SubmissionRecords.Add(record);
        db.ActivityLogs.Add(new ActivityLog { SubscriptionId = subscriptionId, ActorName = actorName, Action = $"Upload dokumen {fileName}", At = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return record.ToDto();
    }

    public async Task<List<RegionDto>> GetRegionsAsync()
    {
        return await db.Regions.Include(r => r.Areas).Select(r => r.ToDto()).ToListAsync();
    }

    public async Task<List<AreaDto>> GetAreasAsync()
    {
        return await db.Areas.Include(a => a.Region).Select(a => a.ToDto()).ToListAsync();
    }

    public async Task<List<ActivityLogDto>> GetRecentActivityAsync(int count = 20)
    {
        return await db.ActivityLogs.OrderByDescending(a => a.At).Take(count).Select(a => a.ToDto()).ToListAsync();
    }

    public async Task<List<ActivityLogDto>> GetSubscriptionActivityAsync(int subscriptionId)
    {
        return await db.ActivityLogs.Where(a => a.SubscriptionId == subscriptionId).OrderByDescending(a => a.At).Select(a => a.ToDto()).ToListAsync();
    }

    public async Task<List<UserInfo>> GetUsersAsync()
    {
        var users = await db.Users.Include(u => u.Area).ToListAsync();
        var result = new List<UserInfo>();
        foreach (var user in users)
        {
            var roles = await db.UserRoles.Where(ur => ur.UserId == user.Id)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToListAsync();
            result.Add(new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "",
                AreaId = user.AreaId,
                AreaName = user.Area?.Name
            });
        }
        return result;
    }
}
