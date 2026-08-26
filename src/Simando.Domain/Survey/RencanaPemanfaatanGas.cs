namespace Simando.Domain.Survey;

// Multi-select ("beri tanda X") — Lampiran 10 §13. docs/domain/04-prospect-survey.md#the-official-kk0-form-lampiran-10.
[Flags]
public enum RencanaPemanfaatanGas
{
    BahanBaku = 1,
    BahanBakar = 2,
    PembangkitListrik = 4,
    Cng = 8,
    TransportasiGas = 16,
    Lainnya = 32,
}
