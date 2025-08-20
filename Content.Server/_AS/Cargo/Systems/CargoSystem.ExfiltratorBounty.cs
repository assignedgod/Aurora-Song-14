using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._NF.Contraband.Components;
using Content.Server._AS.Exfiltrator.Components;
using Content.Server.NameIdentifier;
using Content.Server.Labels;
using Content.Server.Paper;
using Content.Server._AS.SectorServices;
using Content.Shared._AS.Exfiltrator;
using Content.Shared._AS.Exfiltrator.Components;
using Content.Shared._AS.Exfiltrator.Prototypes;
using Content.Shared._AS.Exfiltrator.Events;
using Content.Shared.Access.Components;
using Content.Shared.Database;
using Content.Shared.NameIdentifier;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._AS.Cargo.Systems;

public sealed partial class CargoSystem : EntitySystem
{
    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string ExfiltratorBountyNameIdentifierGroup = "Bounty"; // Use the bounty name ID group (0-999) for now.

    private EntityQuery<ContainerManagerComponent> _containerQuery;
    private EntityQuery<ExfiltratorBountyLabelComponent> _exfiltratorBountyLabelQuery;


    private readonly TimeSpan _redemptionDelay = TimeSpan.FromSeconds(2);

    private void InitializeExfiltratorBounty()
    {
        SubscribeLocalEvent<ExfiltratorBountyConsoleComponent, BoundUIOpenedEvent>(OnExfiltratorBountyConsoleOpened);
        SubscribeLocalEvent<ExfiltratorBountyConsoleComponent, ExfiltratorBountyAcceptMessage>(OnExfiltratorBountyAccept);
        SubscribeLocalEvent<ExfiltratorBountyConsoleComponent, ExfiltratorBountySkipMessage>(OnSkipExfiltratorBountyMessage);

        SubscribeLocalEvent<ExfiltratorBountyRedemptionConsoleComponent, ExfiltratorBountyRedemptionMessage>(OnRedeemBounty);

        SubscribeLocalEvent<SectorExfiltratorBountyDatabaseComponent, MapInitEvent>(OnExfiltratorMapInit);

        _exfiltratorBountyLabelQuery = GetEntityQuery<ExfiltratorBountyLabelComponent>();
        _containerQuery = GetEntityQuery<ContainerManagerComponent>();
    }

