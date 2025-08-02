using System.Diagnostics.CodeAnalysis;
using Content.Shared.Mind;
using Robust.Shared.Containers;

namespace Content.Shared._AS.License;

public sealed class LicenseSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public void SetName(Entity<LicenseComponent> ent, string? owner = null, string? licenseName = null)
    {
        if (owner != null)
            ent.Comp.OwnerName = owner;

        if (licenseName != null)
            ent.Comp.LicenseName = licenseName;

        if (ent.Comp.LicenseName is not {} license)
            return;

        var newItemName = ent.Comp.OwnerName != null
            ? Loc.GetString(ent.Comp.OwnerLoc, ("owner", ent.Comp.OwnerName), ("license", license))
            : Loc.GetString(ent.Comp.NoOwnerLoc, ("license", license));
        _meta.SetEntityName(ent, newItemName);
    }
}
