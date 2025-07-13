using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Content.Server.TaskList.Components;

[RegisterComponent]
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
}
