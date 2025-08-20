using Robust.Shared.Serialization;

namespace Content.Shared._AS.Exfiltrator;

[NetSerializable, Serializable]
public enum ExfiltratorConsoleUiKey : byte
{
    Bounty,
    BountyRedemption
}

[NetSerializable, Serializable]
public enum ExfiltratorPalletConsoleUiKey : byte
{
    Sale
}

public abstract class SharedExfiltratorSystem : EntitySystem {}
