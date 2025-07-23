using Robust.Shared.Serialization;

namespace Content.Shared._AS.Contraband.Events;

/// <summary>
/// Raised on a client request pallet sale
/// </summary>
[Serializable, NetSerializable]
public sealed class ContrabandPalletRegisterMessage : BoundUserInterfaceMessage
{

}
