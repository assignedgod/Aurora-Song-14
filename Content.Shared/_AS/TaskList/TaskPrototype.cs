using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._AS.TaskList.Prototypes;

[Prototype("task"), Serializable, NetSerializable]
public sealed partial class TaskPrototype : IPrototype
{

    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<TaskGroupPrototype> TaskGroup = "CivilianTaskGroup";

    [DataField(required: true)]
    public int Reward;

    [DataField]
    public LocId Description = string.Empty;

    [DataField]
    public SpriteSpecifier? Sprite;

    [DataField(required: true)]
    public List<TaskItemEntry> ItemEntries = new();
}

[DataDefinition, Serializable, NetSerializable]
public readonly partial record struct TaskItemEntry()
{
    [DataField]
    public int Amount { get; init; } = 1;

    [DataField]
    public LocId Name { get; init; } = string.Empty;

    [DataField(required: true)]
    public EntityWhitelist Whitelist { get; init; } = default!;
}

// TODO: Create objectives as tasks
// public readonly partial record struct TaskObjectiveEntry()
// {
//     [DataField]
//     public
// }
