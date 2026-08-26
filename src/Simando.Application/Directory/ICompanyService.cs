using Simando.Application.Common;
using Simando.Application.Nol;
using Simando.Application.Registration;
using Simando.Domain.Security;

namespace Simando.Application.Directory;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyListItem>> GetListAsync(CompanyListFilter filter, CancellationToken ct = default);

    Task<PagedResult<CompanyListItem>> GetPagedListAsync(CompanyListFilter filter, CancellationToken ct = default);

    Task<CreateCompanyResult> CreateAsync(
        CreateCompanyRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<SoftDeleteResult> SoftDeleteAsync(Guid companyId, Guid actorUserId, CancellationToken ct = default);

    Task<PlottingDetail?> GetPlottingAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> SavePlottingAsync(
        Guid companyId, SavePlottingRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<StageEditResult> PromoteToProspekAsync(
        Guid companyId, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<IReadOnlyList<ContactDetail>> GetContactsAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> AddContactAsync(
        Guid companyId, SaveContactRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<StageEditResult> UpdateContactAsync(
        Guid companyId, Guid contactId, SaveContactRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<StageEditResult> DeleteContactAsync(
        Guid companyId, Guid contactId, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<StageEditResult> UpdateLocationAsync(
        Guid companyId, double latitude, double longitude, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<SurveyDetail> GetSurveyAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> SaveSurveyFullAsync(
        Guid companyId,
        SaveSurveyRequest request,
        IReadOnlyList<SaveSurveyProductRequest> products,
        IReadOnlyList<SaveSurveyRawMaterialRequest> rawMaterials,
        IReadOnlyList<SaveSurveyMarketRequest> markets,
        IReadOnlyList<SaveSurveyEquipmentRequest> equipment,
        Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<A1RegistrationDetail?> GetA1RegistrationAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> SaveA1RegistrationAsync(
        Guid companyId, SaveA1RegistrationRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<NolRequestDetail?> GetNolRequestAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> SaveNolRequestAsync(
        Guid companyId, SaveNolRequestRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<NolEvaluationDetail?> GetNolEvaluationAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> SaveNolEvaluationAsync(
        Guid companyId, SaveNolEvaluationRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);

    Task<NolIssuanceDetail?> GetNolIssuanceAsync(Guid companyId, CancellationToken ct = default);

    Task<StageEditResult> SaveNolIssuanceAsync(
        Guid companyId, SaveNolIssuanceRequest request, Guid actorUserId, EffectivePermissions actor, CancellationToken ct = default);
}
