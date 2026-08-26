namespace Simando.Application.Workflow;

public interface IWorkflowEventNotifier
{
    event Action<Guid?>? WorkflowStateChanged;
    void NotifyUser(Guid userId);
    void NotifyWorkflowChanged();
}
