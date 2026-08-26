using Shouldly;
using Simando.Domain.Nol;
using Simando.Domain.Registration;

namespace Simando.Domain.Tests.Nol;

public class NolRequestTests
{
    [Fact]
    public void NolRequest_DerivedBiayaPenyambunganJumlah_ComputesCorrectly()
    {
        var nol = new NolRequest
        {
            CompanyId = Guid.NewGuid(),
            RegistrationType = RegistrationType.RegistrasiBaru,
            BiayaPenyambunganReguler = 50_000_000m,
            BiayaPenyambunganExtra = 15_000_000m,
        };

        nol.BiayaPenyambunganJumlah.ShouldBe(65_000_000m);
    }

    [Fact]
    public void NolEvaluation_And_NolIssuance_InitializeProperties()
    {
        var nolRequestId = Guid.NewGuid();

        var eval = new NolEvaluation
        {
            NolRequestId = nolRequestId,
            FeedStatus = FeedStatus.DalamProses,
            StatusRkap = StatusRkap.Rkap,
            SkemaPembayaran = SkemaPembayaran.JaminanPembayaran,
            KetersediaanPasokanBbtud = 5.5m,
        };

        eval.NolRequestId.ShouldBe(nolRequestId);
        eval.FeedStatus.ShouldBe(FeedStatus.DalamProses);
        eval.StatusRkap.ShouldBe(StatusRkap.Rkap);

        var issuance = new NolIssuance
        {
            NolRequestId = nolRequestId,
            Outcome = NolOutcome.Nol,
            KontrakBersyarat = ["Syarat A", "Syarat B"],
        };

        issuance.Outcome.ShouldBe(NolOutcome.Nol);
        issuance.KontrakBersyarat.Count.ShouldBe(2);
    }
}
