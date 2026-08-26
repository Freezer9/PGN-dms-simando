namespace Simando.Domain.Directory;

// One column serving both the entry-form label (Posisi Pelanggan) and the
// list-screen label (Jalur Pipa) — no separate jalur_pipa column.
// docs/design/data-model.md#plotting--stage-2.
public enum PosisiPelanggan
{
    Pengembangan,
    JalurExisting,
}
