using Content.Client._AS.TaskList.UI;
using Content.Shared._AS.TaskList.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._AS.TaskList.BUI;

[UsedImplicitly]
public sealed class TaskConsoleMenuBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private TaskConsoleMenu? _console;
    public TaskConsoleMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _console = this.CreateWindow<TaskConsoleMenu>();

        _console.OnCompleteButtonPressed += id =>
        {
            SendMessage(new TaskCompletedMessage(id));
        };

        _console.NewTaskRequested += OnNewTask;
    }

    private void OnNewTask()
    {
        SendMessage(new NewTaskMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not TaskConsoleState state)
            return;

        _console?.UpdateEntries(state.Tasks);
    }
}
