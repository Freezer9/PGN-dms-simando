using Shouldly;
using Simando.Domain.Attachments;
using Simando.Domain.Registration;
using Simando.Domain.Workflow;

namespace Simando.Domain.Tests.Workflow;

public class StageGateEvaluatorTests
{
    [Fact]
    public void SurveyToA1Gate_Fails_WhenKk0Missing()
    {
        var result = StageGateEvaluator.EvaluateSurveyToA1Gate([]);

        result.IsPassed.ShouldBeFalse();
        result.MissingPrerequisites.ShouldContain(x => x.Contains("KK0"));
    }

    [Fact]
    public void SurveyToA1Gate_Passes_WhenKk0Present()
    {
        var result = StageGateEvaluator.EvaluateSurveyToA1Gate([AttachmentKind.Kk0]);

        result.IsPassed.ShouldBeTrue();
        result.MissingPrerequisites.ShouldBeEmpty();
    }

    [Fact]
    public void A1ToNolRequestGate_Fails_WhenAttachmentsMissing()
    {
        var result = StageGateEvaluator.EvaluateA1ToNolRequestGate(SkemaHarga.Reguler, []);

        result.IsPassed.ShouldBeFalse();
        result.MissingPrerequisites.Count.ShouldBe(2); // A1 & BuktiKelayakan
    }

    [Fact]
    public void A1ToNolRequestGate_RequiresMomSigas_WhenSkemaHargaIsSigas()
    {
        var result = StageGateEvaluator.EvaluateA1ToNolRequestGate(
            SkemaHarga.Sigas,
            [AttachmentKind.A1, AttachmentKind.BuktiKelayakan]);

        result.IsPassed.ShouldBeFalse();
        result.MissingPrerequisites.ShouldContain(x => x.Contains("MOM SiGas"));
    }

    [Fact]
    public void A1ToNolRequestGate_Passes_WhenAllRequiredAttachmentsPresent()
    {
        var result = StageGateEvaluator.EvaluateA1ToNolRequestGate(
            SkemaHarga.Sigas,
            [AttachmentKind.A1, AttachmentKind.BuktiKelayakan, AttachmentKind.MomSigas]);

        result.IsPassed.ShouldBeTrue();
        result.MissingPrerequisites.ShouldBeEmpty();
    }
}
