using System.Diagnostics.CodeAnalysis;
using Content.Shared.Mind;
using Robust.Shared.Containers;

namespace Content.Shared._AS.Licence;

public sealed class LicenceSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LicenceComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<LicenceComponent> ent, ref ComponentInit args)
    {
        if (ent.Comp.LicenceName is not {} licence)
            return;

        var newItemName = TryGetOwnerName(ent, out var owner)
            ? Loc.GetString(ent.Comp.OwnerLoc, ("owner", owner), ("licence", licence))
            : Loc.GetString(ent.Comp.NoOwnerLoc, ("licence", licence));
        _meta.SetEntityName(ent, newItemName);
    }

    private bool TryGetOwnerName(EntityUid ent, [NotNullWhen(true)] out string? owner)
    {
        owner = null;
        if (Transform(ent) is not { } xform)
            return false;
        // Outermost container owner is expected to be player entity
        // This is true in this case as licences will be generated before a owner will be entering a potential container
        if (!_container.TryGetOuterContainer(ent, xform, out var container))
            return false;
        // Getting the player character name via mind
        if (_mind.TryGetMind(container.Owner, out _, out var mindComp))
            owner = mindComp.CharacterName;
        return owner != null;
    }
}
