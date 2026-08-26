namespace Simando.Application.Documents;

public interface IDocumentGenerator
{
    Task<byte[]> GenerateKk0DocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateA1DocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateNolRequestDocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateEvaluationResumeDocxAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> GenerateNolIssuanceDocxAsync(Guid companyId, CancellationToken ct = default);
}
