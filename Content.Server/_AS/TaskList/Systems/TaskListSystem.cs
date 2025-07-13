

using System.Linq;
using Content.Server._NF.Bank;
using Content.Server.Chat.Managers;
using Content.Server.NameIdentifier;
using Content.Shared.NameIdentifier;
using Content.Server.TaskList.Components;
using Content.Shared.Chat;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.TaskList;

public sealed partial class TaskListSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bankSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;

    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string TaskNameIdentifierGroup = "Bounty"; // Use the bounty name ID group (0-999) for now.

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<TaskCompletedEvent>(OnTaskCompleted);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        TryCreateTask(args.Entity);
    }

    public void TryCreateTask(EntityUid uid)
    {
        var component = EnsureComp<PlayerTaskDatabaseComponent>(uid);
        _nameIdentifier.GenerateUniqueName(uid, TaskNameIdentifierGroup, out var randomVal);
        var allTasks = _protoMan.EnumeratePrototypes<TaskPrototype>().ToList();
        var task = _random.Pick(allTasks);
        component.Tasks.Add(new TaskData(task, randomVal));
        component.TotalTasks = component.Tasks.Count;
    }

    private bool CompleteTask(EntityUid uid, TaskData task)
    {
        var ev = new TaskCompletedEvent(uid, task);
        RaiseLocalEvent(ref ev);
        return true;
    }

    private void OnTaskCompleted(ref TaskCompletedEvent ev)
    {
        var data = ev.Task;
        var uid = ev.Uid;

        if (!_protoMan.TryIndex(data.Task, out var task))
            return;

        if (!_bankSystem.TryBankDeposit(uid, task.Reward))
        {
            Log.Error($"Failed to deposit station pay for uid: {ToPrettyString(uid)}");
        }
    }
}

[ByRefEvent]
public readonly record struct TaskCompletedEvent(EntityUid Uid, TaskData Task);
