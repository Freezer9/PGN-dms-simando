using BlazorBlueprint.Components;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Simando.Application.Attachments;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.MasterData;
using Simando.Domain.MasterData;
using Simando.Domain.Survey;
using Simando.Web.Components.Pages.StageGates;

namespace Simando.Web.Tests.Pages.StageGates;

public class SurveyFormTests : TestContext
{
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly IAttachmentService _attachmentService = Substitute.For<IAttachmentService>();
    private readonly IUnitLookupService _unitLookupService = Substitute.For<IUnitLookupService>();
    private readonly IEntityService<Country> _countryService = Substitute.For<IEntityService<Country>>();
    private readonly IEntityService<FuelType> _fuelTypeService = Substitute.For<IEntityService<FuelType>>();

    public SurveyFormTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBlazorBlueprintComponents();
        Services.AddSingleton(_companyService);
        Services.AddSingleton(_attachmentService);
        Services.AddSingleton(_unitLookupService);
        Services.AddSingleton(_countryService);
        Services.AddSingleton(_fuelTypeService);

        _unitLookupService.GetUnitsAsync(Arg.Any<UnitSet>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UnitOption>>([]));
        _countryService.GetAllAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Country, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Country>()));
        _fuelTypeService.GetAllAsync(Arg.Any<System.Linq.Expressions.Expression<Func<FuelType, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<FuelType>()));
    }

    [Fact(DisplayName = "SurveyForm renders header and equipment table with derived energy total")]
    public void SurveyForm_RendersEquipmentTable_AndDerivedTotal()
    {
        var companyId = Guid.NewGuid();
        var surveyDetail = new SurveyDetail(
            CompanyId: companyId,
            TanggalSurvey: new DateOnly(2026, 8, 10),
            SurveyorUserId: Guid.NewGuid(),
            SurveyorUserName: "Surveyor Test",
            JumlahKaryawan: 50,
            JumlahShift: 2,
            JamKerjaPerHari: 8m,
            HariPerMinggu: 5,
            BebanPuncak1Mulai: new TimeOnly(8, 0),
            BebanPuncak1Selesai: new TimeOnly(12, 0),
            BebanPuncak2Mulai: new TimeOnly(13, 0),
            BebanPuncak2Selesai: new TimeOnly(17, 0),
            KebutuhanEnergi: KebutuhanEnergiJenis.Steam,
            KebutuhanEnergiLainnya: null,
            KapasitasNilai: 100m,
            KapasitasUnitId: null,
            PemakaianNilai: 80m,
            PemakaianUnitId: null,
            PipaTerdekatJarakM: 50m,
            PipaTerdekatDiameter: 4m,
            PipaTerdekatTekanan: 2m,
            BahanBakarEksisting: BahanBakarEksisting.Lpg,
            NamaPemasok: "Pertamina",
            KapasitasListrikKw: 200m,
            PemakaianListrikKwh: 1500m,
            RencanaPemanfaatanGas: RencanaPemanfaatanGas.BahanBakar,
            DeskripsiProsesProduksi: "Pabrik Tekstil",
            MinEfisiensiDiharapkanPct: 15m,
            WillingnessToPayUsdMmbtu: 9.5m,
            KeteranganLain: null,
            JumlahKebutuhanEnergi: 450m,
            Products: [],
            RawMaterials: [],
            Markets: [],
            Equipment:
            [
                new SurveyEquipmentDetail(Guid.NewGuid(), "Boiler #1", 250m, null, 8m, 5, null, 1000m, 300m, null, 250m, 1),
                new SurveyEquipmentDetail(Guid.NewGuid(), "Burner #2", 150m, null, 8m, 5, null, 500m, 200m, null, 200m, 2)
            ]
        );

        _companyService.GetSurveyAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(surveyDetail);

        var cut = RenderComponent<SurveyForm>(parameters => parameters.Add(p => p.CompanyId, companyId));

        var markup = cut.Markup;
        markup.ShouldContain("Operasional");
        markup.ShouldContain("Beban Puncak");
        markup.ShouldContain("Peralatan");
        markup.ShouldContain("450");
    }
}
