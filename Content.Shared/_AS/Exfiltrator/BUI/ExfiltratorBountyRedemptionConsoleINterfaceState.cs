using Robust.Shared.Serialization;

namespace Content.Shared._AS.Exfiltrator.BUI;

[NetSerializable, Serializable]
public sealed class ExfiltratorBountyRedemptionConsoleInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// Whether or not the sale was successful.
    /// </summary>
    public bool Success;

    /// <summary>
    /// A message to print out onto the console
    /// </summary>
    public string Message;

    public ExfiltratorBountyRedemptionConsoleInterfaceState(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
