using Content.Client._AS.Exfiltrator.UI; //Replace with Exfiltrator equivalent
using Content.Shared._AS.Exfiltrator.Components; // Replace with Exfiltrator equivalent
using JetBrains.Annotations;

namespace Content.Client._AS.Exfiltrator.BUI;

[UsedImplicitly]
public sealed class ExfiltratorBountyConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ExfiltratorBountyMenu? _menu;

    public ExfiltratorBountyConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new();

        _menu.OnClose += Close;

        _menu.OnLabelButtonPressed += id =>
        {
            SendMessage(new ExfiltratorBountyAcceptMessage(id));
        };

        _menu.OnSkipButtonPressed += id =>
        {
            SendMessage(new ExfiltratorBountySkipMessage(id));
        };

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not ExfiltratorBountyConsoleState state)
            return;

        _menu?.UpdateEntries(state.Bounties, state.UntilNextSkip);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
