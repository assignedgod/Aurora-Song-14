using System.Threading.Tasks;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.TaskList.Components;

[RegisterComponent]
public sealed partial class TaskConsoleComponent : Component
{
    [DataField]
    public int Placeholder = 1;
}

[NetSerializable, Serializable]
public sealed class TaskConsoleState : BoundUserInterfaceState
{
    public List<TaskData> Tasks;

    public TaskConsoleState(List<TaskData> tasks)
    {
        Tasks = tasks;
    }
}

[NetSerializable, Serializable]
public sealed class TaskCompletedMessage : BoundUserInterfaceMessage
{
    public string TaskId;

    public TaskCompletedMessage(string taskId)
    {
        TaskId = taskId;
    }
}

[NetSerializable, Serializable]
public sealed class NewTaskMessage : BoundUserInterfaceMessage
{
    public NewTaskMessage() { }
}