    private void OnExfiltratorBountyConsoleOpened(EntityUid uid, ExfiltratorBountyConsoleComponent component, BoundUIOpenedEvent args)
    {
        var service = _sectorService.GetServiceEntity();
        if (!TryComp<SectorExfiltratorBountyDatabaseComponent>(service, out var bountyDb))
        {
            return;
        }

        var untilNextSkip = bountyDb.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid, ExfiltratorConsoleUiKey.Bounty, new ExfiltratorBountyConsoleState(bountyDb.Bounties, untilNextSkip));
    }

    private void OnExfiltratorBountyAccept(EntityUid uid, ExfiltratorBountyConsoleComponent component, ExfiltratorBountyAcceptMessage args)
    {
        if (_timing.CurTime < component.NextPrintTime)
            return;

        var service = _sectorService.GetServiceEntity();
        if (!TryGetExfiltratorBountyFromId(service, args.BountyId, out var bounty))
            return;

        var bountyObj = bounty.Value;

        // Check if the crate for this bounty has already been summoned.  If not, create a new one.
        if (bountyObj.Accepted || !_protoMan.TryIndex(bountyObj.Bounty, out var bountyPrototype))
            return;

        ExfiltratorBountyData bountyData = new ExfiltratorBountyData(bountyPrototype!, bountyObj.Id, true);

        TryOverwriteExfiltratorBountyFromId(service, bountyData);

        if (bountyPrototype.SpawnChest)
        {
            var chest = Spawn(component.BountyCrateId, Transform(uid).Coordinates);
            SetupExfiltratorBountyChest(chest, bountyData, bountyPrototype);
            _audio.PlayPvs(component.SpawnChestSound, uid);
        }
        else
        {
            var label = Spawn(component.BountyLabelId, Transform(uid).Coordinates);
            SetupExfiltratorBountyManifest(label, bountyData, bountyPrototype);
            _audio.PlayPvs(component.PrintSound, uid);
        }

        component.NextPrintTime = _timing.CurTime + component.PrintDelay;
        UpdateExfiltratorBountyConsoles();
    }

    private void OnSkipExfiltratorBountyMessage(EntityUid uid, ExfiltratorBountyConsoleComponent component, ExfiltratorBountySkipMessage args)
    {
        var service = _sectorService.GetServiceEntity();
        if (!TryComp<SectorExfiltratorBountyDatabaseComponent>(service, out var db))
            return;

        if (_timing.CurTime < db.NextSkipTime)
            return;

        if (!TryGetExfiltratorBountyFromId(service, args.BountyId, out var bounty))
            return;

        if (args.Actor is not { Valid: true } mob)
            return;

        if (TryComp<AccessReaderComponent>(uid, out var accessReaderComponent) &&
            !_accessReaderSystem.IsAllowed(mob, uid, accessReaderComponent))
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        if (!TryRemoveExfiltratorBounty(service, bounty.Value.Id))
            return;

        FillExfiltratorBountyDatabase(service);
        if (bounty.Value.Accepted)
            db.NextSkipTime = _timing.CurTime + db.SkipDelay;
        else
            db.NextSkipTime = _timing.CurTime + db.CancelDelay;

        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid, ExfiltratorConsoleUiKey.Bounty, new ExfiltratorBountyConsoleState(db.Bounties, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }

    private void SetupExfiltratorBountyChest(EntityUid uid, ExfiltratorBountyData bounty, ExfiltratorBountyPrototype prototype)
    {
        _metaSystem.SetEntityName(uid, Loc.GetString("exfiltrator-bounty-chest-name", ("id", bounty.Id)));

        FormattedMessage message = new FormattedMessage();
        message.TryAddMarkup(Loc.GetString("exfiltrator-bounty-chest-description-start"), out var _);
        foreach (var entry in prototype.Entries)
        {
            message.PushNewline();
            message.TryAddMarkup($"- {Loc.GetString("exfiltrator-bounty-console-manifest-entry",
                ("amount", entry.Amount),
                ("item", Loc.GetString(entry.Name)))}", out var _);
        }
        message.PushNewline();
        message.TryAddMarkup(Loc.GetString("exfiltrator-bounty-console-manifest-reward", ("reward", prototype.Reward)), out var _);

        _metaSystem.SetEntityDescription(uid, message.ToMarkup());

        if (TryComp<ExfiltratorBountyLabelComponent>(uid, out var label))
            label.Id = bounty.Id;
    }

    private void SetupExfiltratorBountyManifest(EntityUid uid, ExfiltratorBountyData bounty, ExfiltratorBountyPrototype prototype, PaperComponent? paper = null)
    {
        _metaSystem.SetEntityName(uid, Loc.GetString("exfiltrator-bounty-manifest-name", ("id", bounty.Id)));

        if (!Resolve(uid, ref paper))
            return;

        var msg = new FormattedMessage();
        msg.AddText(Loc.GetString("exfiltrator-bounty-manifest-header", ("id", bounty.Id)));
        msg.PushNewline();
        msg.AddText(Loc.GetString("exfiltrator-bounty-manifest-list-start"));
        msg.PushNewline();
        foreach (var entry in prototype.Entries)
        {
            msg.TryAddMarkup($"- {Loc.GetString("exfiltrator-bounty-console-manifest-entry",
                ("amount", entry.Amount),
                ("item", Loc.GetString(entry.Name)))}", out var _);
            msg.PushNewline();
        }
        msg.TryAddMarkup(Loc.GetString("exfiltrator-bounty-console-manifest-reward", ("reward", prototype.Reward)), out var _);
        _paperSystem.SetContent(uid, msg.ToMarkup(), paper);
    }

    private bool TryGetExfiltratorBountyLabel(EntityUid uid,
        [NotNullWhen(true)] out EntityUid? labelEnt,
        [NotNullWhen(true)] out ExfiltratorBountyLabelComponent? labelComp)
    {
        labelEnt = null;
        labelComp = null;
        if (!_containerQuery.TryGetComponent(uid, out var containerMan))
            return false;

        // make sure this label was actually applied to a crate.
        if (!_container.TryGetContainer(uid, LabelSystem.ContainerName, out var container, containerMan))
            return false;

        if (container.ContainedEntities.FirstOrNull() is not { } label ||
            !_exfiltratorBountyLabelQuery.TryGetComponent(label, out var component))
            return false;

        labelEnt = label;
        labelComp = component;
        return true;
    }

    private void OnExfiltratorMapInit(EntityUid uid, SectorExfiltratorBountyDatabaseComponent component, MapInitEvent args)
    {
        FillExfiltratorBountyDatabase(uid, component);
    }

    /// <summary>
    /// Fills up the bounty database with random bounties.
    /// </summary>
    public void FillExfiltratorBountyDatabase(EntityUid serviceId, SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!Resolve(serviceId, ref component))
            return;

        while (component?.Bounties.Count < component?.MaxBounties)
        {
            if (!TryAddExfiltratorBounty(serviceId, component))
                break;
        }

        UpdateExfiltratorBountyConsoles();
    }

    [PublicAPI]
    public bool TryAddExfiltratorBounty(EntityUid serviceId, SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!Resolve(serviceId, ref component))
            return false;

        // todo: consider making the exfiltrator bounties weighted.
        var allBounties = _protoMan.EnumeratePrototypes<ExfiltratorBountyPrototype>().ToList();
        var filteredBounties = new List<ExfiltratorBountyPrototype>();
        foreach (var proto in allBounties)
        {
            if (component.Bounties.Any(b => b.Bounty == proto.ID))
                continue;
            filteredBounties.Add(proto);
        }

        var pool = filteredBounties.Count == 0 ? allBounties : filteredBounties;
        var bounty = _random.Pick(pool);
        return TryAddExfiltratorBounty(serviceId, bounty, component);
    }

    [PublicAPI]
    public bool TryAddExfiltratorBounty(EntityUid serviceId, string bountyId, SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!_protoMan.TryIndex<ExfiltratorBountyPrototype>(bountyId, out var bounty))
        {
            return false;
        }

        return TryAddExfiltratorBounty(serviceId, bounty, component);
    }

    public bool TryAddExfiltratorBounty(EntityUid serviceId, ExfiltratorBountyPrototype bounty, SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!Resolve(serviceId, ref component))
            return false;

        if (component.Bounties.Count >= component.MaxBounties)
            return false;

        _nameIdentifier.GenerateUniqueName(serviceId, ExfiltratorBountyNameIdentifierGroup, out var randomVal); // Need a string ID for internal name, probably doesn't need to be outward facing.
        component.Bounties.Add(new ExfiltratorBountyData(bounty, randomVal, false));
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"Added exfiltrator bounty \"{bounty.ID}\" (id:{component.TotalBounties}) to service {ToPrettyString(serviceId)}");
        component.TotalBounties++;
        return true;
    }

    [PublicAPI]
    public bool TryRemoveExfiltratorBounty(EntityUid serviceId, string dataId, SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!TryGetExfiltratorBountyFromId(serviceId, dataId, out var data, component))
            return false;

        return TryRemoveExfiltratorBounty(serviceId, data.Value, component);
    }

    public bool TryRemoveExfiltratorBounty(EntityUid serviceId, ExfiltratorBountyData data, SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!Resolve(serviceId, ref component))
            return false;

        for (var i = 0; i < component.Bounties.Count; i++)
        {
            if (component.Bounties[i].Id == data.Id)
            {
                component.Bounties.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public bool TryGetExfiltratorBountyFromId(
        EntityUid uid,
        string id,
        [NotNullWhen(true)] out ExfiltratorBountyData? bounty,
        SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        bounty = null;
        if (!Resolve(uid, ref component))
            return false;

        foreach (var bountyData in component.Bounties)
        {
            if (bountyData.Id != id)
                continue;
            bounty = bountyData;
            break;
        }

        return bounty != null;
    }

    private bool TryOverwriteExfiltratorBountyFromId(
        EntityUid uid,
        ExfiltratorBountyData bounty,
        SectorExfiltratorBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        for (int i = 0; i < component.Bounties.Count; i++)
        {
            if (bounty.Id == component.Bounties[i].Id)
            {
                component.Bounties[i] = bounty;
                return true;
            }
        }
        return false;
    }

    public void UpdateExfiltratorBountyConsoles()
    {
        var query = EntityQueryEnumerator<ExfiltratorBountyConsoleComponent, UserInterfaceComponent>();

        var serviceId = _sectorService.GetServiceEntity();
        if (!TryComp<SectorExfiltratorBountyDatabaseComponent>(serviceId, out var db))
            return;

        while (query.MoveNext(out var uid, out _, out var ui))
        {
            var untilNextSkip = db.NextSkipTime - _timing.CurTime;
            _uiSystem.SetUiState((uid, ui), ExfiltratorConsoleUiKey.Bounty, new ExfiltratorBountyConsoleState(db.Bounties, untilNextSkip));
        }
    }

    private List<(EntityUid Entity, ContrabandPalletComponent Component)> GetContrabandPallets(EntityUid gridUid)
    {
        var pads = new List<(EntityUid, ContrabandPalletComponent)>();
        var query = AllEntityQuery<ContrabandPalletComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var compXform))
        {
            if (compXform.ParentUid != gridUid ||
                !compXform.Anchored)
            {
                continue;
            }

            pads.Add((uid, comp));
        }

        return pads;
    }

    private void OnRedeemBounty(EntityUid uid, ExfiltratorBountyRedemptionConsoleComponent component, ExfiltratorBountyRedemptionMessage args)
    {
        var amount = 0;

        // Component still cooling down.
        if (component.LastRedeemAttempt + _redemptionDelay > _timing.CurTime)
            return;

        EntityUid gridUid = Transform(uid).GridUid ?? EntityUid.Invalid;
        if (gridUid == EntityUid.Invalid)
            return;

        // 1. Separate out accepted crate and non-crate bounties.  Create a tracker for non-crate bounties.
        if (!TryComp<SectorExfiltratorBountyDatabaseComponent>(_sectorService.GetServiceEntity(), out var bountyDb))
            return;

        ExfiltratorBountyEntitySearchState bountySearchState = new ExfiltratorBountyEntitySearchState();

        foreach (var bounty in bountyDb.Bounties)
        {
            if (bounty.Accepted)
            {
                if (!_protoMan.TryIndex(bounty.Bounty, out var bountyPrototype))
                    continue;
                if (bountyPrototype.SpawnChest)
                {
                    var newState = new ExfiltratorBountyState(bounty, bountyPrototype);
                    foreach (var entry in bountyPrototype.Entries)
                    {
                        newState.Entries[entry.Name] = 0;
                    }
                    bountySearchState.CrateBounties[bounty.Id] = newState;
                }
                else
                {
                    var newState = new ExfiltratorBountyState(bounty, bountyPrototype);
                    foreach (var entry in bountyPrototype.Entries)
                    {
                        newState.Entries[entry.Name] = 0;
                    }
                    bountySearchState.LooseObjectBounties[bounty.Id] = newState;
                }
            }
        }

        // 2. Iterate over bounty pads, find all tagged, non-tagged items.
        foreach (var (palletUid, _) in GetContrabandPallets(gridUid))
        {
            foreach (var ent in _lookup.GetEntitiesIntersecting(palletUid,
                         LookupFlags.Dynamic | LookupFlags.Sundries | LookupFlags.Approximate))
            {
                // Dont match:
                // - anything anchored (e.g. light fixtures)
                // Checks against already handled set done by CheckEntityForExfiltratorBounties
                if (_xformQuery.TryGetComponent(ent, out var xform) &&
                    xform.Anchored)
                {
                    continue;
                }

                CheckEntityForExfiltratorBounties(ent, ref bountySearchState);
            }
        }

        // 4. When done, note all completed bounties.  Remove them from the list of accepted bounties, and spawn the rewards.
        bool bountiesRemoved = false;
        string redeemedBounties = string.Empty;
        foreach (var (id, bounty) in bountySearchState.CrateBounties)
        {
            bool bountyMet = true;
            var prototype = bounty.Prototype;
            foreach (var entry in prototype.Entries)
            {
                if (!bounty.Entries.ContainsKey(entry.Name) ||
                    entry.Amount > bounty.Entries[entry.Name])
                {
                    bountyMet = false;
                    break;
                }
            }

            if (bountyMet)
            {
                bountiesRemoved = true;
                redeemedBounties = Loc.GetString("exfiltrator-bounty-redemption-append", ("bounty", id), ("empty", string.IsNullOrEmpty(redeemedBounties) ? 0 : 1), ("prev", redeemedBounties));

                TryRemoveExfiltratorBounty(_sectorService.GetServiceEntity(), id);
                amount += prototype.Reward;
                foreach (var entity in bounty.Entities)
                {
                    Del(entity);
                }
            }
        }

        foreach (var (id, bounty) in bountySearchState.LooseObjectBounties)
        {
            bool bountyMet = true;
            var prototype = bounty.Prototype;
            foreach (var entry in prototype.Entries)
            {
                if (!bounty.Entries.ContainsKey(entry.Name) ||
                    entry.Amount > bounty.Entries[entry.Name])
                {
                    bountyMet = false;
                    break;
                }
            }

            if (bountyMet)
            {
                bountiesRemoved = true;
                redeemedBounties = Loc.GetString("exfiltrator-bounty-redemption-append", ("bounty", id), ("empty", string.IsNullOrEmpty(redeemedBounties) ? 0 : 1), ("prev", redeemedBounties));

                TryRemoveExfiltratorBounty(_sectorService.GetServiceEntity(), id);
                amount += prototype.Reward;
                foreach (var entity in bounty.Entities)
                {
                    Del(entity);
                }
            }
        }

        if (amount > 0)
        {
            // TODO: play a sound here, ideally the "deposit money" chime used on ATMs
            _stack.SpawnMultiple("Doubloon", amount, Transform(uid).Coordinates);
            _audio.PlayPvs(component.AcceptSound, uid);
            _popup.PopupEntity(Loc.GetString("exfiltrator-bounty-redemption-success", ("bounties", redeemedBounties), ("amount", amount)), args.Actor);
        }
        else
        {
            _audio.PlayPvs(component.DenySound, uid);
            _popup.PopupEntity(Loc.GetString("exfiltrator-bounty-redemption-deny"), args.Actor);
        }

        // Bounties removed, restore database list
        if (bountiesRemoved)
        {
            FillExfiltratorBountyDatabase(_sectorService.GetServiceEntity());
        }
        component.LastRedeemAttempt = _timing.CurTime;
    }

    class ExfiltratorBountyState
    {
        public readonly ExfiltratorBountyData Data;
        public ExfiltratorBountyPrototype Prototype;
        public HashSet<EntityUid> Entities = new();
        public Dictionary<string, int> Entries = new();
        public bool Calculating = false; // Relevant only for crate bounties (due to tree traversal)

        public ExfiltratorBountyState(ExfiltratorBountyData data, ExfiltratorBountyPrototype prototype)
        {
            Data = data;
            Prototype = prototype;
        }
    }

    class ExfiltratorBountyEntitySearchState
    {
        public HashSet<EntityUid> HandledEntities = new();
        public Dictionary<string, ExfiltratorBountyState> LooseObjectBounties = new();
        public Dictionary<string, ExfiltratorBountyState> CrateBounties = new();
    }

    private void CheckEntityForExfiltratorCrateBounty(EntityUid uid, ref ExfiltratorBountyEntitySearchState state, string id)
    {
        // Sanity check: entity previously handled, this subtree is done.
        if (state.HandledEntities.Contains(uid))
            return;

        // Add this container to the list of entities to remove.
        var bounty = state.CrateBounties[id]; // store the particular bounty we're looking up.
        if (bounty.Calculating) // Bounty check is already happening in a parent, return.
        {
            state.HandledEntities.Add(uid);
            return;
        }

        if (TryComp<ContainerManagerComponent>(uid, out var containers))
        {
            bounty.Entities.Add(uid);
            bounty.Calculating = true;

            foreach (var container in containers.Containers.Values)
            {
                foreach (var ent in container.ContainedEntities)
                {
                    // Subtree has a separate label, run check on that label
                    if (TryComp<ExfiltratorBountyLabelComponent>(ent, out var label))
                    {
                        CheckEntityForExfiltratorCrateBounty(ent, ref state, label.Id);
                    }
                    else
                    {
                        // Check entry against bounties
                        foreach (var entry in bounty.Prototype.Entries)
                        {
                            // Should add an assertion here, entry.Name should exist.
                            // Entry already fulfilled, skip this entity.
                            if (bounty.Entries[entry.Name] >= entry.Amount)
                            {
                                continue;
                            }

                            // Check whitelists for the exfiltrator bounty.
                            if ((_whitelistSys.IsWhitelistPass(entry.Whitelist, ent) ||
                                _entProtoIdWhitelist.IsWhitelistPass(entry.IdWhitelist, ent)) &&
                                _whitelistSys.IsBlacklistFailOrNull(entry.Blacklist, ent))
                            {
                                bounty.Entries[entry.Name]++;
                                bounty.Entities.Add(ent);
                                break;
                            }
                        }
                        state.HandledEntities.Add(ent);
                    }
                }
            }
        }
        state.HandledEntities.Add(uid);
    }

    // Return two lists: a list of non-labelled entities (nodes), and a list of labelled entities (subtrees)
    private void CheckEntityForExfiltratorBounties(EntityUid uid, ref ExfiltratorBountyEntitySearchState state)
    {
        // Entity previously handled, this subtree is done.
        if (state.HandledEntities.Contains(uid))
            return;

        // 3a. If tagged as labelled, check contents against crate bounties.  If it satisfies any of them, note it as solved.
        if (TryComp<ExfiltratorBountyLabelComponent>(uid, out var label))
            CheckEntityForExfiltratorCrateBounty(uid, ref state, label.Id);
        else
        {
            // 3b. If not tagged as labelled, check contents against non-create bounties.  If it satisfies any of them, increase the quantity.
            foreach (var (id, bounty) in state.LooseObjectBounties)
            {
                foreach (var entry in bounty.Prototype.Entries)
                {
                    // Should add an assertion here, entry.Name should exist.
                    // Entry already fulfilled, skip this entity.
                    if (bounty.Entries[entry.Name] >= entry.Amount)
                    {
                        continue;
                    }

                    // Check whitelists for the exfiltrator bounty.
                    if ((_whitelistSys.IsWhitelistPass(entry.Whitelist, uid) ||
                        _entProtoIdWhitelist.IsWhitelistPass(entry.IdWhitelist, uid)) &&
                        _whitelistSys.IsBlacklistFailOrNull(entry.Blacklist, uid))
                    {
                        bounty.Entries[entry.Name]++;
                        bounty.Entities.Add(uid);
                        state.HandledEntities.Add(uid);
                        return;
                    }
                }
            }
        }
        state.HandledEntities.Add(uid);
    }
}
