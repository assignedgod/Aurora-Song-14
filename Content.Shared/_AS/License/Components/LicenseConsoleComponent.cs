using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.License.Components;

[RegisterComponent]
public sealed partial class LicenseConsoleComponent : Component
{
    public const int MaxFullNameLength = 30;

    [Serializable, NetSerializable]
    public sealed class WriteToLicenseMessage : BoundUserInterfaceMessage
    {
        public readonly string FullName;

        public WriteToLicenseMessage(string fullName)
        {
            FullName = fullName;
        }
    }

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
