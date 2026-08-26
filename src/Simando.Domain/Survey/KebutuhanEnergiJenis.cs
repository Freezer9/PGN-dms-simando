namespace Simando.Domain.Survey;

// Multi-select — Entry Apps!F64:M64. docs/domain/04-prospect-survey.md#kebutuhan-energi--multi-select--capacityusage.
[Flags]
public enum KebutuhanEnergiJenis
{
    Listrik = 1,
    Steam = 2,
    Panas = 4,
    Dingin = 8,
    Lainnya = 16,
}
