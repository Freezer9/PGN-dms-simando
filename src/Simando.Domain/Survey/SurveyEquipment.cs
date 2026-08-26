namespace Simando.Domain.Survey;

// Equipment table — repeating, "Insert Row jika > 1". Drives meter/pipe
// sizing via Survey.JumlahKebutuhanEnergi. KapasitasUnitId is scoped to
// UnitSet.Capacity, KonsumsiUnitId to UnitSet.FuelConsumption.
//
// KonversiKeGas is a plain, manually-typed field — no conversion service,
// no calorific-value table. docs/domain/04-prospect-survey.md#the-conversion-engine.
public sealed class SurveyEquipment
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }

    public required string JenisPeralatan { get; set; }
    public decimal? Kapasitas { get; set; }
    public Guid? KapasitasUnitId { get; set; }
    public decimal? JamPerHari { get; set; }
    public short? HariPerMinggu { get; set; }
    public Guid? FuelTypeId { get; set; }
    public decimal? HargaBahanBakar { get; set; }
    public decimal? KonsumsiPerBulan { get; set; }
    public Guid? KonsumsiUnitId { get; set; }
    public required decimal KonversiKeGas { get; set; }
    public required short SortOrder { get; set; }
}
