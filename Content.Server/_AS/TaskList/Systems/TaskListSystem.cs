

using System.Linq;
using Content.Server._NF.Bank;
using Content.Server.Chat.Managers;
using Content.Server.NameIdentifier;
using Content.Shared.NameIdentifier;
using Content.Server._AS.TaskList.Components;
using Content.Shared.Chat;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._AS.TaskList.Prototypes;
using Content.Shared._AS.TaskList;
using Content.Shared._AS.TaskList.Components;
using System.Threading.Tasks;

namespace Content.Server._AS.TaskList;

public sealed partial class TaskListSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bankSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string TaskNameIdentifierGroup = "Bounty"; // Use the bounty name ID group (0-999) for now.

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TaskConsoleComponent, BoundUIOpenedEvent>(OnTaskConsoleOpened);
        SubscribeLocalEvent<TaskConsoleComponent, TaskCompletedMessage>(OnTaskCompletedMessage);
        SubscribeLocalEvent<TaskConsoleComponent, NewTaskMessage>(OnNewTaskMessage);

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<TaskCompletedEvent>(OnTaskCompleted);
    }

    private void OnNewTaskMessage(Entity<TaskConsoleComponent> ent, ref NewTaskMessage args)
    {
        TryCreateTask(ent, args.Actor);
    }

    private void OnTaskConsoleOpened(EntityUid uid, TaskConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (!TryComp<PlayerTaskDatabaseComponent>(args.Actor, out var taskDb))
            return;

        _uiSystem.SetUiState(uid, TaskConsoleUiKey.Task, new TaskConsoleState(taskDb.Tasks));
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        TryCreateTask(args.Entity);
    }

    public void TryCreateTask(EntityUid uid)
    {
        var component = EnsureComp<PlayerTaskDatabaseComponent>(uid);

        if (component.TotalTasks >= component.MaxTasks)
            return;

        _nameIdentifier.GenerateUniqueName(uid, TaskNameIdentifierGroup, out var randomVal);
        var allTasks = _protoMan.EnumeratePrototypes<TaskPrototype>().ToList();
        var task = _random.Pick(allTasks);
        component.Tasks.Add(new TaskData(task, randomVal));
        component.TotalTasks = component.Tasks.Count;
    }

    public void TryCreateTask(EntityUid uid, EntityUid actorUid)
    {
        var component = EnsureComp<PlayerTaskDatabaseComponent>(actorUid);

        if (component.TotalTasks >= component.MaxTasks)
            return;

        _nameIdentifier.GenerateUniqueName(actorUid, TaskNameIdentifierGroup, out var randomVal);
        var allTasks = _protoMan.EnumeratePrototypes<TaskPrototype>().ToList();
        var task = _random.Pick(allTasks);
        component.Tasks.Add(new TaskData(task, randomVal));
        component.TotalTasks = component.Tasks.Count;
        _uiSystem.SetUiState(uid, TaskConsoleUiKey.Task, new TaskConsoleState(component.Tasks));
    }

    private void OnTaskCompletedMessage(EntityUid uid, TaskConsoleComponent component, TaskCompletedMessage args)
    {
        if (!TryComp<PlayerTaskDatabaseComponent>(args.Actor, out var taskDb))
            return;

        var task = taskDb.Tasks.Find(t => t.Id == args.TaskId);
        CompleteTask(uid, args.Actor, task);
    }

    private bool CompleteTask(EntityUid uid, EntityUid actorUid, TaskData task)
    {
        var ev = new TaskCompletedEvent(uid, actorUid, task);
        RaiseLocalEvent(ref ev);
        return true;
    }

    private void OnTaskCompleted(ref TaskCompletedEvent ev)
    {
        var actorUid = ev.ActorUid;
        var data = ev.Task;
        var uid = ev.Uid;

        if (!_protoMan.TryIndex(data.Task, out var task))
            return;

        if (!_bankSystem.TryBankDeposit(actorUid, task.Reward))
        {
            Log.Error($"Failed to deposit station pay for uid: {ToPrettyString(actorUid)}");
        }

        if (!TryComp<PlayerTaskDatabaseComponent>(actorUid, out var taskDb))
            return;

        taskDb.History.Add(data);
        taskDb.Tasks.Remove(data);
        taskDb.TotalTasks = taskDb.Tasks.Count;
        _uiSystem.SetUiState(uid, TaskConsoleUiKey.Task, new TaskConsoleState(taskDb.Tasks));
    }
}

[ByRefEvent]
public readonly record struct TaskCompletedEvent(EntityUid Uid, EntityUid ActorUid, TaskData Task);
