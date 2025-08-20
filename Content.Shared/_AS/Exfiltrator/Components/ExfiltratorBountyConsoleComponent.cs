using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._AS.Exfiltrator.Components;

[RegisterComponent]
public sealed partial class ExfiltratorBountyConsoleComponent : Component
{
    /// <summary>
    /// The id of the label entity spawned by the print label button.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BountyLabelId = "PaperExfiltratorBountyManifest"; // TODO: make some paper
    /// <summary>
    /// The id of the label entity spawned by the print label button.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BountyCrateId = "CrateExfiltratorBounty"; // TODO: make some paper

    /// <summary>
    /// The time at which the console will be able to print a label again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextPrintTime = TimeSpan.Zero;

    /// <summary>
    /// The time between prints.
    /// </summary>
    [DataField]
    public TimeSpan PrintDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The sound made when printing occurs
    /// </summary>
    [DataField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    /// <summary>
    /// The sound made when bounty skipping is denied due to lacking access.
    /// </summary>
    [DataField]
    public SoundSpecifier SpawnChestSound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningbolt.ogg");

    /// <summary>
    /// The sound made when the bounty is skipped.
    /// </summary>
    [DataField]
    public SoundSpecifier SkipSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound made when bounty skipping is denied due to lacking access.
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_two.ogg");
}

[NetSerializable, Serializable]
public sealed class ExfiltratorBountyConsoleState : BoundUserInterfaceState
{
    public List<ExfiltratorBountyData> Bounties;
    public TimeSpan UntilNextSkip;

    public ExfiltratorBountyConsoleState(List<ExfiltratorBountyData> bounties, TimeSpan untilNextSkip)
    {
        Bounties = bounties;
        UntilNextSkip = untilNextSkip;
    }
}

//TODO: inherit this from the base message
[Serializable, NetSerializable]
public sealed class ExfiltratorBountyAcceptMessage : BoundUserInterfaceMessage
{
    public string BountyId;

    public ExfiltratorBountyAcceptMessage(string bountyId)
    {
        BountyId = bountyId;
    }
}

[Serializable, NetSerializable]
public sealed class ExfiltratorBountySkipMessage : BoundUserInterfaceMessage
{
    public string BountyId;

    public ExfiltratorBountySkipMessage(string bountyId)
    {
        BountyId = bountyId;
    }
}
