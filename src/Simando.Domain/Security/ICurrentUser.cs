namespace Simando.Domain.Security;

// docs/design/roles-permissions.md §4 "Scope resolution", verbatim.
public interface ICurrentUser
{
    Guid UserId { get; }
    AccessScope Scope { get; }
    Guid? AreaId { get; }
    Guid? RegionId { get; }
    bool HasCapability(Capability capability);
}
