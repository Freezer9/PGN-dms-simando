using Simando.Application.Workflow;

namespace Simando.Infrastructure.Workflow;

internal sealed class WorkflowEventNotifier : IWorkflowEventNotifier
{
    public event Action<Guid?>? WorkflowStateChanged;

    public void NotifyUser(Guid userId)
    {
        WorkflowStateChanged?.Invoke(userId);
    }

    public void NotifyWorkflowChanged()
    {
        WorkflowStateChanged?.Invoke(null);
    }
}
