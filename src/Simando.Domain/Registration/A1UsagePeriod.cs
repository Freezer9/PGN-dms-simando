namespace Simando.Domain.Registration;

public sealed class A1UsagePeriod
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid A1RegistrationCompanyId { get; set; }

    public DateOnly PeriodeMulai { get; set; }
    public DateOnly PeriodeSelesai { get; set; }
    public decimal RataRata { get; set; }
    public decimal Minimum { get; set; }
    public decimal Maksimum { get; set; }
    public short SortOrder { get; set; }
}
