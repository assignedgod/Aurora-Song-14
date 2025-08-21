using Robust.Shared.Serialization;

namespace Content.Shared._NF.Contraband.BUI;

[NetSerializable, Serializable]
public sealed class ContrabandPalletConsoleInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// estimated appraised value of all the contraband on top of pallets on the same grid as the console
    /// </summary>
    public int Appraisal;

    /// <summary>
    /// number of contraband entities on top of pallets on the same grid as the console
    /// </summary>
    public int Count;

    /// <summary>
    /// Aurora - number of items that can be translated into registered counterparts
    /// </summary>
    public int Unregistered;

    /// <summary>
    /// are the buttons enabled
    /// </summary>
    public bool Enabled;

    // Aurora - added unregistered
    public ContrabandPalletConsoleInterfaceState(int appraisal, int count, int unregistered, bool enabled)
    {
        Appraisal = appraisal;
        Count = count;
        Unregistered = unregistered;
        Enabled = enabled;
    }
}
