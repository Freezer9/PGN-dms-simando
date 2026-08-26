using Simando.Domain.Attachments;

namespace Simando.Domain.Workflow;

public sealed record StageGateCheckResult(bool IsPassed, IReadOnlyList<string> MissingPrerequisites)
{
    public static StageGateCheckResult Success() => new(true, Array.Empty<string>());
    public static StageGateCheckResult Failed(params string[] missing) => new(false, missing);
    public static StageGateCheckResult Failed(IEnumerable<string> missing) => new(false, missing.ToList());
}

public static class StageGateEvaluator
{
    public static StageGateCheckResult EvaluateSurveyToA1Gate(IEnumerable<AttachmentKind> existingAttachmentKinds)
    {
        var kinds = existingAttachmentKinds.ToHashSet();
        var missing = new List<string>();

        if (!kinds.Contains(AttachmentKind.Kk0))
        {
            missing.Add("Dokumen KK0 (Lampiran 10) yang sudah ditandatangani belum diunggah.");
        }

        return missing.Count == 0
            ? StageGateCheckResult.Success()
            : StageGateCheckResult.Failed(missing);
    }

    public static StageGateCheckResult EvaluateA1ToNolRequestGate(
        Registration.SkemaHarga? skemaHarga,
        IEnumerable<AttachmentKind> existingAttachmentKinds)
    {
        var kinds = existingAttachmentKinds.ToHashSet();
        var missing = new List<string>();

        if (!kinds.Contains(AttachmentKind.A1))
        {
            missing.Add("Dokumen A1 Registrasi (Lampiran 11) yang telah ditandatangani belum diunggah.");
        }

        if (!kinds.Contains(AttachmentKind.BuktiKelayakan))
        {
            missing.Add("Dokumen Bukti Kelayakan belum diunggah.");
        }

        if (skemaHarga == Registration.SkemaHarga.Sigas && !kinds.Contains(AttachmentKind.MomSigas))
        {
            missing.Add("MOM SiGas wajib diunggah untuk skema harga SiGas.");
        }

        return missing.Count == 0
            ? StageGateCheckResult.Success()
            : StageGateCheckResult.Failed(missing);
    }

    public static StageGateCheckResult EvaluateNolRequestToSubmitGate(IEnumerable<AttachmentKind> existingAttachmentKinds)
    {
        var kinds = existingAttachmentKinds.ToHashSet();
        var missing = new List<string>();

        if (!kinds.Contains(AttachmentKind.A1))
        {
            missing.Add("Dokumen A1 Registrasi (Lampiran 11) yang telah ditandatangani belum diunggah.");
        }

        if (!kinds.Contains(AttachmentKind.Kk0))
        {
            missing.Add("Dokumen KK0 (Lampiran 10) yang sudah ditandatangani belum diunggah.");
        }

        if (!kinds.Contains(AttachmentKind.CapexPreGr3))
        {
            missing.Add("Dokumen Biaya Capex Pre GR3 belum diunggah.");
        }

        return missing.Count == 0
            ? StageGateCheckResult.Success()
            : StageGateCheckResult.Failed(missing);
    }
}
