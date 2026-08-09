using Microsoft.EntityFrameworkCore;
using Pgn.Dms.Api.Data;
using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Services;

/// <summary>
/// Get/upsert for the stage satellite tables. Each Save* follows the same shape as
/// <see cref="WorkflowService.SaveResumeAsync"/>: fetch-or-create the aggregate, replace
/// child rows wholesale, stamp UpdatedAt, and append an ActivityLog row.
/// </summary>
public class StageService(ApplicationDbContext db)
{
    // ── Plotting ────────────────────────────────────────────────────────────

    public async Task<PlottingDto?> GetPlottingAsync(int subscriptionId)
    {
        var p = await db.Plottings.Include(x => x.SalesUser)
            .FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return p?.ToDto();
    }

    public async Task<PlottingDto> SavePlottingAsync(int subscriptionId, SavePlottingRequest req, string actorName)
    {
        var p = await db.Plottings.FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        if (p is null)
        {
            p = new Plotting { SubscriptionId = subscriptionId };
            db.Plottings.Add(p);
        }

        p.SalesUserId = req.SalesUserId;
        p.PosisiPelanggan = req.PosisiPelanggan;
        p.Kawasan = req.Kawasan;
        p.UpdatedAt = DateTime.UtcNow;

        Log(subscriptionId, actorName, "Menyimpan data plotting");
        await db.SaveChangesAsync();

        return (await db.Plottings.Include(x => x.SalesUser)
            .FirstAsync(x => x.SubscriptionId == subscriptionId)).ToDto();
    }

    // ── Contacts (Prospek) ──────────────────────────────────────────────────

    public async Task<List<CompanyContactDto>> GetContactsAsync(int subscriptionId)
        => await db.CompanyContacts
            .Where(c => c.SubscriptionId == subscriptionId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => c.ToDto())
            .ToListAsync();

    public async Task<CompanyContactDto> SaveContactAsync(int subscriptionId, SaveContactRequest req, string actorName)
    {
        var contact = req.Id is > 0
            ? await db.CompanyContacts.FirstOrDefaultAsync(c => c.Id == req.Id && c.SubscriptionId == subscriptionId)
            : null;

        var isNew = contact is null;
        if (contact is null)
        {
            var maxOrder = await db.CompanyContacts
                .Where(c => c.SubscriptionId == subscriptionId)
                .Select(c => (int?)c.SortOrder).MaxAsync() ?? -1;

            contact = new CompanyContact
            {
                SubscriptionId = subscriptionId,
                Nama = req.Nama,
                Jabatan = req.Jabatan,
                SortOrder = maxOrder + 1
            };
            db.CompanyContacts.Add(contact);
        }

        contact.Nama = req.Nama;
        contact.Jabatan = req.Jabatan;
        contact.Email = req.Email;
        contact.NoHp = req.NoHp;
        contact.LinkedIn = req.LinkedIn;
        contact.Instagram = req.Instagram;
        contact.Facebook = req.Facebook;
        contact.IsPrimary = req.IsPrimary;

        // Exactly one primary contact per company.
        if (req.IsPrimary)
        {
            var others = await db.CompanyContacts
                .Where(c => c.SubscriptionId == subscriptionId && c.Id != contact.Id)
                .ToListAsync();
            foreach (var other in others) other.IsPrimary = false;
        }

        Log(subscriptionId, actorName, isNew ? $"Menambah kontak PIC {req.Nama}" : $"Memperbarui kontak PIC {req.Nama}");
        await db.SaveChangesAsync();
        return contact.ToDto();
    }

    public async Task<bool> DeleteContactAsync(int subscriptionId, int contactId, string actorName)
    {
        var contact = await db.CompanyContacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.SubscriptionId == subscriptionId);
        if (contact is null) return false;

