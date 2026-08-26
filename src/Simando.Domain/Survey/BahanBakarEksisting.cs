namespace Simando.Domain.Survey;

// Multi-select — Lampiran 10 §11. Smaller closed set than the fuel_type
// master list (which backs survey_equipment.fuel_type_id): this is the
// existing-fuel checkbox row on the KK0 printout, not the equipment table.
// docs/domain/04-prospect-survey.md#the-official-kk0-form-lampiran-10.
[Flags]
public enum BahanBakarEksisting
{
    Lpg = 1,
    Hsd = 2,
    Mfo = 4,
    Cng = 8,
    Lainnya = 16,
}
