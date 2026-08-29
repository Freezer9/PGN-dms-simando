using Simando.Domain.Security;

namespace Simando.Application.Documents;

public sealed record GeneratedDocumentResult(
    byte[] Content,
    string Filename,
    bool CompanyFound,
    bool Authorized)
{
    public static GeneratedDocumentResult Success(byte[] content, string filename) =>
        new(content, filename, true, true);
    public static GeneratedDocumentResult NotFoundResult() =>
        new([], string.Empty, false, false);
    public static GeneratedDocumentResult ForbiddenResult() =>
        new([], string.Empty, true, false);
}

public interface IDocumentGenerator
{
    Task<GeneratedDocumentResult> GenerateKk0DocxAsync(Guid companyId, EffectivePermissions permissions, CancellationToken ct = default);
    Task<GeneratedDocumentResult> GenerateA1DocxAsync(Guid companyId, EffectivePermissions permissions, CancellationToken ct = default);
    Task<GeneratedDocumentResult> GenerateNolRequestDocxAsync(Guid companyId, EffectivePermissions permissions, CancellationToken ct = default);
    Task<GeneratedDocumentResult> GenerateEvaluationResumeDocxAsync(Guid companyId, EffectivePermissions permissions, CancellationToken ct = default);
    Task<GeneratedDocumentResult> GenerateNolIssuanceDocxAsync(Guid companyId, EffectivePermissions permissions, CancellationToken ct = default);

    // Raw generators without permission check for internal/system calls
    Task<byte[]> GenerateKk0DocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateA1DocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateNolRequestDocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateEvaluationResumeDocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateNolIssuanceDocxAsync(Guid companyId, CancellationToken ct = default);
}
