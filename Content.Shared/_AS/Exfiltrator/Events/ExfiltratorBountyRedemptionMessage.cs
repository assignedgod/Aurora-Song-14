﻿using Robust.Shared.Serialization;

namespace Content.Shared._AS.Exfiltrator.Events;

/// <summary>
/// Raised on a client request pallet sale
/// </summary>
[Serializable, NetSerializable]
public sealed class ExfiltratorBountyRedemptionMessage : BoundUserInterfaceMessage
{
    public ExfiltratorBountyRedemptionMessage()
    {
    }
}
