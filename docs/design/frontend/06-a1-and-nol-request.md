# Frontend 06 — A1 & NOL Request

Stages 5–6. Field definitions in [05-a1-registration.md](../../domain/05-a1-registration.md) and
[06-nol.md](../../domain/06-nol.md).

---

## A1 tab — Registrasi Berlangganan

Maps to **Lampiran 11**. The customer's formal application, and the first point
where money enters.

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  A1 — Registrasi Berlangganan Gas                                                    │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  REGISTRASI                                                                          │
│  Tanggal Registrasi *   [ 20/08/2026 ]     Sumber: Manual                            │
│  Penanggung Jawab *     [ Ir. Hendra Wijaya      ]  Jabatan [ Direktur Operasi    ]  │
│  NPWP *                 [ 01.234.567.8-901.000   ]  ✅ salinan terunggah             │
│                                                                                      │
│  PROFIL PEMASANGAN                                                                   │
│  Status Bangunan *      ( ) Dalam rencana  ( ) Dalam pembangunan                     │
│                         (●) Eksisting      ( ) Proses ekspansi                       │
│  Sektor *               ( ) Komersial  (●) Industri  ( ) Transportasi                │
│                         Produksi utama [ Jasa Laundry                            ]   │
│  Peralatan Gas *        [✓] Boiler  [✓] Dryer  [ ] Oven    [ ] Furnace               │
│                         [ ] Kiln    [ ] Chiller [ ] Gas Engine [ ] Gas Turbine       │
│                         [ ] Air Conditioner   [ ] Lainnya [                      ]   │
│  Tekanan Operasi *      [  4 ] barg                                                  │
│                                                                                      │
│  PEMAKAIAN GAS                                            [ + Tambah periode ]       │
│  ┌───┬─────────────────────────┬───────────┬───────────┬───────────┬──────┐         │
│  │ 1 │[01/01/27]–[31/12/27]    │ Rata-rata │ Minimum   │ Maksimum  │      │         │
│  │   │                         │[  95.000 ]│[  80.000 ]│[ 110.000 ]│ (✕)  │         │
│  ├───┼─────────────────────────┼───────────┼───────────┼───────────┼──────┤         │
│  │ 2 │[01/01/28]–[31/12/28]    │[ 120.000 ]│[ 100.000 ]│[ 140.000 ]│ (✕)  │         │
│  └───┴─────────────────────────┴───────────┴───────────┴───────────┴──────┘         │
│    ℹ️ Beberapa periode dipakai untuk rencana ramp up.        satuan: m³/bulan        │
│                                                                                      │
│  Permohonan Bulan Dimulai *   [ Januari 2027 ▾ ]                                     │
│  Jam Operasi / hari  {{ 24 }}   Hari kerja / minggu  {{ 7 }}   ← dari Survei         │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

**Carry forward, don't re-ask.** `Jam Operasi` and `Hari Kerja` already exist on
the survey; render them as derived values with a link back rather than asking the
rep to retype figures that must match.

### Pricing panel

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  HARGA GAS                                                                           │
│  Basis Kontrak *   ( ) Harian   (●) Bulanan   ( ) Tahunan                            │
│  Skema *           (●) Reguler  ( ) SiGas     ( ) Bersyarat                          │
│                                                                                      │
│  Segmen *          [ Gold                                            ▾ ]             │
│  Kode Harga *      [ GLD-BLN-2026                                    ▾ ] ( + Baru )  │
│  Harga *           [        9,18 ] USD / MMBtu ▾                                     │
│                                                                                      │
│  Perhitungan Capex Awal   Rp [ 3.250.000.000 ]                                       │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

`Segmen`, `Kode Harga` and `Harga` are all plain entry fields — there is no
`Penetapan` (`Otomatis`/`Manual`) toggle and nothing is looked up. `OUP`/`UMP`
are gone too — they existed only to feed a price formula that no source ever
defined, so v1 doesn't collect inputs for a calculation it isn't going to run.

