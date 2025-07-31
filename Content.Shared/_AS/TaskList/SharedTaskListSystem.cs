using Robust.Shared.Serialization;

namespace Content.Shared._AS.TaskList;

public abstract class SharedTaskListSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }
}

[NetSerializable, Serializable]
public enum TaskConsoleUiKey : byte
{
    Task
}
