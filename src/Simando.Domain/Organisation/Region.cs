namespace Simando.Domain.Organisation;

public sealed class Region
{
    public required Guid Id { get; init; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool Active { get; set; } = true;
}