Conditional behaviour:

| Condition | Effect |
|---|---|
| `Skema = SiGas` | **MOM upload becomes required** — a hard gate |
| `Skema = Bersyarat` | `Alasan Kontrak Bersyarat` textarea appears, required |

`Kode Harga` is a dropdown that also accepts new values — *"jika tidak ada bisa
Input baru"* — hence `( + Baru )`. `Harga` has no equivalent dropdown; it's
typed fresh every time, the currency/unit picker matching Lampiran 17's
**`USD…/MMBtu atau Rp…/m³`**.

### Document generation and signing

**One path** ([05-a1-registration](../../domain/05-a1-registration.md#how-a1-actually-works)).
Signing happens outside the application, so the screen's job is to make the
round trip obvious and hard to abandon halfway.

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  DOKUMEN A1                                                                          │
│                                                                                      │
│   ①  Unduh              ②  Tandatangani di luar sistem      ③  Unggah kembali        │
│   ─────────────────────────────────────────────────────────────────────────────────  │
│   [ ⬇ Unduh A1 (.docx) ]   Cetak & tandatangani basah,         [ ⬆ Unggah A1 ]       │
│    diunduh 20/08 14:02      atau tandatangani berkasnya          PDF/JPG · maks 25 MB │
│                             secara digital.                                          │
│                             Berkas boleh disunting dulu.                             │
│                                                                                      │
│   Metode tanda tangan:  ( ) Basah — cetak & pindai   ( ) Digital — berkas ditandatangani│
│                                                                                      │
│   Status: ✅ A1-0000042-ttd.pdf · 1,4 MB · diunggah 20/08/2026 16:40 oleh Budi S.     │
│           ( Lihat dokumen )  ( Ganti )  ( Riwayat versi 2 )                          │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

**`.docx` is the named requirement** — *"Yang word bisa didownload"* — because
staff edit it before printing. No PDF version is generated.

Three things this layout is doing deliberately:

- **Numbered steps, because step ② leaves the application.** The user goes away,
  possibly for days. The panel has to still make sense when they come back, so it
  shows the download timestamp and holds the upload slot open rather than resetting.
- **The status line credits the *uploader*, not a signer.** The system knows who
  uploaded a file and when; it does not know who signed it. Claiming otherwise
  would be a stronger assertion than the evidence supports.
- **`Metode tanda tangan` is a self-declared radio**, recorded as metadata and
  never validated. Useful to know; not a control.

Uses the standard [`<AttachmentUploader>`](11-components.md#attachmentuploader) —
same versioning, same supersede-never-overwrite behaviour as every other document,
which is the point of removing the special case.

---

## NOL tab — Permohonan NOL

Covers both the **Lampiran 17 evaluation** (Area's work) and the **Lampiran 15
Nota Dinas** request.

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Permohonan NOL                                                                      │
├──────────────────────────────────────────────────────────────────────────────────────┤
│  VOLUME KONTRAK                                                                      │
│  (●) Sama dengan A1        ( ) Entry manual                                          │
│  ┌───┬─────────────────────────┬───────────┬─────────────────┬─────────────────┐    │
│  │ 1 │ 01/01/27 – 31/12/27     │ Rata-rata │ Kontrak Minimum │ Kontrak Maksimum│    │
│  │   │                         │  95.000   │      80.000     │     110.000     │    │
│  └───┴─────────────────────────┴───────────┴─────────────────┴─────────────────┘    │
│    ℹ️ Nilai disalin dari A1. Pilih "Entry manual" untuk mengubah.                    │
│                                                                                      │
│  DATA PERUSAHAAN UNTUK NOTA DINAS                                                    │
│  Nama Pimpinan Perusahaan *  [ Bpk. Suryanto Halim                              ]    │
│  Jangka Waktu Kontrak *      [ 5 ] tahun                                             │
│  Alamat Pabrik               {{ Jalan Pemuda 56-58, Gentengkali }} ← Lokasi Pemasangan│
│                                                                                      │
│  BIAYA                                                                               │
│  Biaya Capex Pre GR3      Rp [ 3.180.000.000 ]   ( ⬆ Unggah dokumen )                │
│  Biaya Penyambungan       Reguler Rp [ 350.000.000 ]                                 │
│                           Extra   Rp [  62.500.000 ]                                 │
│                           Jumlah  Rp {{ 412.500.000 }}  (belum termasuk PPN)         │
│                                                                                      │
│  DOKUMEN ACUAN KERJA                                          [ + Tambah dokumen ]   │
│  [✓] Ketentuan Harga Gas — rev. 2026-03                                              │
│  [✓] Ketentuan Biaya Penyambungan — rev. 2025-11                                     │
│                                                                                      │
│  LAMPIRAN WAJIB                                                                      │
│  ✅ 1. A1 (tertandatangan)          ✅ 2. KK0 (tertandatangan)                        │
│  ✅ 3. Bukti Kelayakan              ⬜ 4. Gambar Situasi Pabrik                       │
│  ⬜ 5. Gambar Pipa Eksisting        ⬜ 6. Usulan Titik Taping                         │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

### Notes

- **`Sama dengan A1` is a stored flag, not just a copy action.** It records
  *whether the request matches what the customer applied for* — and when it
  doesn't, that difference is what reviewers examine. Switching to `Entry manual`
  unlocks the fields and keeps the flag `false`.
- **`Biaya Penyambungan — Jumlah` is computed** and marked *(belum termasuk PPN)*,
  matching Lampiran 17 §7.
- Pricing repeats the A1 panel exactly — same fields, same manual entry, no
  toggle at either stage.
- Reference documents attach the **versioned** policy that governed the decision,
  which is what makes it auditable years later.
- The Lampiran 17 sections that are narrative + image (Analisis komersial, Analisis
  kompetisi, Gambar situasi, Titik taping) render as description + upload pairs.

### Daily contract basis

Hidden unless `Basis Kontrak = Harian` — see
[docs/future](../../future/README.md#daily-contract-basis-harian). When shown:

```
┌────────────────────────────────────────────────────────────────────┐
│  PEMAKAIAN PER HARI KONTRAK                                        │
│  ┌────┬───────────┬──────────────────┬──────────────────┐          │
│  │ 1  │ Senin     │ Min [         ]  │ Maks [        ]  │          │
│  │ 2  │ Selasa    │ Min [         ]  │ Maks [        ]  │          │
│  │ …  │ …         │                  │                  │          │
│  │ 7  │ Minggu    │ Min [         ]  │ Maks [        ]  │          │
│  └────┴───────────┴──────────────────┴──────────────────┘          │
└────────────────────────────────────────────────────────────────────┘
```

Seven fixed rows, never add/remove.

---

## Submitting

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Terakhir disimpan 15:07        ( Simpan Draf )   [ Ajukan untuk Persetujuan ]       │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

Clicking submit opens a confirmation that **states the chain**, so the creator
knows what they are starting:

```
┌────────────────────────────────────────────────────────────────────┐
│  Ajukan untuk Persetujuan?                                         │
│                                                                    │
│  PT Indonesia 1945 · 0000042-35-78                                 │
│                                                                    │
│  Alur persetujuan:                                                 │
│    1. Area Head — Rudi H.                                          │
│    2. Admin Regional — Sari W.                                     │
│    3. Reviewer (ditetapkan oleh Admin Regional)                    │
│    4. Division Head                                                │
│                                                                    │
│  Setelah diajukan, record tidak dapat diubah kecuali dikembalikan  │
│  melalui Revisi.                                                   │
│                                                                    │
│                                    ( Batal )   [ Ya, Ajukan ]      │
└────────────────────────────────────────────────────────────────────┘
```

The second sentence matters: editing locks on submit, and users should learn
that from a dialog rather than from a support ticket.
