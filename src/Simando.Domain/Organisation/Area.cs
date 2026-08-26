namespace Simando.Domain.Organisation;

public sealed class Area
{
    public required Guid Id { get; init; }
    public required Guid RegionId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool Active { get; set; } = true;
}
