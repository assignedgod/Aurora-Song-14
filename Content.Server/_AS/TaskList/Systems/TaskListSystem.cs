

using System.Linq;
using Content.Server._NF.Bank;
using Content.Server.Chat.Managers;
using Content.Server.NameIdentifier;
using Content.Shared.NameIdentifier;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._AS.TaskList.Prototypes;
using Content.Shared._AS.TaskList;
using Content.Shared._AS.TaskList.Components;
using Content.Shared._NF.Roles.Components;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Content.Shared._AS.CCVar;
using Content.Shared.Stacks;
using Content.Server.Stack;
using Content.Shared.Coordinates;
using Content.Server.Hands.Systems;
using Robust.Server.GameObjects;

namespace Content.Server._AS.TaskList;

public sealed partial class TaskListSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bankSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string TaskNameIdentifierGroup = "Bounty"; // Use the bounty name ID group (0-999) for now.

    private bool _startWithTasks = false;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configManager, AuroraCVars.StartWithTasks, value => _startWithTasks = value, true);

        SubscribeLocalEvent<TaskConsoleComponent, BoundUIOpenedEvent>(OnTaskConsoleOpened);
        SubscribeLocalEvent<TaskConsoleComponent, TaskCompletedMessage>(OnTaskCompletedMessage);
        SubscribeLocalEvent<TaskConsoleComponent, NewTaskMessage>(OnNewTaskMessage);

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
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

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        TryCreateDatabase(ev.Mob);
    }

    private void TryCreateDatabase(EntityUid uid)
    {
        var taskDb = EnsureComp<PlayerTaskDatabaseComponent>(uid);
        var groups = _protoMan.EnumeratePrototypes<TaskGroupPrototype>().ToList();

        if (TryComp<JobTrackingComponent>(uid, out var jobTracker))
        {
            if (jobTracker.Job != null)
            {
                var taskGroup = groups.Find(x => x.GroupJobs.Contains(jobTracker.Job));
                taskDb.TaskGroup = taskGroup != null ? taskGroup : new TaskGroupPrototype();
            }
        }

        if (_startWithTasks) TryCreateTask(uid);
    }

    public void TryCreateTask(EntityUid uid)
    {
        if (!TryComp<PlayerTaskDatabaseComponent>(uid, out var taskDb))
            return;

        if (taskDb.TotalTasks >= taskDb.MaxTasks)
            return;

        CreateTask(uid, taskDb);
    }

    public void TryCreateTask(EntityUid uid, EntityUid actorUid)
    {
        if (!TryComp<PlayerTaskDatabaseComponent>(uid, out var taskDb))
            return;

        if (taskDb.TotalTasks >= taskDb.MaxTasks)
            return;

        CreateTask(actorUid, taskDb);

        _uiSystem.SetUiState(uid, TaskConsoleUiKey.Task, new TaskConsoleState(taskDb.Tasks));
    }

    private void CreateTask(EntityUid uid, PlayerTaskDatabaseComponent taskDb)
    {
        _nameIdentifier.GenerateUniqueName(uid, TaskNameIdentifierGroup, out var randomVal);
        var allTasks = _protoMan.EnumeratePrototypes<TaskPrototype>().ToList().FindAll(x => x.TaskGroup == taskDb.TaskGroup);
        var task = _random.Pick(allTasks);
        taskDb.Tasks.Add(new TaskData(task, randomVal));
        taskDb.TotalTasks = taskDb.Tasks.Count;
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


        if (!TryComp<PlayerTaskDatabaseComponent>(actorUid, out var taskDb))
            return;

        var rewardPrototype = _protoMan.Index<StackPrototype>(taskDb.TaskGroup.RewardType);
        var stackUid = _stack.Spawn(task.Reward, rewardPrototype, actorUid.ToCoordinates());

        if (!_hands.TryPickupAnyHand(actorUid, stackUid))
            _transform.SetLocalRotation(stackUid, Angle.Zero); // Orient these to grid north instead of map north

        taskDb.History.Add(data);
        taskDb.Tasks.Remove(data);
        taskDb.TotalTasks = taskDb.Tasks.Count;
        _uiSystem.SetUiState(uid, TaskConsoleUiKey.Task, new TaskConsoleState(taskDb.Tasks));
    }
}

[ByRefEvent]
public readonly record struct TaskCompletedEvent(EntityUid Uid, EntityUid ActorUid, TaskData Task);
