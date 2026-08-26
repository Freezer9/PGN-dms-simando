using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.Nol;
using Simando.Application.Registration;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.Nol;
using Simando.Domain.Registration;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Directory;

// Fresh-context-per-call, same shape as every other service. GetListAsync
// leans entirely on Company's own row-level-security query filter
// (SimandoDbContext.OnModelCreating) for scope + soft-delete — nothing here
// re-implements that check.
internal sealed class CompanyService(IDbContextFactory<SimandoDbContext> dbContextFactory) : ICompanyService
{
    public async Task<PagedResult<CompanyListItem>> GetPagedListAsync(CompanyListFilter filter, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        IQueryable<Company> query = db.Companies.AsNoTracking();

        if (filter.Stage is { } stage)
            query = query.Where(c => c.CurrentStage == stage);

        if (filter.IndustryTypeId is { } industryTypeId)
            query = query.Where(c => c.IndustryTypeId == industryTypeId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(c => EF.Functions.ILike(c.NamaPerusahaan, $"%{term}%") || EF.Functions.ILike(c.Nomor, $"%{term}%"));
        }

        if (filter.VillageId is { } villageId)
        {
            query = query.Where(c => c.VillageId == villageId);
        }
        else if (filter.DistrictId is { } districtId)
        {
            var vIds = db.Villages.Where(v => v.DistrictId == districtId).Select(v => v.Id);
            query = query.Where(c => vIds.Contains(c.VillageId));
        }
        else if (filter.RegencyId is { } regencyId)
        {
            var dIds = db.Districts.Where(d => d.RegencyId == regencyId).Select(d => d.Id);
            var vIds = db.Villages.Where(v => dIds.Contains(v.DistrictId)).Select(v => v.Id);
            query = query.Where(c => vIds.Contains(c.VillageId));
        }
        else if (filter.ProvinceId is { } provinceId)
        {
            var rIds = db.Regencies.Where(r => r.ProvinceId == provinceId).Select(r => r.Id);
            var dIds = db.Districts.Where(d => rIds.Contains(d.RegencyId)).Select(d => d.Id);
            var vIds = db.Villages.Where(v => dIds.Contains(v.DistrictId)).Select(v => v.Id);
            query = query.Where(c => vIds.Contains(c.VillageId));
        }

        if (filter.PosisiPelanggan is { } posisiPelanggan)
        {
            var pCompIds = db.Plottings.Where(p => p.PosisiPelanggan == posisiPelanggan).Select(p => p.CompanyId);
            query = query.Where(c => pCompIds.Contains(c.Id));
        }

        if (filter.Kawasan is { } kawasan)
        {
            var pCompIds = db.Plottings.Where(p => p.Kawasan == kawasan).Select(p => p.CompanyId);
            query = query.Where(c => pCompIds.Contains(c.Id));
        }

        var totalCount = await query.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var companies = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var villageIds = companies.Select(c => c.VillageId).ToHashSet();
        var villages = await db.Villages.AsNoTracking().Where(v => villageIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, ct);

        var districtIds = villages.Values.Select(v => v.DistrictId).ToHashSet();
        var districts = await db.Districts.AsNoTracking().Where(d => districtIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, ct);

        var regencyIds = districts.Values.Select(d => d.RegencyId).ToHashSet();
        var regencies = await db.Regencies.AsNoTracking().Where(r => regencyIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);

        var industryTypeIds = companies.Select(c => c.IndustryTypeId).ToHashSet();
        var industryTypeNames = await db.IndustryTypes.AsNoTracking()
            .Where(t => industryTypeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, ct);

        var companyIds = companies.Select(c => c.Id).ToHashSet();
        var plottings = await db.Plottings.AsNoTracking()
            .Where(p => companyIds.Contains(p.CompanyId))
            .ToDictionaryAsync(p => p.CompanyId, ct);

        var salesUserIds = plottings.Values.Select(p => p.SalesUserId).ToHashSet();
        var salesUserNames = await db.Users.AsNoTracking()
            .Where(u => salesUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);

        var result = new List<CompanyListItem>();
        foreach (var company in companies)
        {
            var village = villages[company.VillageId];
            var district = districts[village.DistrictId];
            var regency = regencies[district.RegencyId];

            var plotting = plottings.GetValueOrDefault(company.Id);
            var locationLabel = $"{(regency.Type == RegencyType.Kota ? "Kota" : "Kabupaten")} {regency.Name}";
            result.Add(new CompanyListItem(
                company.Id, company.Nomor, company.NamaPerusahaan,
                industryTypeNames.GetValueOrDefault(company.IndustryTypeId, ""),
                locationLabel, company.CurrentStage, company.Status,
                plotting?.SalesUserId,
                plotting is null ? null : salesUserNames.GetValueOrDefault(plotting.SalesUserId),
                plotting?.PosisiPelanggan,
                plotting?.Kawasan,
                company.Location?.Y,
                company.Location?.X));
        }

        return new PagedResult<CompanyListItem>(result, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<CompanyListItem>> GetListAsync(CompanyListFilter filter, CancellationToken ct = default)
    {
        var paged = await GetPagedListAsync(filter with { Page = 1, PageSize = 1000 }, ct);
        return paged.Items;
    }

    public async Task<CreateCompanyResult> CreateAsync(
        CreateCompanyRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var village = await db.Villages.AsNoTracking().FirstAsync(v => v.Id == request.VillageId, ct);
        var district = await db.Districts.AsNoTracking().FirstAsync(d => d.Id == village.DistrictId, ct);
        var regency = await db.Regencies.AsNoTracking().FirstAsync(r => r.Id == district.RegencyId, ct);
        var province = await db.Provinces.AsNoTracking().FirstAsync(p => p.Id == regency.ProvinceId, ct);

        var company = new Company
        {
            Id = Guid.NewGuid(),
            // Placeholder — Nomor is only knowable once NomorSeq is
            // allocated by the DB default on insert (docs/domain/
            // 03-directory-plotting.md#allocating-the-number).
            Nomor = "",
            NamaPerusahaan = request.NamaPerusahaan,
            Website = request.Website,
            VillageId = request.VillageId,
            Alamat = request.Alamat,
            Location = new Point(request.Longitude, request.Latitude) { SRID = 4326 },
            IndustryTypeId = request.IndustryTypeId,
            Npwp = request.Npwp,
            Email = request.Email,
            KodePos = request.KodePos,
            Telp = request.Telp,
            AreaId = request.AreaId,
            CurrentStage = 1,
            Status = RecordStatus.Draft,
            CreatedBy = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        // NomorSeq is only known after the insert (DB-generated via
        // nextval()) — render and persist Nomor in a second save.
        company.Nomor = $"{company.NomorSeq:0000000}-{province.BpsCode}-{regency.BpsCode}";
        await db.SaveChangesAsync(ct);

        return new CreateCompanyResult(company.Id, company.Nomor);
    }

    public async Task<SoftDeleteResult> SoftDeleteAsync(Guid companyId, Guid actorUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return SoftDeleteResult.Rejected("Berkas tidak ditemukan.");
        }

        if (company.Status != RecordStatus.Draft)
        {
            return SoftDeleteResult.Rejected("Berkas yang pernah diajukan tidak dapat dihapus.");
        }

        company.DeletedAt = DateTimeOffset.UtcNow;
        company.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return SoftDeleteResult.Success();
    }

    public async Task<PlottingDetail?> GetPlottingAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var plotting = await db.Plottings.AsNoTracking().FirstOrDefaultAsync(p => p.CompanyId == companyId, ct);
        if (plotting is null)
        {
            return new PlottingDetail(companyId, null, null, null, null);
        }

        var salesUserName = await db.Users.AsNoTracking()
            .Where(u => u.Id == plotting.SalesUserId).Select(u => u.FullName).FirstOrDefaultAsync(ct) ?? "Sales Representative";

        return new PlottingDetail(companyId, plotting.SalesUserId, salesUserName, plotting.PosisiPelanggan, plotting.Kawasan);
    }

    public async Task<StageEditResult> SavePlottingAsync(
        Guid companyId, SavePlottingRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditStages1To3, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        var plotting = await db.Plottings.FirstOrDefaultAsync(p => p.CompanyId == companyId, ct);
        if (plotting is null)
        {
            plotting = new Plotting
            {
                CompanyId = companyId,
                SalesUserId = request.SalesUserId,
                PosisiPelanggan = request.PosisiPelanggan,
                Kawasan = request.Kawasan,
            };
            db.Plottings.Add(plotting);
        }
        else
        {
            plotting.SalesUserId = request.SalesUserId;
            plotting.PosisiPelanggan = request.PosisiPelanggan;
            plotting.Kawasan = request.Kawasan;
        }

        if (company.CurrentStage < 2)
        {
            company.CurrentStage = 2;
        }
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<StageEditResult> PromoteToProspekAsync(
        Guid companyId, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditStages1To3, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        if (company.CurrentStage > 2)
        {
            return StageEditResult.Rejected("Berkas sudah melewati tahap Plotting.");
        }

        var plotting = await db.Plottings.FirstOrDefaultAsync(p => p.CompanyId == companyId, ct);
        if (plotting is null)
        {
            return StageEditResult.Rejected("Lengkapi data plotting (Sales Representative, Posisi Pelanggan, dan Kawasan) terlebih dahulu.");
        }

        if (company.CurrentStage < 2)
        {
            company.CurrentStage = 2;
        }
        company.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return StageEditResult.Success();
    }

    public async Task<IReadOnlyList<ContactDetail>> GetContactsAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        return await db.CompanyContacts.AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .OrderByDescending(c => c.IsPrimary).ThenBy(c => c.SortOrder)
            .Select(c => new ContactDetail(
                c.Id, c.Nama, c.Jabatan, c.Email, c.NoHp, c.LinkedIn, c.Instagram, c.Facebook, c.IsPrimary, c.SortOrder))
            .ToListAsync(ct);
    }

    public async Task<StageEditResult> AddContactAsync(
        Guid companyId, SaveContactRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditStages1To3, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        var existing = await db.CompanyContacts.Where(c => c.CompanyId == companyId).ToListAsync(ct);

        if (request.IsPrimary)
        {
            foreach (var other in existing)
            {
                other.IsPrimary = false;
            }
        }

        db.CompanyContacts.Add(new CompanyContact
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Nama = request.Nama,
            Jabatan = request.Jabatan,
            Email = request.Email,
            NoHp = request.NoHp,
            LinkedIn = request.LinkedIn,
            Instagram = request.Instagram,
            Facebook = request.Facebook,
            // First contact for a company is primary regardless of the
            // request — there is always exactly one primary once any exist.
            IsPrimary = request.IsPrimary || existing.Count == 0,
            SortOrder = (short)existing.Count,
        });

        if (company.CurrentStage < 3)
        {
            company.CurrentStage = 3;
            company.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<StageEditResult> UpdateContactAsync(
        Guid companyId, Guid contactId, SaveContactRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditStages1To3, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        var contact = await db.CompanyContacts.FirstOrDefaultAsync(c => c.Id == contactId && c.CompanyId == companyId, ct);
        if (contact is null)
        {
            return StageEditResult.Rejected("Kontak tidak ditemukan.");
        }

        if (request.IsPrimary)
        {
            var others = await db.CompanyContacts
                .Where(c => c.CompanyId == companyId && c.Id != contactId)
                .ToListAsync(ct);
            foreach (var other in others)
            {
                other.IsPrimary = false;
            }
        }

        contact.Nama = request.Nama;
        contact.Jabatan = request.Jabatan;
        contact.Email = request.Email;
        contact.NoHp = request.NoHp;
        contact.LinkedIn = request.LinkedIn;
        contact.Instagram = request.Instagram;
        contact.Facebook = request.Facebook;
        contact.IsPrimary = request.IsPrimary;

        if (company.CurrentStage < 3)
        {
            company.CurrentStage = 3;
            company.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<StageEditResult> DeleteContactAsync(
        Guid companyId, Guid contactId, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditStages1To3, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        var contactCount = await db.CompanyContacts.CountAsync(c => c.CompanyId == companyId, ct);
        var contact = await db.CompanyContacts.FirstOrDefaultAsync(c => c.Id == contactId && c.CompanyId == companyId, ct);
        if (contact is null)
        {
            return StageEditResult.Rejected("Kontak tidak ditemukan.");
        }

        if (contactCount <= 1)
        {
            return StageEditResult.Rejected("Kontak terakhir tidak dapat dihapus.");
        }

        db.CompanyContacts.Remove(contact);

        if (contact.IsPrimary)
        {
            var next = await db.CompanyContacts
                .Where(c => c.CompanyId == companyId && c.Id != contactId)
                .OrderBy(c => c.SortOrder)
                .FirstAsync(ct);
            next.IsPrimary = true;
        }

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<StageEditResult> UpdateLocationAsync(
        Guid companyId, double latitude, double longitude, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.DropMovePin, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        company.Location = new Point(longitude, latitude) { SRID = 4326 };
        company.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return StageEditResult.Success();
    }

    public async Task<SurveyDetail> GetSurveyAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var survey = await db.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.CompanyId == companyId, ct);

        string? surveyorUserName = null;
        if (survey?.SurveyorUserId is { } surveyorUserId)
        {
            surveyorUserName = await db.Users.AsNoTracking()
                .Where(u => u.Id == surveyorUserId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        }

        var products = await db.SurveyProducts.AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new SurveyProductDetail(p.Id, p.Produk, p.Kapasitas, p.HargaProduk, p.Catatan, p.SortOrder))
            .ToListAsync(ct);

        var rawMaterials = await db.SurveyRawMaterials.AsNoTracking()
            .Where(m => m.CompanyId == companyId)
            .OrderBy(m => m.SortOrder)
            .Select(m => new SurveyRawMaterialDetail(m.Id, m.Bahan, m.Asal, m.CountryId, m.Volume, m.SatuanUnitId, m.SortOrder))
            .ToListAsync(ct);

        var markets = await db.SurveyMarkets.AsNoTracking()
            .Where(m => m.CompanyId == companyId)
            .OrderBy(m => m.SortOrder)
            .Select(m => new SurveyMarketDetail(m.Id, m.Bahan, m.Asal, m.CountryId, m.Volume, m.SatuanUnitId, m.SortOrder))
            .ToListAsync(ct);

        var equipment = await db.SurveyEquipment.AsNoTracking()
            .Where(e => e.CompanyId == companyId)
            .OrderBy(e => e.SortOrder)
            .Select(e => new SurveyEquipmentDetail(
                e.Id, e.JenisPeralatan, e.Kapasitas, e.KapasitasUnitId, e.JamPerHari, e.HariPerMinggu,
                e.FuelTypeId, e.HargaBahanBakar, e.KonsumsiPerBulan, e.KonsumsiUnitId, e.KonversiKeGas, e.SortOrder))
            .ToListAsync(ct);

        return new SurveyDetail(
            companyId,
            survey?.TanggalSurvey, survey?.SurveyorUserId, surveyorUserName,
            survey?.JumlahKaryawan, survey?.JumlahShift, survey?.JamKerjaPerHari, survey?.HariPerMinggu,
            survey?.BebanPuncak1Mulai, survey?.BebanPuncak1Selesai, survey?.BebanPuncak2Mulai, survey?.BebanPuncak2Selesai,
            survey?.KebutuhanEnergi, survey?.KebutuhanEnergiLainnya,
            survey?.KapasitasNilai, survey?.KapasitasUnitId, survey?.PemakaianNilai, survey?.PemakaianUnitId,
            survey?.PipaTerdekatJarakM, survey?.PipaTerdekatDiameter, survey?.PipaTerdekatTekanan,
            survey?.BahanBakarEksisting, survey?.NamaPemasok,
            survey?.KapasitasListrikKw, survey?.PemakaianListrikKwh,
            survey?.RencanaPemanfaatanGas, survey?.DeskripsiProsesProduksi,
            survey?.MinEfisiensiDiharapkanPct, survey?.WillingnessToPayUsdMmbtu,
            survey?.KeteranganLain, survey?.JumlahKebutuhanEnergi ?? 0m,
            products, rawMaterials, markets, equipment);
    }

    public async Task<StageEditResult> SaveSurveyFullAsync(
        Guid companyId,
        SaveSurveyRequest request,
        IReadOnlyList<SaveSurveyProductRequest> products,
        IReadOnlyList<SaveSurveyRawMaterialRequest> rawMaterials,
        IReadOnlyList<SaveSurveyMarketRequest> markets,
        IReadOnlyList<SaveSurveyEquipmentRequest> equipment,
        Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null)
        {
            return StageEditResult.Rejected("Berkas tidak ditemukan.");
        }

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditSurvey, ct);
        if (editCheck is { } rejection)
        {
            return rejection;
        }

        var survey = await GetOrCreateSurveyAsync(db, companyId, ct);
        survey.TanggalSurvey = request.TanggalSurvey;
        survey.SurveyorUserId = request.SurveyorUserId;
        survey.JumlahKaryawan = request.JumlahKaryawan;
        survey.JumlahShift = request.JumlahShift;
        survey.JamKerjaPerHari = request.JamKerjaPerHari;
        survey.HariPerMinggu = request.HariPerMinggu;
        survey.BebanPuncak1Mulai = request.BebanPuncak1Mulai;
        survey.BebanPuncak1Selesai = request.BebanPuncak1Selesai;
        survey.BebanPuncak2Mulai = request.BebanPuncak2Mulai;
        survey.BebanPuncak2Selesai = request.BebanPuncak2Selesai;
        survey.KebutuhanEnergi = request.KebutuhanEnergi;
        survey.KebutuhanEnergiLainnya = request.KebutuhanEnergiLainnya;
        survey.KapasitasNilai = request.KapasitasNilai;
        survey.KapasitasUnitId = request.KapasitasUnitId;
        survey.PemakaianNilai = request.PemakaianNilai;
        survey.PemakaianUnitId = request.PemakaianUnitId;
        survey.PipaTerdekatJarakM = request.PipaTerdekatJarakM;
        survey.PipaTerdekatDiameter = request.PipaTerdekatDiameter;
        survey.PipaTerdekatTekanan = request.PipaTerdekatTekanan;
        survey.BahanBakarEksisting = request.BahanBakarEksisting;
        survey.NamaPemasok = request.NamaPemasok;
        survey.KapasitasListrikKw = request.KapasitasListrikKw;
        survey.PemakaianListrikKwh = request.PemakaianListrikKwh;
        survey.RencanaPemanfaatanGas = request.RencanaPemanfaatanGas;
        survey.DeskripsiProsesProduksi = request.DeskripsiProsesProduksi;
        survey.MinEfisiensiDiharapkanPct = request.MinEfisiensiDiharapkanPct;
        survey.WillingnessToPayUsdMmbtu = request.WillingnessToPayUsdMmbtu;
        survey.KeteranganLain = request.KeteranganLain;

        var oldProducts = await db.SurveyProducts.Where(p => p.CompanyId == companyId).ToListAsync(ct);
        db.SurveyProducts.RemoveRange(oldProducts);
        for (short i = 0; i < products.Count; i++)
        {
            var p = products[i];
            db.SurveyProducts.Add(new SurveyProduct
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Produk = p.Produk,
                Kapasitas = p.Kapasitas,
                HargaProduk = p.HargaProduk,
                Catatan = p.Catatan,
                SortOrder = i,
            });
        }

        var oldRawMaterials = await db.SurveyRawMaterials.Where(r => r.CompanyId == companyId).ToListAsync(ct);
        db.SurveyRawMaterials.RemoveRange(oldRawMaterials);
        for (short i = 0; i < rawMaterials.Count; i++)
        {
            var r = rawMaterials[i];
            db.SurveyRawMaterials.Add(new SurveyRawMaterial
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Bahan = r.Bahan,
                Asal = r.Asal,
                CountryId = r.CountryId,
                Volume = r.Volume,
                SatuanUnitId = r.SatuanUnitId,
                SortOrder = i,
            });
        }

