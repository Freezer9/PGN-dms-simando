namespace Simando.Domain.Audit;

public sealed class BreakGlassAccess
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }
    public required Guid UserId { get; init; }
    public required string Reason { get; init; }
    public required DateTimeOffset RequestedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
