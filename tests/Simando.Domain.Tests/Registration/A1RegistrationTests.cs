using Shouldly;
using Simando.Domain.Registration;

namespace Simando.Domain.Tests.Registration;

public class A1RegistrationTests
{
    [Fact]
    public void A1Registration_DefaultsAndProperties_AreInitialized()
    {
        var companyId = Guid.NewGuid();
        var a1 = new A1Registration
        {
            CompanyId = companyId,
            TanggalRegistrasi = new DateOnly(2026, 8, 9),
            RegistrasiSource = RegistrasiSource.Manual,
            NamaPenanggungJawab = "Budi Santoso",
            Jabatan = "Direktur",
            SkemaHarga = SkemaHarga.Sigas,
            MomSigasTersedia = true,
            StatusBangunan = StatusBangunan.Eksisting,
            Sektor = Sektor.Industri,
        };

        a1.CompanyId.ShouldBe(companyId);
        a1.RegistrasiSource.ShouldBe(RegistrasiSource.Manual);
        a1.SkemaHarga.ShouldBe(SkemaHarga.Sigas);
        a1.MomSigasTersedia.ShouldBeTrue();
        a1.UsagePeriods.ShouldBeEmpty();
    }

    [Fact]
    public void A1UsagePeriod_Properties_SetCorrectly()
    {
        var period = new A1UsagePeriod
        {
            A1RegistrationCompanyId = Guid.NewGuid(),
            PeriodeMulai = new DateOnly(2026, 1, 1),
            PeriodeSelesai = new DateOnly(2026, 12, 31),
            RataRata = 1000m,
            Minimum = 800m,
            Maksimum = 1200m,
            SortOrder = 1,
        };

        period.RataRata.ShouldBe(1000m);
        period.Minimum.ShouldBe(800m);
        period.Maksimum.ShouldBe(1200m);
    }
}
