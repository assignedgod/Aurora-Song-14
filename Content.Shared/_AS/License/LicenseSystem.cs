using Content.Shared._AS.License.Components;
using Content.Shared._AS.License.Events;
using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._AS.License;

public sealed class LicenseSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly LicenseSystem _license = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LicenseConsoleComponent, PrintLicenseMessage>(OnPrintLicenseMessage);
        SubscribeLocalEvent<LicenseConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<LicenseConsoleComponent, ComponentRemove>(OnRemove);
    }

    private void OnInit(Entity<LicenseConsoleComponent> ent, ref ComponentInit args)
    {
        _slots.AddItemSlot(ent, LicenseConsoleComponent.HolderIdSlotId, ent.Comp.HolderIdSlot);
    }

    private void OnRemove(Entity<LicenseConsoleComponent> ent, ref ComponentRemove args)
    {
        _slots.RemoveItemSlot(ent, ent.Comp.HolderIdSlot);
    }

    private void OnPrintLicenseMessage(Entity<LicenseConsoleComponent> ent, ref PrintLicenseMessage args)
    {
        // No license to spawn
        if (string.IsNullOrWhiteSpace(ent.Comp.LicenseName))
            return;

        // If fail notify player of the error
        if (!TryGetIdCardName(ent, out var idCardName))
        {
            _audio.PlayPredicted(ent.Comp.FailSound, Transform(ent).Coordinates, null);
            _popup.PopupEntity(Loc.GetString(ent.Comp.FailPopup), ent, PopupType.MediumCaution);
            return;
        }

        var xform = Transform(ent);
        var licenceEnt = SpawnAtPosition(ent.Comp.LicenseName, xform.Coordinates);

        _xform.SetLocalRotation(licenceEnt, Angle.Zero);

        if (!TryComp<LicenseComponent>(licenceEnt, out var license))
            return;

        _audio.PlayPredicted(ent.Comp.SuccessSound, Transform(ent).Coordinates, null);
        _license.SetName((licenceEnt, license), idCardName);
    }

    public void SetName(Entity<LicenseComponent> ent, string? owner = null, string? licenseName = null)
    {
        if (owner != null)
            ent.Comp.OwnerName = owner;

        if (licenseName != null)
            ent.Comp.LicenseName = licenseName;

        if (ent.Comp.LicenseName is not { } license)
            return;

        var newItemName = ent.Comp.OwnerName != null
            ? Loc.GetString(ent.Comp.OwnerLoc, ("owner", ent.Comp.OwnerName), ("license", license))
            : Loc.GetString(ent.Comp.NoOwnerLoc, ("license", license));
        _meta.SetEntityName(ent, newItemName);
    }

    private bool TryGetIdCardName(
        Entity<LicenseConsoleComponent> ent,
        out string? name)
    {
        name = null;
        if (_slots.GetItemOrNull(ent, LicenseConsoleComponent.HolderIdSlotId) is not { } slotItem)
            return false;
        if (!TryComp<IdCardComponent>(slotItem, out var idCard))
            return false;
        name = idCard.FullName;

        return true;
    }
}
