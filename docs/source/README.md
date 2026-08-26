# Source Material — Archive

**Every factual claim in these docs traces to a file in this directory.**

The design documents make ~79 citations of the form `Entry Apps!D93` or
`Lampiran 17 §6`. Those citations are only worth something if the sources are
here to check against — which is why they are committed rather than linked.

## Why this matters

Two of these sources are not ours and cannot be re-fetched reliably:

- The **Drive folder** ([`File PGN`](https://drive.google.com/drive/folders/1ep53ML-gCEKHD61NqmgRyImAN4x-dQnR))
  belongs to `rizal.yuniarto.pgn@gmail.com`. If those files are edited or the
  folder is moved, the originals are gone.
- **PGN's procedure `O-001/06.02` is 211 pages and we have never had it.** We have
  17 screenshots of it, embedded in `form-output-data.docx` — and those screenshots
  are the *only* record we hold of the authoritative form definitions. Every
  Lampiran specification in these docs derives from them.

**Ask PGN for the full procedure PDF.** Until then, treat
[`procedure-pages/`](procedure-pages/) as the authority and be aware it is partial.

---

## Contents

### `worksheet.xlsx`
*"1. Worksheet Sistem Sales Tools (1).xlsx"* — the client's functional spec.
Modified 2026-08-04 by `rizal.yuniarto.pgn@gmail.com`.

Seven sheets:

| Sheet | Contains |
|---|---|
| `Tabel` | The six segment names (Bronze 1–3, Silver, Gold, Platinum) |
| `Entry Apps` | **The field spec.** Every field tagged with its pipeline stage 1–8 |
| `Directory Industry` | Stage 1 screen mock, filters, row actions |
| `Data Plotting` | Stage 2 screen mock + the BBTUD allocation widget |
| `Jenis Industri` | 20 industry types with example products |
| `Sheet1` | Short definitions of stages 1–3 |
| `KK0` | The survey form, 17 sections |

### `form-output-data.docx`
*"Form Output Data.docx"* — 17 screenshots of PGN's official procedure, plus
**19 floating text-box shapes the client drew on top of them**.

The annotations are the source for the approval chain
(`Creator → Review 1 → Review 2 → Approval`) and for A1 being wet-signed or signed
digitally — both handled by the same download/sign/re-upload loop. The
screenshots are extracted to `procedure-pages/`.

> ⚠️ **The annotations are shapes, not paragraphs, and position is half the
> meaning.** A plain text extraction returns them as loose sentences with no
> indication of which image — or which *part* of which image — they sit on. Two
> say `Entry Data`, three say `Upload Gambar`; flattened into a list they read as
> noise, and `Entry Data` on Lampiran 17 §8 is the instruction behind the
> 24-hour load profile being built as a digital form, not an upload — see
> [domain/06-nol](../domain/06-nol.md#stage-7--evaluasi-nol). Text **with
> anchor positions** is in
> [`extracts/annotations.txt`](extracts/annotations.txt).

### `procedure-pages/`
The 17 screenshots, cropped to the document area and upscaled 2× for legibility,
named by the page of `O-001/06.02` they show.

`annotated/` holds the eight annotated pages re-rendered with every shape drawn
back in its recorded position — the fastest way to see what a note points at.

| File | Content |
|---|---|
| `p066-diagram-alir-6-1-…` | **The official swimlane process flow.** `Calon Pelanggan \| Area \| SOR \| Fungsi Lain` |
| `p160-lampiran-10-formulir-kk0` | KK0 survey form — *DATA SURVEY PASAR* |
| `p161`, `p162-lampiran-11-…` | Formulir Registrasi Berlangganan Gas (the A1) |
| `p169`, `p170-lampiran-15-…` | Permohonan NOL/RL — Nota Dinas |
| `p171`–`p174-lampiran-16-…` | Penerbitan NOL/RL — Nota Dinas |
| `p175`, `p180`–`p182-lampiran-17-…` | Evaluasi Registrasi Berlangganan Gas |
| `p185`, `p186-resume-evaluasi-…` | Resume Evaluasi / Analisis |
| `lampiran-formulir-daftar-peralatan-gas` | **The official energy-conversion references** |

Page numbers come from each page's own header block, cross-checked against the
PDF viewer's page indicator visible in the screenshot chrome — it runs exactly two
ahead of the document page across all 17. That second source is what numbers
`p169`, which was cropped above its header, and `p165`, the Daftar Peralatan Gas
lampiran.

The same chrome gives us the source filename: **`Final-PO Berlangganan Gas 2023_
o.pdf`**, 213 viewer pages for 211 numbered ones. That is the file to ask PGN
for — by name.

### `extracts/`

Machine-readable extractions, so the spreadsheet can be searched and diffed
without opening Excel.

| File | Contents |
|---|---|
| `sheets.txt` | Every non-empty cell in all seven sheets, **with coordinates** — this is what `Entry Apps!D93` citations resolve against |
| `comments.txt` | **All 54 cell comments** (46 on `Entry Apps`, 4 each on the two screen mocks). These carry the input-type hints — *"Drop down Negara"*, *"Otomatis"*, *"jika tidak ada bisa Input baru"* |
| `formulas.txt` | **All 9 formulas.** Small file, disproportionate importance — see below |
| `annotations.txt` | **All 19 shape annotations**, each with its anchor position and the form section it overlays, plus the confirmed page map |

---

## The nine formulas

These are the highest-value bytes in the whole archive. Plain text extraction of
the spreadsheet loses them entirely, and they encode business rules that appear
nowhere in prose:

```
KK0!K61   =I61*6000/9000        ← the fuel→gas conversion rule
KK0!K62   =I62*9000/9000
KK0!K63   =I63*9000/9000
KK0!K67   =SUM(K61:K66)

Data Plotting!D31  =60/64*D30    ← the allocation conversion factor
Data Plotting!F31  =D31*1000*30  ← ×1000×days
Data Plotting!F32  =D32*1000*30
Data Plotting!F33  =F31/F32      ← utilisation %
Data Plotting!A32  =D30*C31      ← broken leftover, evaluates #VALUE!
```

`K61` is the reason we know conversion is calorific-value based rather than a flat
per-fuel multiplier, and `D31` is the reason `60/64` became a named business constant
instead of a magic number.

---

## What the shape annotations settle

Beyond `Entry Data` (see above), three things only become visible once each
note is tied to what it sits on.

**`FORM 1` · `FORM 2` · `FORM 3` — the client's own scope statement.**
Exactly three of the 17 pages carry a FORM label, and they are the three
data-entry forms:

| Label | Page | Form |
|---|---|---|
| `FORM 1` | p160 | Lampiran 10 — **KK0** survey |
| `FORM 2` | p161 | Lampiran 11 — **A1** registration |
| `FORM 3` | p175 | Lampiran 17 — **Evaluasi** |

Lampiran 15 and 16 (the Nota Dinas) and the Resume Evaluasi carry no label — the
client treats those as **generated output**, not screens to fill.

**`Otomasi Data Tersedia` — the Nota Dinas is assembled, not typed.**
The note covers the entire customer block on Lampiran 15 (`Nama Perusahaan`,
`Nama Pimpinan Perusahaan`, `Alamat Kantor`, `Alamat Pabrik`, `Produksi Utama`,
`Jangka waktu kontrak`). Every one of those comes from the record.

**Reference documents appear twice, and behave differently each time.**
`Drop Down Daftar Dokumen dan atau Entry manual` is anchored to the Nota Dinas
`Menunjuk:` lines on **p169** — a combo box, free text permitted. The five named
*Ketentuan* documents belong to a separate note on **p186**, §8 *Lain-lain*, marked
`Pilih Upload` — a checklist with attachments. Two controls, not one; see
[domain/06](../domain/06-nol.md#reference-documents--two-places-two-behaviours).

---

## Reproducing the extracts

```bash
pip install openpyxl
python tools/extract-sources.py      # if the script is kept
```

Or ad hoc — the extraction is not complicated:

```python
# sheets.txt
import openpyxl
wb = openpyxl.load_workbook('worksheet.xlsx', data_only=True)
for ws in wb.worksheets:
    for row in ws.iter_rows():
        vals = [(c.coordinate, str(c.value).strip())
                for c in row if c.value is not None and str(c.value).strip()]
        if vals: print(f"r{row[0].row} | " + " | ".join(f"{k}={v}" for k, v in vals))

# formulas.txt — same, but load_workbook without data_only
```

Cell comments live in `xl/comments*.xml` inside the `.xlsx` zip and are **dropped
by openpyxl** when the cell is part of a merged range — parse the XML directly.
The docx screenshots are in `word/media/` inside the `.docx` zip.

> Both `.xlsx` and `.docx` are zip archives. `unzip -l` is often the fastest way
> to see what a "text extraction" is throwing away — in this case, 17 images that
> turned out to contain the authoritative form definitions.