        var oldMarkets = await db.SurveyMarkets.Where(m => m.CompanyId == companyId).ToListAsync(ct);
        db.SurveyMarkets.RemoveRange(oldMarkets);
        for (short i = 0; i < markets.Count; i++)
        {
            var m = markets[i];
            db.SurveyMarkets.Add(new SurveyMarket
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Bahan = m.Bahan,
                Asal = m.Asal,
                CountryId = m.CountryId,
                Volume = m.Volume,
                SatuanUnitId = m.SatuanUnitId,
                SortOrder = i,
            });
        }

        var oldEquipment = await db.SurveyEquipment.Where(e => e.CompanyId == companyId).ToListAsync(ct);
        db.SurveyEquipment.RemoveRange(oldEquipment);
        for (short i = 0; i < equipment.Count; i++)
        {
            var e = equipment[i];
            db.SurveyEquipment.Add(new SurveyEquipment
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                JenisPeralatan = e.JenisPeralatan,
                Kapasitas = e.Kapasitas,
                KapasitasUnitId = e.KapasitasUnitId,
                JamPerHari = e.JamPerHari,
                HariPerMinggu = e.HariPerMinggu,
                FuelTypeId = e.FuelTypeId,
                HargaBahanBakar = e.HargaBahanBakar,
                KonsumsiPerBulan = e.KonsumsiPerBulan,
                KonsumsiUnitId = e.KonsumsiUnitId,
                KonversiKeGas = e.KonversiKeGas,
                SortOrder = i,
            });
        }

        await db.SaveChangesAsync(ct);
        await RecomputeJumlahKebutuhanEnergiAsync(db, companyId, ct);

        if (company.CurrentStage < 4)
        {
            company.CurrentStage = 4;
        }
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    // Loads the Survey header row, creating an empty one if this is the
    // first write to any part of Survey for this Company — child rows FK
    // straight to Company (not to Survey), so they can be transcribed
    // before the header's own fields are ever touched. Caller must
    // SaveChangesAsync.
    private static async Task<Survey> GetOrCreateSurveyAsync(SimandoDbContext db, Guid companyId, CancellationToken ct)
    {
        var survey = await db.Surveys.FirstOrDefaultAsync(s => s.CompanyId == companyId, ct);
        if (survey is null)
        {
            survey = new Survey { CompanyId = companyId };
            db.Surveys.Add(survey);
        }

        return survey;
    }

    // Stored (not computed on read) because it appears on signed documents.
    // docs/design/data-model.md#survey--stage-4-kk0-header. Caller must
    // SaveChangesAsync after the equipment row change is itself persisted,
    // so this query sees it.
    private static async Task RecomputeJumlahKebutuhanEnergiAsync(SimandoDbContext db, Guid companyId, CancellationToken ct)
    {
        var total = await db.SurveyEquipment
            .Where(e => e.CompanyId == companyId)
            .SumAsync(e => e.KonversiKeGas, ct);

        var survey = await GetOrCreateSurveyAsync(db, companyId, ct);
        survey.JumlahKebutuhanEnergi = total;
    }

    // Shared by every stage 1-4 write: scope + Draft + capability gate.
    // Returns null when the actor may edit, otherwise the rejection to return.
    private static async Task<StageEditResult?> CanEditAsync(
        SimandoDbContext db, Company company, EffectivePermissions actor, Capability required, CancellationToken ct)
    {
        if (company.Status != RecordStatus.Draft)
        {
            return StageEditResult.Rejected("Berkas ini sudah tidak dapat diubah.");
        }

        if (!actor.HasCapability(required))
        {
            return StageEditResult.Rejected("Anda tidak memiliki akses untuk mengubah berkas ini.");
        }

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        if (area is null || !PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, company.AreaId, area.RegionId))
        {
            return StageEditResult.Rejected("Anda tidak memiliki akses untuk mengubah berkas ini.");
        }

        return null;
    }

    private static async Task<StageEditResult?> CanEditEvaluationAsync(
        SimandoDbContext db, Company company, EffectivePermissions actor, CancellationToken ct)
    {
        if (company.Status != RecordStatus.RegionalAdmin)
        {
            return StageEditResult.Rejected("Evaluasi hanya dapat diubah saat berkas berada di tahap Admin Regional.");
        }

        if (!actor.HasCapability(Capability.EditEvaluation))
        {
            return StageEditResult.Rejected("Anda tidak memiliki akses untuk mengubah evaluasi berkas ini.");
        }

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        if (area is null || !PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, company.AreaId, area.RegionId))
        {
            return StageEditResult.Rejected("Anda tidak memiliki akses untuk mengubah berkas ini.");
        }

        return null;
    }

    private static async Task<StageEditResult?> CanEditIssuanceAsync(
        SimandoDbContext db, Company company, EffectivePermissions actor, CancellationToken ct)
    {
        if (company.Status != RecordStatus.Approval)
        {
            return StageEditResult.Rejected("Penerbitan NOL/RL hanya dapat diubah saat berkas berada di tahap Division Head.");
        }

        if (!actor.HasCapability(Capability.SetApprovedTerms) && !actor.HasCapability(Capability.IssueNolRl))
        {
            return StageEditResult.Rejected("Anda tidak memiliki akses untuk menentukan syarat atau menerbitkan NOL/RL.");
        }

        var area = await db.Areas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == company.AreaId, ct);
        if (area is null || !PermissionEvaluator.CanView(actor.Scope, actor.AreaId, actor.RegionId, company.AreaId, area.RegionId))
        {
            return StageEditResult.Rejected("Anda tidak memiliki akses untuk mengubah berkas ini.");
        }

        return null;
    }

    public async Task<A1RegistrationDetail?> GetA1RegistrationAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var a1 = await db.A1Registrations.AsNoTracking()
            .Include(a => a.UsagePeriods)
            .FirstOrDefaultAsync(a => a.CompanyId == companyId, ct);

        if (a1 is null) return null;

        var usagePeriods = a1.UsagePeriods.OrderBy(u => u.SortOrder)
            .Select(u => new A1UsagePeriodDetail(u.Id, u.PeriodeMulai, u.PeriodeSelesai, u.RataRata, u.Minimum, u.Maksimum, u.SortOrder))
            .ToList();

        return new A1RegistrationDetail(
            a1.CompanyId, a1.TanggalRegistrasi, a1.RegistrasiSource, a1.NamaPenanggungJawab, a1.Jabatan,
            a1.BulanDimulai, a1.BasisKontrak, a1.SkemaHarga, a1.SegmentId, a1.KodeHarga,
            a1.HargaNilai, a1.HargaCurrency, a1.HargaUnit, a1.CapexAwal, a1.MomSigasTersedia,
            a1.StatusBangunan, a1.Sektor, a1.ProduksiUtama, a1.JenisPeralatanGas, a1.TekananOperasiBarg,
            a1.SignedDocumentId, a1.SignatureMethod, usagePeriods);
    }

    public async Task<StageEditResult> SaveA1RegistrationAsync(
        Guid companyId, SaveA1RegistrationRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null) return StageEditResult.Rejected("Berkas tidak ditemukan.");

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditSurvey, ct);
        if (editCheck is { } rejection) return rejection;

        var existingKinds = await db.Attachments.AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .Select(a => a.Kind)
            .ToListAsync(ct);
        var gateCheck = StageGateEvaluator.EvaluateSurveyToA1Gate(existingKinds);
        if (!gateCheck.IsPassed) return StageEditResult.Rejected(gateCheck.MissingPrerequisites[0]);

        var a1 = await db.A1Registrations.Include(a => a.UsagePeriods).FirstOrDefaultAsync(a => a.CompanyId == companyId, ct);
        if (a1 is null)
        {
            a1 = new A1Registration { CompanyId = companyId };
            db.A1Registrations.Add(a1);
        }

        a1.TanggalRegistrasi = request.TanggalRegistrasi;
        a1.NamaPenanggungJawab = request.NamaPenanggungJawab;
        a1.Jabatan = request.Jabatan;
        a1.BulanDimulai = request.BulanDimulai;
        a1.BasisKontrak = request.BasisKontrak;
        a1.SkemaHarga = request.SkemaHarga;
        a1.SegmentId = request.SegmentId;
        a1.KodeHarga = request.KodeHarga;
        a1.HargaNilai = request.HargaNilai;
        a1.HargaCurrency = request.HargaCurrency;
        a1.HargaUnit = request.HargaUnit;
        a1.CapexAwal = request.CapexAwal;
        a1.MomSigasTersedia = request.MomSigasTersedia;
        a1.StatusBangunan = request.StatusBangunan;
        a1.Sektor = request.Sektor;
        a1.ProduksiUtama = request.ProduksiUtama;
        a1.JenisPeralatanGas = request.JenisPeralatanGas;
        a1.TekananOperasiBarg = request.TekananOperasiBarg;
        a1.SignedDocumentId = request.SignedDocumentId;
        a1.SignatureMethod = request.SignatureMethod;

        db.A1UsagePeriods.RemoveRange(a1.UsagePeriods);
        a1.UsagePeriods.Clear();

        short sortOrder = 0;
        foreach (var u in request.UsagePeriods)
        {
            a1.UsagePeriods.Add(new A1UsagePeriod
            {
                Id = Guid.NewGuid(),
                A1RegistrationCompanyId = companyId,
                PeriodeMulai = u.PeriodeMulai,
                PeriodeSelesai = u.PeriodeSelesai,
                RataRata = u.RataRata,
                Minimum = u.Minimum,
                Maksimum = u.Maksimum,
                SortOrder = sortOrder++,
            });
        }

        if (company.CurrentStage < 5) company.CurrentStage = 5;
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<NolRequestDetail?> GetNolRequestAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var nol = await db.NolRequests.AsNoTracking()
            .Include(n => n.Periods)
            .Include(n => n.DailyBasisRows)
            .Include(n => n.References)
            .FirstOrDefaultAsync(n => n.CompanyId == companyId, ct);

        if (nol is null) return null;

        var periods = nol.Periods.OrderBy(p => p.SortOrder)
            .Select(p => new NolRequestPeriodDetail(p.Id, p.PeriodeMulai, p.PeriodeSelesai, p.RataRata, p.KontrakMinimum, p.KontrakMaksimum, p.SortOrder))
            .ToList();

        var dailyRows = nol.DailyBasisRows
            .Select(d => new NolRequestDailyDetail(d.Id, d.Hari, d.Min, d.Max))
            .ToList();

        var refs = nol.References.Select(r => r.ReferenceDocumentId).ToList();

        return new NolRequestDetail(
            nol.CompanyId, nol.NomorNotaDinas, nol.RegistrationType, nol.SamaDenganA1,
            nol.BulanDimulai, nol.BasisKontrak, nol.SkemaHarga, nol.SegmentId, nol.KodeHarga,
            nol.HargaNilai, nol.HargaCurrency, nol.HargaUnit, nol.AlasanKontrakBersyarat,
            nol.NamaPimpinanPerusahaan, nol.JangkaWaktuKontrak, nol.CapexPreGr3,
            nol.BiayaPenyambunganReguler, nol.BiayaPenyambunganExtra, nol.BiayaPenyambunganJumlah,
            nol.WorkflowInstanceId, nol.SubmittedAt, periods, dailyRows, refs);
    }

    public async Task<StageEditResult> SaveNolRequestAsync(
        Guid companyId, SaveNolRequestRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null) return StageEditResult.Rejected("Berkas tidak ditemukan.");

        var editCheck = await CanEditAsync(db, company, actor, Capability.EditSurvey, ct);
        if (editCheck is { } rejection) return rejection;

        var existingKinds = await db.Attachments.AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .Select(a => a.Kind)
            .ToListAsync(ct);
        var gateCheck = StageGateEvaluator.EvaluateA1ToNolRequestGate(request.SkemaHarga, existingKinds);
        if (!gateCheck.IsPassed) return StageEditResult.Rejected(gateCheck.MissingPrerequisites[0]);

        var nol = await db.NolRequests
            .Include(n => n.Periods)
            .Include(n => n.DailyBasisRows)
            .Include(n => n.References)
            .FirstOrDefaultAsync(n => n.CompanyId == companyId, ct);

        if (nol is null)
        {
            nol = new NolRequest { CompanyId = companyId };
            db.NolRequests.Add(nol);
        }

        nol.NomorNotaDinas = request.NomorNotaDinas;
        nol.RegistrationType = request.RegistrationType;
        nol.SamaDenganA1 = request.SamaDenganA1;
        nol.BulanDimulai = request.BulanDimulai;
        nol.BasisKontrak = request.BasisKontrak;
        nol.SkemaHarga = request.SkemaHarga;
        nol.SegmentId = request.SegmentId;
        nol.KodeHarga = request.KodeHarga;
        nol.HargaNilai = request.HargaNilai;
        nol.HargaCurrency = request.HargaCurrency;
        nol.HargaUnit = request.HargaUnit;
        nol.AlasanKontrakBersyarat = request.AlasanKontrakBersyarat;
        nol.NamaPimpinanPerusahaan = request.NamaPimpinanPerusahaan;
        nol.JangkaWaktuKontrak = request.JangkaWaktuKontrak;
        nol.CapexPreGr3 = request.CapexPreGr3;
        nol.BiayaPenyambunganReguler = request.BiayaPenyambunganReguler;
        nol.BiayaPenyambunganExtra = request.BiayaPenyambunganExtra;

        db.NolRequestPeriods.RemoveRange(nol.Periods);
        nol.Periods.Clear();
        short sortOrder = 0;
        foreach (var p in request.Periods)
        {
            nol.Periods.Add(new NolRequestPeriod
            {
                Id = Guid.NewGuid(),
                NolRequestCompanyId = companyId,
                PeriodeMulai = p.PeriodeMulai,
                PeriodeSelesai = p.PeriodeSelesai,
                RataRata = p.RataRata,
                KontrakMinimum = p.KontrakMinimum,
                KontrakMaksimum = p.KontrakMaksimum,
                SortOrder = sortOrder++,
            });
        }

        db.NolRequestDailies.RemoveRange(nol.DailyBasisRows);
        nol.DailyBasisRows.Clear();
        foreach (var d in request.DailyBasisRows)
        {
            nol.DailyBasisRows.Add(new NolRequestDaily
            {
                Id = Guid.NewGuid(),
                NolRequestCompanyId = companyId,
                Hari = d.Hari,
                Min = d.Min,
                Max = d.Max,
            });
        }

        db.NolRequestReferences.RemoveRange(nol.References);
        nol.References.Clear();
        foreach (var refId in request.ReferenceDocumentIds)
        {
            nol.References.Add(new NolRequestReference
            {
                Id = Guid.NewGuid(),
                NolRequestCompanyId = companyId,
                ReferenceDocumentId = refId,
            });
        }

        if (company.CurrentStage < 6) company.CurrentStage = 6;
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<NolEvaluationDetail?> GetNolEvaluationAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var eval = await db.NolEvaluations.AsNoTracking()
            .Include(e => e.Scenarios)
            .FirstOrDefaultAsync(e => e.NolRequestId == companyId, ct);

        if (eval is null) return null;

        var scenarios = eval.Scenarios
            .Select(s => new NolEvaluationScenarioDetail(s.Id, s.Label, s.IrrPct, s.Npv, s.PaybackYears, s.HasilAnalisis))
            .ToList();

        return new NolEvaluationDetail(
            eval.NolRequestId, eval.FeedStatus, eval.FeedCompletedAt, eval.CapexFinal,
            eval.PipaIndukPanjangM, eval.PipaIndukDiameter, eval.PipaIndukDiameterUnit,
            eval.PipaServicePanjangM, eval.PipaServiceDiameter, eval.PipaServiceDiameterUnit,
            eval.SpesifikasiMrs, eval.GSize, eval.Tekanan, eval.MaksFlowrate,
            eval.MaksKapasitasMeterM3Jam, eval.DurasiPelaksanaanBulan, eval.StatusRkap,
            eval.SkemaPembayaran, eval.JaminanStatus, eval.JaminanJenis, eval.JaminanMasaBerlaku,
            eval.JaminanPenerbit, eval.KetersediaanPasokanBbtud, eval.AnalisisKomersial,
            eval.AnalisisKompetitor, eval.Kesimpulan, eval.RadiusKompetitorKm,
            eval.EvaluatedBy, eval.EvaluatedAt, scenarios);
    }

    public async Task<StageEditResult> SaveNolEvaluationAsync(
        Guid companyId, SaveNolEvaluationRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null) return StageEditResult.Rejected("Berkas tidak ditemukan.");

        var evalCheck = await CanEditEvaluationAsync(db, company, actor, ct);
        if (evalCheck is { } rejection) return rejection;

        var eval = await db.NolEvaluations.Include(e => e.Scenarios).FirstOrDefaultAsync(e => e.NolRequestId == companyId, ct);
        if (eval is null)
        {
            eval = new NolEvaluation { NolRequestId = companyId };
            db.NolEvaluations.Add(eval);
        }

        eval.FeedStatus = request.FeedStatus;
        eval.FeedCompletedAt = request.FeedCompletedAt;
        eval.CapexFinal = request.CapexFinal;
        eval.PipaIndukPanjangM = request.PipaIndukPanjangM;
        eval.PipaIndukDiameter = request.PipaIndukDiameter;
        eval.PipaIndukDiameterUnit = request.PipaIndukDiameterUnit;
        eval.PipaServicePanjangM = request.PipaServicePanjangM;
        eval.PipaServiceDiameter = request.PipaServiceDiameter;
        eval.PipaServiceDiameterUnit = request.PipaServiceDiameterUnit;
        eval.SpesifikasiMrs = request.SpesifikasiMrs;
        eval.GSize = request.GSize;
        eval.Tekanan = request.Tekanan;
        eval.MaksFlowrate = request.MaksFlowrate;
        eval.MaksKapasitasMeterM3Jam = request.MaksKapasitasMeterM3Jam;
        eval.DurasiPelaksanaanBulan = request.DurasiPelaksanaanBulan;
        eval.StatusRkap = request.StatusRkap;
        eval.SkemaPembayaran = request.SkemaPembayaran;
        eval.JaminanStatus = request.JaminanStatus;
        eval.JaminanJenis = request.JaminanJenis;
        eval.JaminanMasaBerlaku = request.JaminanMasaBerlaku;
        eval.JaminanPenerbit = request.JaminanPenerbit;
        eval.KetersediaanPasokanBbtud = request.KetersediaanPasokanBbtud;
        eval.AnalisisKomersial = request.AnalisisKomersial;
        eval.AnalisisKompetitor = request.AnalisisKompetitor;
        eval.Kesimpulan = request.Kesimpulan;
        eval.RadiusKompetitorKm = request.RadiusKompetitorKm;
        eval.EvaluatedBy = actorUserId;
        eval.EvaluatedAt = DateTimeOffset.UtcNow;

        db.NolEvaluationScenarios.RemoveRange(eval.Scenarios);
        eval.Scenarios.Clear();
        foreach (var s in request.Scenarios)
        {
            eval.Scenarios.Add(new NolEvaluationScenario
            {
                Id = Guid.NewGuid(),
                NolEvaluationNolRequestId = companyId,
                Label = s.Label,
                IrrPct = s.IrrPct,
                Npv = s.Npv,
                PaybackYears = s.PaybackYears,
                HasilAnalisis = s.HasilAnalisis,
            });
        }

        if (company.CurrentStage < 7) company.CurrentStage = 7;
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }

    public async Task<NolIssuanceDetail?> GetNolIssuanceAsync(Guid companyId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var issuance = await db.NolIssuances.AsNoTracking()
            .Include(i => i.ApprovedTerms)
            .FirstOrDefaultAsync(i => i.NolRequestId == companyId, ct);

        if (issuance is null) return null;

        var approvedTerms = issuance.ApprovedTerms.OrderBy(a => a.SortOrder)
            .Select(a => new NolIssuanceApprovedTermDetail(a.Id, a.PeriodeMulai, a.PeriodeSelesai, a.RataRata, a.KontrakMinimum, a.KontrakMaksimum, a.SortOrder))
            .ToList();

        return new NolIssuanceDetail(
            issuance.NolRequestId, issuance.Outcome, issuance.NomorNotaDinas,
            issuance.KontrakBersyarat, issuance.BerlakuSejak, issuance.BerlakuSampai,
            issuance.SignedByUserId, issuance.SignedAt, issuance.DocumentId, approvedTerms);
    }

    public async Task<StageEditResult> SaveNolIssuanceAsync(
        Guid companyId, SaveNolIssuanceRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);
        if (company is null) return StageEditResult.Rejected("Berkas tidak ditemukan.");

        var issuanceCheck = await CanEditIssuanceAsync(db, company, actor, ct);
        if (issuanceCheck is { } rejection) return rejection;

        var issuance = await db.NolIssuances.Include(i => i.ApprovedTerms).FirstOrDefaultAsync(i => i.NolRequestId == companyId, ct);
        if (issuance is null)
        {
            issuance = new NolIssuance { NolRequestId = companyId };
            db.NolIssuances.Add(issuance);
        }

        issuance.Outcome = request.Outcome;
        issuance.NomorNotaDinas = request.NomorNotaDinas;
        issuance.KontrakBersyarat = request.KontrakBersyarat.ToList();
        issuance.BerlakuSejak = request.BerlakuSejak;
        issuance.BerlakuSampai = request.BerlakuSampai;
        issuance.DocumentId = request.DocumentId;

        db.NolIssuanceApprovedTerms.RemoveRange(issuance.ApprovedTerms);
        issuance.ApprovedTerms.Clear();

        short sortOrder = 0;
        foreach (var a in request.ApprovedTerms)
        {
            issuance.ApprovedTerms.Add(new NolIssuanceApprovedTerm
            {
                Id = Guid.NewGuid(),
                NolIssuanceNolRequestId = companyId,
                PeriodeMulai = a.PeriodeMulai,
                PeriodeSelesai = a.PeriodeSelesai,
                RataRata = a.RataRata,
                KontrakMinimum = a.KontrakMinimum,
                KontrakMaksimum = a.KontrakMaksimum,
                SortOrder = sortOrder++,
            });
        }

        if (company.CurrentStage < 8) company.CurrentStage = 8;
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return StageEditResult.Success();
    }
}
