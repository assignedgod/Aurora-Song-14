using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.TaskList.Prototypes;

[Prototype("taskGroup"), Serializable, NetSerializable]
public sealed partial class TaskGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = "BaseTaskGroup";

    [DataField(required: true)]
    public string RewardType = "SpaceCash";

    [DataField]
    public List<string> GroupJobs = new();
}
