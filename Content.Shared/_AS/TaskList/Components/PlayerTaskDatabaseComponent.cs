using Content.Shared._AS.TaskList.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._AS.TaskList.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PlayerTaskDatabaseComponent : Component
{
    [DataField]
    public int MaxTasks = 4;

    [DataField]
    public List<TaskData> Tasks = new();

    [DataField]
    public List<TaskData> History = new();

    [DataField]
    public int TotalTasks = 0;

    [DataField(required: true)]
    public TaskGroupPrototype TaskGroup = new();
}
