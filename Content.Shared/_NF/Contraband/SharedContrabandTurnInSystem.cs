using System.Linq; // Aurora
using Content.Shared._AS.Contraband.ScuOutput; // Aurora
using Content.Shared._AS.License; // Aurora
using Content.Shared._NF.Contraband.Components; // Aurora
using Content.Shared.Access.Systems; // Aurora
using Content.Shared.Contraband;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory; // Aurora
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._NF.Contraband;

[NetSerializable, Serializable]
public enum ContrabandPalletConsoleUiKey : byte
{
    Contraband
}

public abstract class SharedContrabandTurnInSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!; // Aurora
    [Dependency] private readonly AccessReaderSystem _access = default!; // Aurora
    [Dependency] private readonly InventorySystem _inventory = default!; // Aurora
    [Dependency] private readonly SharedHandsSystem _hands = default!; // Aurora


    public override void Initialize() // Aurora
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerManagerComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }

    private void OnInteractionAttempt(Entity<ContainerManagerComponent> ent, ref InteractionAttemptEvent args) // Aurora
    {
        if (!TryComp<ContrabandPalletConsoleComponent>(args.Target, out var console))
            return;

        if (!CheckLicense(console, ent) && !CheckAccess(console, ent))
            args.Cancelled = true;
    }

    private bool CheckLicense(ContrabandPalletConsoleComponent console, Entity<ContainerManagerComponent> user) // Aurora
    {
        if (!_inventory.TryGetSlotEntity(user, "id", out var slotEnt))
            return false;
        if (TryComp<LicenseComponent>(slotEnt, out var license) && license.LicenseName == console.LicenseRequired)
            return true;
        if (!_container.TryGetContainer(slotEnt.Value, "PDA-license", out var container))
            return false;
        foreach (var containerEnt in container.ContainedEntities)
        {
            if (TryComp<LicenseComponent>(containerEnt, out license) && license.LicenseName == console.LicenseRequired)
                return true;
        }
        foreach (var heldEnt in _hands.EnumerateHeld(user))
        {
            if (TryComp<LicenseComponent>(heldEnt, out license) && license.LicenseName == console.LicenseRequired)
                return true;
        }
        return false;
    }

    private bool CheckAccess(ContrabandPalletConsoleComponent console, Entity<ContainerManagerComponent> user) // Aurora
    {
        if (_access.FindAccessTags(user) is not {} tags)
            return false;
        return tags.Contains(console.AccessRequired);
    }

    public void ClearContrabandValue(EntityUid item)
    {
        // Clear contraband value for printed items
        if (TryComp<ContrabandComponent>(item, out var contraband) && contraband.TurnInValues is {} turnInValues)
        {
            foreach (var valueKey in turnInValues.Keys)
            {
                contraband.TurnInValues[valueKey] = 0;
            }
        }

        // Recurse into contained entities
        if (TryComp<ContainerManagerComponent>(item, out var containers))
        {
            foreach (var container in containers.Containers.Values)
            {
                foreach (var ent in container.ContainedEntities)
                {
                    ClearContrabandValue(ent);
                }
            }
        }
    }
}
