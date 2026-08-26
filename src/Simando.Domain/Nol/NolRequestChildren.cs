namespace Simando.Domain.Nol;

public sealed class NolRequestPeriod
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid NolRequestCompanyId { get; set; }

    public DateOnly PeriodeMulai { get; set; }
    public DateOnly PeriodeSelesai { get; set; }
    public decimal RataRata { get; set; }
    public decimal KontrakMinimum { get; set; }
    public decimal KontrakMaksimum { get; set; }
    public short SortOrder { get; set; }
}

public sealed class NolRequestDaily
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid NolRequestCompanyId { get; set; }

    public DayOfWeek Hari { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public sealed class NolRequestReference
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid NolRequestCompanyId { get; set; }
    public required Guid ReferenceDocumentId { get; set; }
}
