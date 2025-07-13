using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.TaskList;

[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct TaskData
{
    [DataField]
    public string Id { get; init; } = string.Empty;

    [DataField(required: true)]
    public ProtoId<TaskPrototype> Task { get; init; } = string.Empty;

    public TaskData(TaskPrototype task, int uniqueIdentifier)
    {
        Task = task.ID;
        Id = $"{uniqueIdentifier}";
    }
}
