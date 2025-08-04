using System.ComponentModel.DataAnnotations;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.License.Components;

[RegisterComponent]
public sealed partial class LicenseConsoleComponent : Component
{
    public static string HolderIdSlotId = "LicenseConsole-holderId";

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/Machines/buzz-sigh.ogg");

    [DataField]
    public string FailPopup = "license-console-fail-no-id";

    [DataField]
    public SoundSpecifier SuccessSound = new SoundPathSpecifier("/Audio/Machines/scan_loop.ogg");

    [DataField]
    public EntProtoId? LicenseName;

    [DataField]
    public ItemSlot HolderIdSlot = new();

    [Serializable, NetSerializable]
    public sealed class WriteToLicenseMessage : BoundUserInterfaceMessage;

    [Serializable, NetSerializable]
    public sealed class LicenseConsoleBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string LicenseName;

        public LicenseConsoleBoundUserInterfaceState(string licenseName)
        {
            LicenseName = licenseName;
        }
    }

    [Serializable, NetSerializable]
    public enum LicenseConsoleUiKey : byte
    {
        Key,
    }
}
