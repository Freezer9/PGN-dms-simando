using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Simando.Application.Documents;
using Simando.Domain.Nol;
using Simando.Domain.Registration;
using Simando.Domain.Survey;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Documents;

internal sealed class DocxDocumentGenerator(IDbContextFactory<SimandoDbContext> dbContextFactory)
    : IDocumentGenerator
{
    public async Task<byte[]> GenerateKk0DocxAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct)
            ?? throw new InvalidOperationException($"Company with ID {companyId} not found.");

        var areaName = await db.Areas.AsNoTracking()
            .Where(a => a.Id == company.AreaId)
            .Select(a => a.Name)
            .FirstOrDefaultAsync(ct) ?? "AREA";

        var industryTypeName = await db.IndustryTypes.AsNoTracking()
            .Where(i => i.Id == company.IndustryTypeId)
            .Select(i => i.Name)
            .FirstOrDefaultAsync(ct) ?? "-";

        var contacts = await db.CompanyContacts.IgnoreQueryFilters().AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var survey = await db.Surveys.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(s => s.CompanyId == companyId, ct);
        var products = await db.SurveyProducts.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        var rawMaterials = await db.SurveyRawMaterials.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.CompanyId == companyId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(ct);

        var markets = await db.SurveyMarkets.IgnoreQueryFilters().AsNoTracking()
            .Where(m => m.CompanyId == companyId)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);

        var equipmentList = await db.SurveyEquipment.IgnoreQueryFilters().AsNoTracking()
            .Where(e => e.CompanyId == companyId)
            .OrderBy(e => e.SortOrder)
            .ToListAsync(ct);

        var countries = await db.Countries.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name, ct);
        var uoms = await db.UnitsOfMeasure.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.Name, ct);
        var fuelTypes = await db.FuelTypes.AsNoTracking().ToDictionaryAsync(f => f.Id, f => f.Name, ct);

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var body = DocxBuilderHelper.CreateDocumentBody(doc);

            // Document Control Box Header
            var tglSurvey = survey?.TanggalSurvey?.ToString("d MMMM yyyy") ?? DateTime.UtcNow.ToString("d MMMM yyyy");
            DocxBuilderHelper.AppendPgnControlHeaderBox(body, "FM-PGN-SALES-KK0-01", "01", tglSurvey, "1 dari 1");

            var docNumber = $"No. {company.NomorSeq:D4}/KK0/AREA-{areaName.ToUpperInvariant()}/{DateTime.UtcNow.Year}";
            DocxBuilderHelper.AppendTitle(body, "FORMULIR KK0 - DATA SURVEY PASAR", docNumber, areaName, "Lampiran 10: Formulir KK0");

            // Section 1: Data Umum Perusahaan
            DocxBuilderHelper.AppendSectionHeader(body, "1. Data Umum Perusahaan");
            var lonLat = company.Location != null ? $"{company.Location.X:F6}, {company.Location.Y:F6}" : "-";
            var picNames = contacts.Count > 0 ? string.Join(", ", contacts.Select(c => $"{c.Nama} ({c.Jabatan})")) : "-";

            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Nama Perusahaan", company.NamaPerusahaan),
                ("Nomor Register", company.Nomor),
                ("Alamat Kantor", company.Alamat),
                ("Lokasi Pemasangan", company.Alamat),
                ("No. Telepon / Fax", $"{company.Telp ?? "-"} / {company.Fax ?? "-"}"),
                ("Email", company.Email),
                ("NPWP", company.Npwp),
                ("Titik Koordinat (Long, Lat)", lonLat),
                ("Jenis Usaha", industryTypeName),
                ("Person In Charge (PIC)", picNames)
            ]);

            // Section 2: Operasional
            DocxBuilderHelper.AppendSectionHeader(body, "2. Data Operasional Pabrik");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Jumlah Karyawan Total", $"{survey?.JumlahKaryawan ?? 0} Orang"),
                ("Jumlah Shift", $"{survey?.JumlahShift ?? 0} Shift/Hari"),
                ("Jam Kerja Operasi", $"{survey?.JamKerjaPerHari ?? 0} Jam/Hari, {survey?.HariPerMinggu ?? 0} Hari/Minggu")
            ]);

            // Section 3: Hasil Produksi Table
            DocxBuilderHelper.AppendSectionHeader(body, "3. Hasil Produksi Utama");
            var prodHeaders = new[] { "No", "Produk Utama", "Kapasitas / Tahun", "Harga Produk", "Catatan" };
            var prodRows = products.Select((p, idx) => new[]
            {
                (idx + 1).ToString(),
                p.Produk,
                p.Kapasitas.HasValue ? $"{p.Kapasitas.Value:N0} Kaps/Tahun" : "-",
                p.HargaProduk.HasValue ? $"Rp {p.HargaProduk.Value:N0}" : "-",
                p.Catatan ?? "-"
            }).ToList();
            if (prodRows.Count == 0) prodRows.Add(new[] { "1", "-", "-", "-", "-" });
            body.AppendChild(DocxBuilderHelper.CreateTable(prodHeaders, prodRows));

            // Section 4: Bahan Baku Table
            DocxBuilderHelper.AppendSectionHeader(body, "4. Kebutuhan Bahan Baku");
            var matHeaders = new[] { "No", "Bahan Baku", "Asal", "Negara", "Volume / Bulan" };
            var matRows = rawMaterials.Select((r, idx) => new[]
            {
                (idx + 1).ToString(),
                r.Bahan ?? "-",
                r.Asal?.ToString() ?? "-",
                r.CountryId.HasValue ? countries.GetValueOrDefault(r.CountryId.Value, "-") : "-",
                r.Volume.HasValue ? $"{r.Volume.Value:N0} {uoms.GetValueOrDefault(r.SatuanUnitId ?? Guid.Empty, "")}" : "-"
            }).ToList();
            if (matRows.Count == 0) matRows.Add(new[] { "1", "-", "-", "-", "-" });
            body.AppendChild(DocxBuilderHelper.CreateTable(matHeaders, matRows));

            // Section 5: Orientasi Pasar Table
            DocxBuilderHelper.AppendSectionHeader(body, "5. Orientasi Pasar");
            var mktHeaders = new[] { "No", "Pasar", "Asal", "Negara Tujuan", "Volume / Bulan" };
            var mktRows = markets.Select((m, idx) => new[]
            {
                (idx + 1).ToString(),
                m.Bahan ?? "-",
                m.Asal?.ToString() ?? "-",
                m.CountryId.HasValue ? countries.GetValueOrDefault(m.CountryId.Value, "-") : "-",
                m.Volume.HasValue ? $"{m.Volume.Value:N0} {uoms.GetValueOrDefault(m.SatuanUnitId ?? Guid.Empty, "")}" : "-"
            }).ToList();
            if (mktRows.Count == 0) mktRows.Add(new[] { "1", "-", "-", "-", "-" });
            body.AppendChild(DocxBuilderHelper.CreateTable(mktHeaders, mktRows));

            // Section 6: Kebutuhan Energi & Eksisting
            DocxBuilderHelper.AppendSectionHeader(body, "6. Kebutuhan Energi & Bahan Bakar Eksisting");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Kebutuhan Energi", survey?.KebutuhanEnergi?.ToString() ?? "-"),
                ("Bahan Bakar Eksisting", survey?.BahanBakarEksisting?.ToString() ?? "-"),
                ("Nama Pemasok", survey?.NamaPemasok ?? "-"),
                ("Kapasitas Listrik", $"{survey?.KapasitasListrikKw ?? 0} KW"),
                ("Pemakaian Listrik", $"{survey?.PemakaianListrikKwh ?? 0} KWh"),
                ("Min. Efisiensi Diharapkan", $"{survey?.MinEfisiensiDiharapkanPct ?? 0} %")
            ]);

            // Section 7: Perincian Bahan Bakar & Konversi Ke Gas Table
            DocxBuilderHelper.AppendSectionHeader(body, "7. Perincian Bahan Bakar Peralatan & Konversi Ke Gas");
            var eqHeaders = new[] { "No", "Jenis Peralatan", "Kapasitas/Jam", "Pola Operasi", "Bahan Bakar", "Konsumsi/Bulan", "Konversi Ke Gas (MMBtu/Bulan)" };
            var eqRows = equipmentList.Select((e, idx) => new[]
            {
                (idx + 1).ToString(),
                e.JenisPeralatan,
                e.Kapasitas.HasValue ? $"{e.Kapasitas.Value:N0} {uoms.GetValueOrDefault(e.KapasitasUnitId ?? Guid.Empty, "")}" : "-",
                $"{e.JamPerHari ?? 0}h/d, {e.HariPerMinggu ?? 0}d/w",
                e.FuelTypeId.HasValue ? fuelTypes.GetValueOrDefault(e.FuelTypeId.Value, "-") : "-",
                e.KonsumsiPerBulan.HasValue ? $"{e.KonsumsiPerBulan.Value:N0} {uoms.GetValueOrDefault(e.KonsumsiUnitId ?? Guid.Empty, "")}" : "-",
                $"{e.KonversiKeGas:N2}"
            }).ToList();

            var totalKonversi = survey?.JumlahKebutuhanEnergi ?? equipmentList.Sum(e => e.KonversiKeGas);
            eqRows.Add(new[] { "", "JUMLAH KEBUTUHAN ENERGI", "", "", "", "", $"{totalKonversi:N2} MMBtu/Bulan" });

            body.AppendChild(DocxBuilderHelper.CreateTable(eqHeaders, eqRows));

            // Section 8: Pipa Gas Terdekat & Rencana Pemanfaatan
            DocxBuilderHelper.AppendSectionHeader(body, "8. Informasi Pipa Terdekat & Rencana Pemanfaatan");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Estimasi Jarak Pipa Terdekat", $"{survey?.PipaTerdekatJarakM ?? 0} Meter"),
                ("Diameter Pipa Terdekat", $"{survey?.PipaTerdekatDiameter ?? 0} Inch"),
                ("Tekanan Pipa Terdekat", $"{survey?.PipaTerdekatTekanan ?? 0} Barg"),
                ("Rencana Pemanfaatan Gas", survey?.RencanaPemanfaatanGas?.ToString() ?? "-"),
                ("Deskripsi Proses Produksi", survey?.DeskripsiProsesProduksi ?? "-"),
                ("Keterangan Lain", survey?.KeteranganLain ?? "-")
            ]);

            // Section 9: Signature Block
            var picName = contacts.FirstOrDefault()?.Nama ?? company.NamaPerusahaan;
            DocxBuilderHelper.AppendSignatureBlock(
                body,
                "PETUGAS SURVEI PGN",
                "Tim Survei PGN Tbk",
                "PEMBERI DATA / CALON PELANGGAN",
                picName
            );

            doc.Save();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateA1DocxAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct)
            ?? throw new InvalidOperationException($"Company with ID {companyId} not found.");

        var areaName = await db.Areas.AsNoTracking()
            .Where(a => a.Id == company.AreaId)
            .Select(a => a.Name)
            .FirstOrDefaultAsync(ct) ?? "AREA";

        var a1 = await db.A1Registrations.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(a => a.CompanyId == companyId, ct);
        var periods = await db.A1UsagePeriods.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.A1RegistrationCompanyId == companyId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        var segmentName = a1?.SegmentId != null
            ? await db.Segments.AsNoTracking().Where(s => s.Id == a1.SegmentId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct) ?? "-"
            : "-";

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var body = DocxBuilderHelper.CreateDocumentBody(doc);

            // PGN Control Header Box
            var tglReg = a1?.TanggalRegistrasi?.ToString("d MMMM yyyy") ?? DateTime.UtcNow.ToString("d MMMM yyyy");
            DocxBuilderHelper.AppendPgnControlHeaderBox(body, "O-001/06.02", "01", tglReg, "1 dari 1");

            var docNumber = $"No. {company.NomorSeq:D4}/REG/A1/{DateTime.UtcNow.Year}";
            DocxBuilderHelper.AppendTitle(body, "FORMULIR REGISTRASI BERLANGGANAN GAS (A1)", docNumber, areaName, "Lampiran 11: Formulir Registrasi Berlangganan Gas");

            // Document Header Block
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Tanggal Registrasi", tglReg),
                ("Sumber Registrasi", a1?.RegistrasiSource.ToString() ?? RegistrasiSource.Manual.ToString())
            ]);

            // Section 1: Data Calon Pelanggan
            DocxBuilderHelper.AppendSectionHeader(body, "1. Data Calon Pelanggan");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Nama Perusahaan / Grup", company.NamaPerusahaan),
                ("Nomor Register", company.Nomor),
                ("Penanggung Jawab", a1?.NamaPenanggungJawab ?? "-"),
                ("Jabatan", a1?.Jabatan ?? "-"),
                ("Alamat Kantor", company.Alamat),
                ("Lokasi Pemasangan", company.Alamat),
                ("No. Telepon / Fax", $"{company.Telp ?? "-"} / {company.Fax ?? "-"}"),
                ("Email", company.Email),
                ("NPWP", company.Npwp),
                ("Kode Pos", company.KodePos)
            ]);

            // Section 2: Status Bangunan & Peralatan Gas
            DocxBuilderHelper.AppendSectionHeader(body, "2. Status Bangunan & Peralatan Gas");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Status Bangunan saat ini", a1?.StatusBangunan?.ToString() ?? "-"),
                ("Perkiraan Tanggal Dimulai", a1?.BulanDimulai?.ToString("MMMM yyyy") ?? "-"),
                ("Sektor Usaha", a1?.Sektor?.ToString() ?? "-"),
                ("Produksi Utama", a1?.ProduksiUtama ?? "-"),
                ("Jenis Peralatan Gas", a1?.JenisPeralatanGas ?? "-"),
                ("Tekanan Operasi (barg)", a1?.TekananOperasiBarg.HasValue == true ? $"{a1.TekananOperasiBarg.Value:N2} barg" : "-")
            ]);

            // Section 3: Ketentuan Kontrak & Skema Harga
            DocxBuilderHelper.AppendSectionHeader(body, "3. Ketentuan Kontrak & Skema Harga");
            var priceText = a1?.HargaNilai.HasValue == true
                ? $"{a1.HargaCurrency} {a1.HargaNilai.Value:N2} / {a1.HargaUnit}"
                : "-";

            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Basis Kontrak", a1?.BasisKontrak?.ToString() ?? "-"),
                ("Skema Harga", a1?.SkemaHarga?.ToString() ?? "-"),
                ("Segment", segmentName),
                ("Kode Harga", a1?.KodeHarga ?? "-"),
                ("Harga Gas", priceText),
                ("Perkiraan Capex Awal", a1?.CapexAwal.HasValue == true ? $"Rp {a1.CapexAwal.Value:N0}" : "-"),
                ("MOM SiGas Tersedia", a1?.MomSigasTersedia == true ? "Ya" : "Tidak")
            ]);

            // Section 4: Tabel Rencana Pemakaian Gas (Usage Periods Table)
            DocxBuilderHelper.AppendSectionHeader(body, "4. Tabel Rencana Pemakaian Gas (Ramp-up Schedule)");
            var periodHeaders = new[] { "No", "Periode Mulai", "Periode Selesai", "Rata-rata (MMBtu)", "Minimum (MMBtu)", "Maksimum (MMBtu)" };
            var periodRows = periods.Select((p, idx) => new[]
            {
                (idx + 1).ToString(),
                p.PeriodeMulai.ToString("dd MMM yyyy"),
                p.PeriodeSelesai.ToString("dd MMM yyyy"),
                $"{p.RataRata:N2}",
                $"{p.Minimum:N2}",
                $"{p.Maksimum:N2}"
            }).ToList();
            if (periodRows.Count == 0) periodRows.Add(new[] { "1", "-", "-", "-", "-", "-" });
            body.AppendChild(DocxBuilderHelper.CreateTable(periodHeaders, periodRows));

            // Section 5: Closing Statement & Signature Block
            DocxBuilderHelper.AppendSectionHeader(body, "5. Pernyataan & Tanda Tangan");
            DocxBuilderHelper.AppendKeyValueParagraph(body, "Pernyataan", "Demikian registrasi berlangganan gas ini kami ajukan dengan sebenarnya untuk proses lebih lanjut.", true);

            var pjName = a1?.NamaPenanggungJawab ?? company.NamaPerusahaan;
            var pjJabatan = a1?.Jabatan ?? "Penanggung Jawab";
            DocxBuilderHelper.AppendSignatureBlock(
                body,
                "CALON PELANGGAN",
                $"{pjName}\n{pjJabatan}",
                "MENGETAHUI PGN",
                "Sales Area Manager PGN Tbk"
            );

            doc.Save();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateNolRequestDocxAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct)
            ?? throw new InvalidOperationException($"Company with ID {companyId} not found.");

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        var regionName = area != null
            ? await db.Regions.AsNoTracking().Where(r => r.Id == area.RegionId).Select(r => r.Name).FirstOrDefaultAsync(ct) ?? "REGIONAL"
            : "REGIONAL";
        var areaName = area?.Name ?? "AREA";

        var nol = await db.NolRequests.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(n => n.CompanyId == companyId, ct);
        var periods = await db.NolRequestPeriods.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.NolRequestCompanyId == companyId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        var refIds = await db.NolRequestReferences.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.NolRequestCompanyId == companyId)
            .Select(r => r.ReferenceDocumentId)
            .ToListAsync(ct);

        var referenceTitles = await db.ReferenceDocuments.AsNoTracking()
            .Where(rd => refIds.Contains(rd.Id))
            .Select(rd => rd.Name)
            .ToListAsync(ct);

        var a1 = await db.A1Registrations.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(a => a.CompanyId == companyId, ct);

        var segmentName = nol?.SegmentId != null
            ? await db.Segments.AsNoTracking().Where(s => s.Id == nol.SegmentId.Value).Select(s => s.Name).FirstOrDefaultAsync(ct) ?? "-"
            : "-";

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var body = DocxBuilderHelper.CreateDocumentBody(doc);

            // PGN Control Header Box
            DocxBuilderHelper.AppendPgnControlHeaderBox(body, "O-001/06.02", "01", DateTime.UtcNow.ToString("d MMMM yyyy"), "1 dari 1");

            var docNumber = nol?.NomorNotaDinas ?? $"No. {company.NomorSeq:D4}/ND/NOL/AREA-{areaName.ToUpperInvariant()}/{DateTime.UtcNow.Year}";
            DocxBuilderHelper.AppendTitle(body, "NOTA DINAS PERMOHONAN NOL / RL", docNumber, areaName, "Lampiran 15: Permohonan NOL/RL");

            // Memorandum Address Block
            DocxBuilderHelper.AppendSectionHeader(body, "MEMORANDUM NOTA DINAS");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Kepada Yth.", $"Direktur Komersial / GM SOR {regionName.ToUpperInvariant()}"),
                ("Dari", $"Area Head PGN Area {areaName}"),
                ("Hal", $"Permohonan Penerbitan NOL/RL Registrasi Berlangganan Gas PT {company.NamaPerusahaan}"),
                ("Sifat", "Segera"),
                ("Lampiran", "1 (satu) Berkas")
            ]);

            // Document References Block (Menunjuk)
            DocxBuilderHelper.AppendSectionHeader(body, "Menunjuk (Acuan Dokumen)");
            if (referenceTitles.Count > 0)
            {
                var refList = referenceTitles.Select((title, idx) => ($"{idx + 1}.", (string?)title)).ToList();
                DocxBuilderHelper.AppendKeyValueTable(body, refList);
            }
            else
            {
                DocxBuilderHelper.AppendKeyValueTable(body,
                [
                    ("1.", $"Surat Registrasi Berlangganan Gas A1 No. {company.NomorSeq:D4}/REG/A1"),
                    ("2.", $"Hasil Survey Pasar KK0 No. {company.NomorSeq:D4}/KK0"),
                    ("3.", "Ketentuan Prosedur PGN yang berlaku")
                ]);
            }

            // Customer Data Block
            DocxBuilderHelper.AppendSectionHeader(body, "1. Data Calon Pelanggan");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Nama Perusahaan", company.NamaPerusahaan),
                ("Nomor Register", company.Nomor),
                ("Nama Pimpinan / PJ", nol?.NamaPimpinanPerusahaan ?? a1?.NamaPenanggungJawab ?? company.NamaPerusahaan),
                ("Jabatan", a1?.Jabatan ?? "Penanggung Jawab"),
                ("Alamat Kantor", company.Alamat),
                ("Alamat Pabrik / Lokasi Pemasangan", company.Alamat),
                ("Jangka Waktu Kontrak Rencana", nol?.JangkaWaktuKontrak ?? "1 (satu) Tahun")
            ]);

            // Requested Gas Subscription Parameters
            DocxBuilderHelper.AppendSectionHeader(body, "2. Permohonan Berlangganan Gas & Ketentuan");
            var priceText = nol?.HargaNilai.HasValue == true
                ? $"{nol.HargaCurrency} {nol.HargaNilai.Value:N2} / {nol.HargaUnit}"
                : "-";

            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Sesuai Data A1", nol?.SamaDenganA1 == true ? "Sama dengan A1" : "Entri Khusus NOL"),
                ("Basis Kontrak", nol?.BasisKontrak?.ToString() ?? "-"),
                ("Skema Harga", nol?.SkemaHarga?.ToString() ?? "-"),
                ("Segment", segmentName),
                ("Kode Harga", nol?.KodeHarga ?? "-"),
                ("Harga Gas", priceText),
                ("Alasan Kontrak Bersyarat", nol?.AlasanKontrakBersyarat ?? "-"),
                ("Permohonan Bulan Dimulai", nol?.BulanDimulai?.ToString("MMMM yyyy") ?? "-")
            ]);

            // Ramp-up Usage Period Table
            DocxBuilderHelper.AppendSectionHeader(body, "3. Tabel Rencana Pemakaian Gas (Commitment Schedule)");
            var periodHeaders = new[] { "No", "Periode Mulai", "Periode Selesai", "Rata-rata (MMBtu)", "Kontrak Min (MMBtu)", "Kontrak Maks (MMBtu)" };
            var periodRows = periods.Select((p, idx) => new[]
            {
                (idx + 1).ToString(),
                p.PeriodeMulai.ToString("dd MMM yyyy"),
                p.PeriodeSelesai.ToString("dd MMM yyyy"),
                $"{p.RataRata:N2}",
                $"{p.KontrakMinimum:N2}",
                $"{p.KontrakMaksimum:N2}"
            }).ToList();
            if (periodRows.Count == 0) periodRows.Add(new[] { "1", "-", "-", "-", "-", "-" });
            body.AppendChild(DocxBuilderHelper.CreateTable(periodHeaders, periodRows));

            // Connection Fees & Capex
            DocxBuilderHelper.AppendSectionHeader(body, "4. Estimasi Capex & Biaya Penyambungan");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Capex Pre-GR3", nol?.CapexPreGr3.HasValue == true ? $"Rp {nol.CapexPreGr3.Value:N0}" : "-"),
                ("Biaya Penyambungan Reguler", nol?.BiayaPenyambunganReguler.HasValue == true ? $"Rp {nol.BiayaPenyambunganReguler.Value:N0}" : "-"),
                ("Biaya Penyambungan Extra", nol?.BiayaPenyambunganExtra.HasValue == true ? $"Rp {nol.BiayaPenyambunganExtra.Value:N0}" : "-"),
                ("Total Biaya Penyambungan", $"Rp {nol?.BiayaPenyambunganJumlah ?? 0:N0} (Belum termasuk PPN)")
            ]);

            // Closing & Signatures
            DocxBuilderHelper.AppendSectionHeader(body, "5. Penutup");
            DocxBuilderHelper.AppendKeyValueParagraph(body, "Pernyataan", "Demikian Nota Dinas permohonan ini disampaikan sebagai bahan pertimbangan Bapak untuk menerbitkan Surat Pernyataan Tidak Keberatan (No Objection Letter) untuk calon Pelanggan tersebut di atas.", true);

            DocxBuilderHelper.AppendSignatureBlock(
                body,
                "DUSUKUSAN / PENGUSUL",
                $"Area Head PGN Area {areaName}",
                "MENGETAHUI",
                $"General Manager SOR {regionName}"
            );

            doc.Save();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateEvaluationResumeDocxAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct)
            ?? throw new InvalidOperationException($"Company with ID {companyId} not found.");

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        var regionName = area != null
            ? await db.Regions.AsNoTracking().Where(r => r.Id == area.RegionId).Select(r => r.Name).FirstOrDefaultAsync(ct) ?? "REGIONAL"
            : "REGIONAL";
        var areaName = area?.Name ?? "AREA";

        var nol = await db.NolRequests.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(n => n.CompanyId == companyId, ct);
        var eval = nol != null
            ? await db.NolEvaluations.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(e => e.NolRequestId == companyId, ct)
            : null;

        var scenarios = eval != null
            ? await db.NolEvaluationScenarios.IgnoreQueryFilters().AsNoTracking()
                .Where(s => s.NolEvaluationNolRequestId == companyId)
                .ToListAsync(ct)
            : [];

        var evaluatorName = eval?.EvaluatedBy != null
            ? await db.Users.AsNoTracking().Where(u => u.Id == eval.EvaluatedBy.Value).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Regional Admin"
            : "Regional Admin";

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var body = DocxBuilderHelper.CreateDocumentBody(doc);

            // PGN Control Header Box
            var tglEval = eval?.EvaluatedAt?.ToString("d MMMM yyyy") ?? DateTime.UtcNow.ToString("d MMMM yyyy");
            DocxBuilderHelper.AppendPgnControlHeaderBox(body, "O-001/06.02", "01", tglEval, "1 dari 1");

            var docNumber = $"No. {company.NomorSeq:D4}/RES-EVAL/REGIONAL-{regionName.ToUpperInvariant()}/{DateTime.UtcNow.Year}";
            DocxBuilderHelper.AppendTitle(body, "RESUME EVALUASI KELAYAKAN COMMERCIAL & TECHNICAL", docNumber, areaName, "Lampiran 17: Evaluasi Registrasi Berlangganan Gas");

            // Document Header Block
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Tanggal Evaluasi", tglEval),
                ("Didesain oleh", evaluatorName)
            ]);

            // Section 1: Data Umum & Status RKAP
            DocxBuilderHelper.AppendSectionHeader(body, "1. Data Calon Pelanggan & Status RKAP");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Nama Perusahaan", company.NamaPerusahaan),
                ("Nomor Register", company.Nomor),
                ("Sales Area / Region", $"{areaName} / {regionName}"),
                ("Status Capel di RKAP", eval?.StatusRkap?.ToString() ?? "-"),
                ("Status FEED Checkpoint", $"{eval?.FeedStatus} (Selesai: {eval?.FeedCompletedAt?.ToString("dd MMM yyyy") ?? "-"})")
            ]);

            // Section 2: Data Technical & Gate Review
            DocxBuilderHelper.AppendSectionHeader(body, "2. Analisis Teknikal & Spesifikasi Infrastruktur");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Capex Final", eval?.CapexFinal.HasValue == true ? $"Rp {eval.CapexFinal.Value:N0}" : "-"),
                ("Pipa Induk", eval?.PipaIndukPanjangM.HasValue == true ? $"{eval.PipaIndukPanjangM.Value:N0} m, Diameter {eval.PipaIndukDiameter} {eval.PipaIndukDiameterUnit}" : "-"),
                ("Pipa Service", eval?.PipaServicePanjangM.HasValue == true ? $"{eval.PipaServicePanjangM.Value:N0} m, Diameter {eval.PipaServiceDiameter} {eval.PipaServiceDiameterUnit}" : "-"),
                ("Spesifikasi MRS", eval?.SpesifikasiMrs ?? "-"),
                ("G-Size MR/S", eval?.GSize ?? "-"),
                ("Tekanan (barg)", eval?.Tekanan.HasValue == true ? $"{eval.Tekanan.Value:N2} barg" : "-"),
                ("Maks Flowrate", eval?.MaksFlowrate.HasValue == true ? $"{eval.MaksFlowrate.Value:N2} m3/jam" : "-"),
                ("Maks Kapasitas Meter", eval?.MaksKapasitasMeterM3Jam.HasValue == true ? $"{eval.MaksKapasitasMeterM3Jam.Value:N2} m3/jam" : "-"),
                ("Durasi Pelaksanaan", eval?.DurasiPelaksanaanBulan.HasValue == true ? $"{eval.DurasiPelaksanaanBulan.Value} Bulan" : "-")
            ]);

            // Section 3: Skema Pembayaran & Jaminan
            DocxBuilderHelper.AppendSectionHeader(body, "3. Skema Pembayaran & Jaminan Pembayaran");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Skema Pembayaran", eval?.SkemaPembayaran?.ToString() ?? "-"),
                ("Status Jaminan Pembayaran", eval?.JaminanStatus ?? "-"),
                ("Jenis Jaminan", eval?.JaminanJenis ?? "-"),
                ("Masa Berlaku", eval?.JaminanMasaBerlaku ?? "-"),
                ("Penerbit Jaminan", eval?.JaminanPenerbit ?? "-")
            ]);

            // Section 4: Pasokan & Analisis Pasar
            DocxBuilderHelper.AppendSectionHeader(body, "4. Pasokan Gas & Analisis Pasar");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Ketersediaan Pasokan", eval?.KetersediaanPasokanBbtud.HasValue == true ? $"{eval.KetersediaanPasokanBbtud.Value:N2} BBTUD" : "-"),
                ("Radius Kompetitor", eval?.RadiusKompetitorKm.HasValue == true ? $"{eval.RadiusKompetitorKm.Value:N1} Km" : "-"),
                ("Analisis Komersial", eval?.AnalisisKomersial ?? "-"),
                ("Analisis Kompetitor", eval?.AnalisisKompetitor ?? "-")
            ]);

            // Section 5: Analisis Kelayakan Scenarios Table (IRR + Payback only per Lampiran 17 spec)
            DocxBuilderHelper.AppendSectionHeader(body, "5. Hasil Analisis Kelayakan & Skenario Finansial");
            var scHeaders = new[] { "No", "Skenario Label", "IRR (%)", "Payback (Tahun)", "Hasil Analisis" };
            var scRows = scenarios.Select((s, idx) => new[]
            {
                (idx + 1).ToString(),
                s.Label,
                s.IrrPct.HasValue ? $"{s.IrrPct.Value:N2} %" : "-",
                s.PaybackYears.HasValue ? $"{s.PaybackYears.Value:N1} Thn" : "-",
                s.HasilAnalisis ?? "-"
            }).ToList();
            if (scRows.Count == 0) scRows.Add(new[] { "1", "Skenario Utama", "-", "-", "Sesuai parameter kelayakan" });
            body.AppendChild(DocxBuilderHelper.CreateTable(scHeaders, scRows));

            // Section 6: Kesimpulan & Rekomendasi
            DocxBuilderHelper.AppendSectionHeader(body, "6. Kesimpulan & Rekomendasi Evaluasi");
            DocxBuilderHelper.AppendKeyValueParagraph(body, "Kesimpulan Evaluasi", eval?.Kesimpulan ?? "Permohonan NOL Berlangganan Gas Layak untuk Disetujui.", true);

            // Section 7: Signature Block
            DocxBuilderHelper.AppendSignatureBlock(
                body,
                "TIM EVALUATOR REGIONAL",
                $"Fungsi Sales & Customer Management {regionName}",
                "MENGETAHUI & MENYETUJUI",
                $"General Manager SOR {regionName}"
            );

            doc.Save();
        }

        return ms.ToArray();
    }

    public async Task<byte[]> GenerateNolIssuanceDocxAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, ct)
            ?? throw new InvalidOperationException($"Company with ID {companyId} not found.");

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        var regionName = area?.RegionId != null
            ? await db.Regions.AsNoTracking().Where(r => r.Id == area.RegionId).Select(r => r.Name).FirstOrDefaultAsync(ct) ?? "REGIONAL"
            : "REGIONAL";
        var areaName = area?.Name ?? "AREA";

        var nol = await db.NolRequests.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(n => n.CompanyId == companyId, ct);
        var issuance = await db.NolIssuances.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(i => i.NolRequestId == companyId, ct);
        var terms = await db.NolIssuanceApprovedTerms.IgnoreQueryFilters().AsNoTracking()
            .Where(t => t.NolIssuanceNolRequestId == companyId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync(ct);

        var signerName = issuance?.SignedByUserId != null
            ? await db.Users.AsNoTracking().Where(u => u.Id == issuance.SignedByUserId.Value).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Division Head"
            : "Division Head";

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var body = DocxBuilderHelper.CreateDocumentBody(doc);

            // PGN Control Header Box
            var tglSign = issuance?.SignedAt?.ToString("d MMMM yyyy") ?? DateTime.UtcNow.ToString("d MMMM yyyy");
            DocxBuilderHelper.AppendPgnControlHeaderBox(body, "O-001/06.02", "01", tglSign, "1 dari 1");

            var docNumber = issuance?.NomorNotaDinas ?? $"No. {company.NomorSeq:D4}/NOTADINAS/NOL/{DateTime.UtcNow.Year}";
            var docTitle = issuance?.Outcome == NolOutcome.Rl
                ? "SURAT PENERBITAN RESPONSE LETTER (RL)"
                : "SURAT PENERBITAN NOTICE OF LETTER (NOL)";

            DocxBuilderHelper.AppendTitle(body, docTitle, docNumber, areaName, "Lampiran 16: Penerbitan NOL/RL");

            // Document Header Block
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Tanggal Penerbitan", tglSign),
                ("Keputusan / Outcome", issuance?.Outcome.ToString().ToUpperInvariant() ?? "NOL")
            ]);

            // Section 1: Data Perusahaan Calon Pelanggan
            DocxBuilderHelper.AppendSectionHeader(body, "1. Data Perusahaan Calon Pelanggan");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Nama Perusahaan", company.NamaPerusahaan),
                ("Nomor Register", company.Nomor),
                ("Alamat Lokasi / Kantor", company.Alamat),
                ("Sales Area", areaName)
            ]);

            // Section 2: Ringkasan Keputusan & Masa Berlaku
            DocxBuilderHelper.AppendSectionHeader(body, "2. Ringkasan Keputusan & Masa Berlaku");
            DocxBuilderHelper.AppendKeyValueTable(body,
            [
                ("Nomor Nota Dinas", docNumber),
                ("Masa Berlaku Sejak", issuance?.BerlakuSejak?.ToString("dd MMMM yyyy") ?? "-"),
                ("Masa Berlaku Sampai", issuance?.BerlakuSampai?.ToString("dd MMMM yyyy") ?? "-"),
                ("Jenis Registrasi", nol?.RegistrationType.ToString() ?? "Registrasi Baru")
            ]);

            // Section 3: Ketentuan Pasokan Gas Disetujui (Approved Terms)
            DocxBuilderHelper.AppendSectionHeader(body, "3. Ketentuan Pasokan Gas Disetujui (Approved Terms)");
            var termHeaders = new[] { "No.", "Periode Mulai", "Periode Selesai", "Rata-Rata (BBTUD)", "Min. Kontrak (BBTUD)", "Maks. Kontrak (BBTUD)" };
            var termRows = terms.Select((t, idx) => new[]
            {
                (idx + 1).ToString(),
                t.PeriodeMulai.ToString("dd MMM yyyy"),
                t.PeriodeSelesai.ToString("dd MMM yyyy"),
                $"{t.RataRata:N2}",
                $"{t.KontrakMinimum:N2}",
                $"{t.KontrakMaksimum:N2}"
            }).ToList();
            if (termRows.Count == 0)
            {
                termRows.Add(new[] { "1", DateTime.UtcNow.ToString("dd MMM yyyy"), DateTime.UtcNow.AddYears(1).ToString("dd MMM yyyy"), "1.00", "0.90", "1.10" });
            }
            body.AppendChild(DocxBuilderHelper.CreateTable(termHeaders, termRows));

            // Section 4: Syarat & Catatan Khusus (Kontrak Bersyarat)
            DocxBuilderHelper.AppendSectionHeader(body, "4. Syarat & Catatan Khusus (Kontrak Bersyarat)");
            var conditions = issuance?.KontrakBersyarat ?? [];
            if (conditions.Count == 0)
            {
                DocxBuilderHelper.AppendKeyValueParagraph(body, "Syarat Kontrak", "Standar sesuai ketentuan berlakunya Perjanjian Jual Beli Gas (PJBAG).");
            }
            else
            {
                foreach (var cond in conditions)
                {
                    DocxBuilderHelper.AppendKeyValueParagraph(body, "• Syarat", cond);
                }
            }

            // Section 5: Signature Block
            DocxBuilderHelper.AppendSignatureBlock(
                body,
                "DIBUAT DAN DIAJUKAN OLEH",
                $"Division Head / Area Head {areaName}",
                "DISETUJUI & DISAHKAN OLEH",
                signerName
            );

            doc.Save();
        }

        return ms.ToArray();
    }
}