        db.CompanyContacts.Remove(contact);
        Log(subscriptionId, actorName, $"Menghapus kontak PIC {contact.Nama}");
        await db.SaveChangesAsync();
        return true;
    }

    // ── Survey ──────────────────────────────────────────────────────────────

    public async Task<SurveyDto?> GetSurveyAsync(int subscriptionId)
    {
        var s = await SurveyQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return s?.ToDto();
    }

    public async Task<SurveyDto> SaveSurveyAsync(int subscriptionId, SurveyDto req, string actorName)
    {
        var s = await SurveyQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        if (s is null)
        {
            s = new Survey { SubscriptionId = subscriptionId };
            db.Surveys.Add(s);
        }

        s.TanggalSurvey = req.TanggalSurvey;
        s.SurveyorUserId = req.SurveyorUserId;
        s.JumlahKaryawan = req.JumlahKaryawan;
        s.JumlahShift = req.JumlahShift;
        s.JamKerjaPerHari = req.JamKerjaPerHari;
        s.HariPerMinggu = req.HariPerMinggu;
        s.KebutuhanEnergi = req.KebutuhanEnergi;
        s.KapasitasNilai = req.KapasitasNilai;
        s.KapasitasUnit = req.KapasitasUnit;
        s.PemakaianNilai = req.PemakaianNilai;
        s.PemakaianUnit = req.PemakaianUnit;
        s.JumlahKebutuhanEnergi = req.JumlahKebutuhanEnergi;
        s.PipaTerdekatJarakM = req.PipaTerdekatJarakM;
        s.PipaTerdekatDiameter = req.PipaTerdekatDiameter;
        s.PipaTerdekatTekanan = req.PipaTerdekatTekanan;
        s.BahanBakarEksisting = req.BahanBakarEksisting;
        s.NamaPemasok = req.NamaPemasok;
        s.KapasitasListrik = req.KapasitasListrik;
        s.PemakaianListrik = req.PemakaianListrik;
        s.RencanaPemanfaatanGas = req.RencanaPemanfaatanGas;
        s.DeskripsiProsesProduksi = req.DeskripsiProsesProduksi;
        s.KeteranganLain = req.KeteranganLain;
        s.BebanPuncak1Mulai = req.BebanPuncak1Mulai;
        s.BebanPuncak1Selesai = req.BebanPuncak1Selesai;
        s.BebanPuncak2Mulai = req.BebanPuncak2Mulai;
        s.BebanPuncak2Selesai = req.BebanPuncak2Selesai;
        s.MinEfisiensiDiharapkanPct = req.MinEfisiensiDiharapkanPct;
        s.WillingnessToPayUsdMmbtu = req.WillingnessToPayUsdMmbtu;
        s.UpdatedAt = DateTime.UtcNow;

        db.SurveyProducts.RemoveRange(s.Products);
        db.SurveyRawMaterials.RemoveRange(s.RawMaterials);
        db.SurveyMarkets.RemoveRange(s.Markets);
        db.SurveyEquipment.RemoveRange(s.Equipment);

        s.Products = [.. req.Products.Select((p, i) => new SurveyProduct
        {
            Nama = p.Nama, Kapasitas = p.Kapasitas, Unit = p.Unit, SortOrder = i
        })];
        s.RawMaterials = [.. req.RawMaterials.Select((r, i) => new SurveyRawMaterial
        {
            Nama = r.Nama, IsImpor = r.IsImpor, NegaraAsal = r.NegaraAsal, SortOrder = i
        })];
        s.Markets = [.. req.Markets.Select((m, i) => new SurveyMarket
        {
            Nama = m.Nama, IsEkspor = m.IsEkspor, PersentasePct = m.PersentasePct, SortOrder = i
        })];
        s.Equipment = [.. req.Equipment.Select((q, i) => new SurveyEquipment
        {
            Jenis = q.Jenis, Kapasitas = q.Kapasitas, JamPerHari = q.JamPerHari,
            HariPerMinggu = q.HariPerMinggu, BahanBakar = q.BahanBakar,
            HargaBahanBakar = q.HargaBahanBakar, KonsumsiPerBulan = q.KonsumsiPerBulan,
            KonversiKeGas = q.KonversiKeGas, SortOrder = i
        })];

        Log(subscriptionId, actorName, "Menyimpan survei KK0");
        await db.SaveChangesAsync();

        return (await SurveyQuery().FirstAsync(x => x.SubscriptionId == subscriptionId)).ToDto();
    }

    private IQueryable<Survey> SurveyQuery() => db.Surveys
        .Include(x => x.SurveyorUser)
        .Include(x => x.Products)
        .Include(x => x.RawMaterials)
        .Include(x => x.Markets)
        .Include(x => x.Equipment);

    // ── A1 ──────────────────────────────────────────────────────────────────

    public async Task<A1RegistrationDto?> GetA1Async(int subscriptionId)
    {
        var a = await A1Query().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return a?.ToDto();
    }

    public async Task<A1RegistrationDto> SaveA1Async(int subscriptionId, A1RegistrationDto req, string actorName)
    {
        var a = await A1Query().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        if (a is null)
        {
            a = new A1Registration { SubscriptionId = subscriptionId };
            db.A1Registrations.Add(a);
        }

        a.TanggalRegistrasi = req.TanggalRegistrasi;
        a.NamaPenanggungJawab = req.NamaPenanggungJawab;
        a.JabatanPenanggungJawab = req.JabatanPenanggungJawab;
        a.BulanDimulai = req.BulanDimulai;
        a.BasisKontrak = req.BasisKontrak;
        a.SkemaHarga = req.SkemaHarga;
        a.SegmentId = req.SegmentId;
        a.KodeHarga = req.KodeHarga;
        a.HargaNilai = req.HargaNilai;
        a.HargaCurrency = req.HargaCurrency;
        a.HargaUnit = req.HargaUnit;
        a.CapexAwal = req.CapexAwal;
        a.MomSigasTersedia = req.MomSigasTersedia;
        a.StatusBangunan = req.StatusBangunan;
        a.Sektor = req.Sektor;
        a.ProduksiUtama = req.ProduksiUtama;
        a.JenisPeralatanGas = req.JenisPeralatanGas;
        a.TekananOperasiBarg = req.TekananOperasiBarg;
        a.SignatureMethod = req.SignatureMethod;
        a.UpdatedAt = DateTime.UtcNow;

        db.A1UsagePeriods.RemoveRange(a.Periods);
        a.Periods = [.. req.Periods.Select((p, i) => new A1UsagePeriod
        {
            PeriodeMulai = p.PeriodeMulai, PeriodeSelesai = p.PeriodeSelesai,
            RataRata = p.RataRata, Minimum = p.Minimum, Maksimum = p.Maksimum, SortOrder = i
        })];

        Log(subscriptionId, actorName, "Menyimpan registrasi A1");
        await db.SaveChangesAsync();

        return (await A1Query().FirstAsync(x => x.SubscriptionId == subscriptionId)).ToDto();
    }

    private IQueryable<A1Registration> A1Query() => db.A1Registrations
        .Include(x => x.Segment)
        .Include(x => x.Periods);

    // ── NOL request ─────────────────────────────────────────────────────────

    public async Task<NolRequestDto?> GetNolRequestAsync(int subscriptionId)
    {
        var n = await NolRequestQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return n?.ToDto();
    }

    public async Task<NolRequestDto> SaveNolRequestAsync(int subscriptionId, NolRequestDto req, string actorName)
    {
        var n = await NolRequestQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        if (n is null)
        {
            n = new NolRequest { SubscriptionId = subscriptionId };
            db.NolRequests.Add(n);
        }

        n.NomorNotaDinas = req.NomorNotaDinas;
        n.RegistrationType = req.RegistrationType;
        n.SamaDenganA1 = req.SamaDenganA1;
        n.BulanDimulai = req.BulanDimulai;
        n.BasisKontrak = req.BasisKontrak;
        n.SkemaHarga = req.SkemaHarga;
        n.SegmentId = req.SegmentId;
        n.KodeHarga = req.KodeHarga;
        n.HargaNilai = req.HargaNilai;
        n.HargaCurrency = req.HargaCurrency;
        n.HargaUnit = req.HargaUnit;
        n.AlasanKontrakBersyarat = req.AlasanKontrakBersyarat;
        n.NamaPimpinanPerusahaan = req.NamaPimpinanPerusahaan;
        n.JangkaWaktuKontrak = req.JangkaWaktuKontrak;
        n.Lampiran17 = req.Lampiran17;
        n.CapexPreGr3 = req.CapexPreGr3;
        n.BiayaPenyambunganReguler = req.BiayaPenyambunganReguler;
        n.BiayaPenyambunganExtra = req.BiayaPenyambunganExtra;
        n.UpdatedAt = DateTime.UtcNow;

        db.NolRequestPeriods.RemoveRange(n.Periods);
        db.NolRequestReferences.RemoveRange(n.References);

        n.Periods = [.. req.Periods.Select((p, i) => new NolRequestPeriod
        {
            PeriodeMulai = p.PeriodeMulai, PeriodeSelesai = p.PeriodeSelesai,
            RataRata = p.RataRata, Minimum = p.Minimum, Maksimum = p.Maksimum, SortOrder = i
        })];
        n.References = [.. req.References.Select((r, i) => new NolRequestReference
        {
            Judul = r.Judul, Nomor = r.Nomor, Tanggal = r.Tanggal, SortOrder = i
        })];

        Log(subscriptionId, actorName, "Menyimpan permohonan NOL");
        await db.SaveChangesAsync();

        return (await NolRequestQuery().FirstAsync(x => x.SubscriptionId == subscriptionId)).ToDto();
    }

    private IQueryable<NolRequest> NolRequestQuery() => db.NolRequests
        .Include(x => x.Segment)
        .Include(x => x.Periods)
        .Include(x => x.References);

    // ── Evaluation ──────────────────────────────────────────────────────────

    public async Task<NolEvaluationDto?> GetEvaluationAsync(int subscriptionId)
    {
        var e = await EvaluationQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return e?.ToDto();
    }

    public async Task<NolEvaluationDto> SaveEvaluationAsync(
        int subscriptionId, NolEvaluationDto req, string userId, string actorName)
    {
        var e = await EvaluationQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        if (e is null)
        {
            e = new NolEvaluation { SubscriptionId = subscriptionId };
            db.NolEvaluations.Add(e);
        }

        e.FeedStatus = req.FeedStatus;
        e.FeedCompletedAt = req.FeedCompletedAt;
        e.CapexFinal = req.CapexFinal;
        e.PipaIndukPanjang = req.PipaIndukPanjang;
        e.PipaIndukDiameter = req.PipaIndukDiameter;
        e.PipaIndukUnit = req.PipaIndukUnit;
        e.PipaServicePanjang = req.PipaServicePanjang;
        e.PipaServiceDiameter = req.PipaServiceDiameter;
        e.PipaServiceUnit = req.PipaServiceUnit;
        e.MrsSpecId = req.MrsSpecId;
        e.MeterSizeId = req.MeterSizeId;
        e.Tekanan = req.Tekanan;
        e.MaksFlowrate = req.MaksFlowrate;
        e.MaksKapasitasMeterM3Jam = req.MaksKapasitasMeterM3Jam;
        e.DurasiPelaksanaanBulan = req.DurasiPelaksanaanBulan;
        e.StatusRkap = req.StatusRkap;
        e.SkemaPembayaran = req.SkemaPembayaran;
        e.JaminanStatus = req.JaminanStatus;
        e.JaminanJenis = req.JaminanJenis;
        e.JaminanPenerbit = req.JaminanPenerbit;
        e.JaminanMasaBerlaku = req.JaminanMasaBerlaku;
        e.KetersediaanPasokanBbtud = req.KetersediaanPasokanBbtud;
        e.AnalisisKomersial = req.AnalisisKomersial;
        e.AnalisisKompetitor = req.AnalisisKompetitor;
        e.RadiusKompetitorKm = req.RadiusKompetitorKm;
        e.Kesimpulan = req.Kesimpulan;
        e.EvaluatedById = userId;
        e.EvaluatedAt = DateTime.UtcNow;
        e.UpdatedAt = DateTime.UtcNow;

        db.NolEvaluationScenarios.RemoveRange(e.Scenarios);
        e.Scenarios = [.. req.Scenarios.Select((s, i) => new NolEvaluationScenario
        {
            Label = s.Label, IrrPct = s.IrrPct, Npv = s.Npv,
            PaybackYears = s.PaybackYears, HasilAnalisis = s.HasilAnalisis, SortOrder = i
        })];

        Log(subscriptionId, actorName, "Menyimpan evaluasi kelayakan");
        await db.SaveChangesAsync();

        return (await EvaluationQuery().FirstAsync(x => x.SubscriptionId == subscriptionId)).ToDto();
    }

    private IQueryable<NolEvaluation> EvaluationQuery() => db.NolEvaluations
        .Include(x => x.MrsSpec)
        .Include(x => x.MeterSize)
        .Include(x => x.EvaluatedBy)
        .Include(x => x.Scenarios);

    // ── Issuance ────────────────────────────────────────────────────────────

    public async Task<NolIssuanceDto?> GetIssuanceAsync(int subscriptionId)
    {
        var i = await IssuanceQuery().FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        return i?.ToDto();
    }

    /// <summary>
    /// Terminal and irreversible: sets the subscription to Disetujui (NOL) or Ditolak (RL)
    /// and refuses to run twice. Returns null when the record was already issued.
    /// </summary>
    public async Task<NolIssuanceDto?> IssueAsync(
        int subscriptionId, IssueNolRequest req, string userId, string actorName)
    {
        var existing = await db.NolIssuances.FirstOrDefaultAsync(x => x.SubscriptionId == subscriptionId);
        if (existing?.SignedAt is not null) return null;

        var sub = await db.Subscriptions.FindAsync(subscriptionId);
        if (sub is null) return null;

        var issuance = existing ?? new NolIssuance { SubscriptionId = subscriptionId };
        if (existing is null) db.NolIssuances.Add(issuance);

        issuance.Outcome = req.Outcome;
        issuance.NomorNotaDinas = req.NomorNotaDinas;
        issuance.BerlakuSejak = req.BerlakuSejak;
        issuance.BerlakuSampai = req.BerlakuSampai;
        issuance.Catatan = req.Catatan;
        issuance.SignedById = userId;
        issuance.SignedAt = DateTime.UtcNow;

        issuance.ApprovedTerms = [.. req.ApprovedTerms.Select((t, i) => new NolIssuanceTerm
        {
            PeriodeMulai = t.PeriodeMulai, PeriodeSelesai = t.PeriodeSelesai,
            RataRata = t.RataRata, Minimum = t.Minimum, Maksimum = t.Maksimum, SortOrder = i
        })];
        issuance.Conditions = [.. req.Conditions
            .Where(c => !string.IsNullOrWhiteSpace(c.Isi))
            .Select((c, i) => new NolIssuanceCondition { Isi = c.Isi, SortOrder = i })];

        sub.Status = req.Outcome == IssuanceOutcome.Nol
            ? SubscriptionStatus.Disetujui
            : SubscriptionStatus.Ditolak;
        sub.UpdatedAt = DateTime.UtcNow;

        var label = req.Outcome == IssuanceOutcome.Nol ? "NOL" : "RL";
        Log(subscriptionId, actorName, $"Menerbitkan {label}", req.Catatan);
        await db.SaveChangesAsync();

        return (await IssuanceQuery().FirstAsync(x => x.SubscriptionId == subscriptionId)).ToDto();
    }

    private IQueryable<NolIssuance> IssuanceQuery() => db.NolIssuances
        .Include(x => x.SignedBy)
        .Include(x => x.ApprovedTerms)
        .Include(x => x.Conditions);

    // ── Master data ─────────────────────────────────────────────────────────

    public async Task<List<MasterDataEntryDto>> GetMasterDataAsync(MasterCategory? category)
    {
        var query = db.MasterData.AsQueryable();
        if (category.HasValue) query = query.Where(m => m.Category == category.Value);
        return await query
            .OrderBy(m => m.Category).ThenBy(m => m.SortOrder).ThenBy(m => m.Name)
            .Select(m => m.ToDto())
            .ToListAsync();
    }

    public async Task<MasterDataEntryDto> SaveMasterDataAsync(int? id, SaveMasterDataRequest req)
    {
        var entry = id is > 0 ? await db.MasterData.FindAsync(id.Value) : null;
        if (entry is null)
        {
            entry = new MasterDataEntry { Name = req.Name };
            db.MasterData.Add(entry);
        }

        entry.Category = req.Category;
        entry.Code = req.Code;
        entry.Name = req.Name;
        entry.Description = req.Description;
        entry.AttributesJson = req.AttributesJson;
        entry.SortOrder = req.SortOrder;
        entry.IsActive = req.IsActive;

        await db.SaveChangesAsync();
        return entry.ToDto();
    }

    public async Task<bool> DeleteMasterDataAsync(int id)
    {
        var entry = await db.MasterData.FindAsync(id);
        if (entry is null) return false;
        db.MasterData.Remove(entry);
        await db.SaveChangesAsync();
        return true;
    }

    // ── Break glass ─────────────────────────────────────────────────────────

    public const int BreakGlassMinutes = 60;

    public async Task<List<BreakGlassGrantDto>> GetBreakGlassGrantsAsync()
        => await db.BreakGlassGrants
            .Include(g => g.Subscription)
            .Include(g => g.GrantedToUser)
            .Include(g => g.GrantedBy)
            .OrderByDescending(g => g.GrantedAt)
            .Select(g => g.ToDto())
            .ToListAsync();

    public async Task<BreakGlassGrantDto?> GrantBreakGlassAsync(
        GrantBreakGlassRequest req, string grantedById, string actorName)
    {
        if (string.IsNullOrWhiteSpace(req.Reason)) return null;
        if (!await db.Subscriptions.AnyAsync(s => s.Id == req.SubscriptionId)) return null;

        var grant = new BreakGlassGrant
        {
            SubscriptionId = req.SubscriptionId,
            GrantedToUserId = req.GrantedToUserId,
            GrantedById = grantedById,
            Reason = req.Reason,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(BreakGlassMinutes)
        };
        db.BreakGlassGrants.Add(grant);

        Log(req.SubscriptionId, actorName, "Memberikan akses darurat (break-glass)", req.Reason);
        await db.SaveChangesAsync();

        return (await db.BreakGlassGrants
            .Include(g => g.Subscription).Include(g => g.GrantedToUser).Include(g => g.GrantedBy)
            .FirstAsync(g => g.Id == grant.Id)).ToDto();
    }

    public async Task<bool> RevokeBreakGlassAsync(int grantId, string actorName)
    {
        var grant = await db.BreakGlassGrants.FindAsync(grantId);
        if (grant is null || grant.RevokedAt is not null) return false;

        grant.RevokedAt = DateTime.UtcNow;
        Log(grant.SubscriptionId, actorName, "Mencabut akses darurat (break-glass)");
        await db.SaveChangesAsync();
        return true;
    }

    // ── Shared ──────────────────────────────────────────────────────────────

    private void Log(int subscriptionId, string actorName, string action, string details = "")
        => db.ActivityLogs.Add(new ActivityLog
        {
            SubscriptionId = subscriptionId,
            ActorName = actorName,
            Action = action,
            Details = details,
            At = DateTime.UtcNow
        });
}
