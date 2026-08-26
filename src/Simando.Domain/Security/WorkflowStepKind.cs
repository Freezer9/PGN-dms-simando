namespace Simando.Domain.Security;

// The chain positions a role can be assigned to, for turn-checking only —
// not the transition graph (Setuju/Revisi/Tolak movement is a later task).
public enum WorkflowStepKind
{
    AreaHead,
    RegionalAdmin,
    Reviewer1,
    Reviewer2,
    Reviewer3,
    DivisionHead,
}
