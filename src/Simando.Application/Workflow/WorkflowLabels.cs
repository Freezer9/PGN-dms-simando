using Simando.Domain.Audit;
using Simando.Domain.Security;

namespace Simando.Application.Workflow;

// Indonesian display labels for the workflow vocabulary — shared between
// Tasks.razor and CompanyHub.razor (and CompanyDetailService's timeline
// role labels) so both pages read from one source instead of duplicating
// the switch.
public static class WorkflowLabels
{
    public static string StepKindLabel(WorkflowStepKind kind) => kind switch
    {
        WorkflowStepKind.AreaHead => "Area Head",
        WorkflowStepKind.RegionalAdmin => "Admin Regional",
        WorkflowStepKind.Reviewer1 => "Reviewer 1",
        WorkflowStepKind.Reviewer2 => "Reviewer 2",
        WorkflowStepKind.Reviewer3 => "Reviewer 3",
        WorkflowStepKind.DivisionHead => "Div. Head",
        _ => kind.ToString(),
    };

    public static string ActionLabel(StatusEventAction action) => action switch
    {
        StatusEventAction.Submit => "Diajukan",
        StatusEventAction.Setuju => "Setuju",
        StatusEventAction.Revisi => "Revisi",
        StatusEventAction.Tolak => "Tolak",
        StatusEventAction.Issue => "Diterbitkan",
        StatusEventAction.Save => "Disimpan",
        StatusEventAction.Create => "Dibuat",
        StatusEventAction.Reassign => "Tugaskan Ulang",
        StatusEventAction.Rework => "Dikembalikan ke Draft",
        StatusEventAction.Discontinue => "Dihentikan",
        _ => action.ToString(),
    };

    // Plain elapsed time, no colour banding or threshold — docs/design/
    // frontend/04-record-hub.md "Ageing is plain elapsed time".
    public static string WaitingLabel(DateTimeOffset waitingSince)
    {
        var days = (int)(DateTimeOffset.UtcNow - waitingSince).TotalDays;
        return days <= 0 ? "hari ini" : $"{days} hari";
    }
}
