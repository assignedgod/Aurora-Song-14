using Content.Client._AS.Exfiltrator.UI;
using Content.Shared._AS.Exfiltrator.BUI;
using Content.Shared._AS.Exfiltrator.Components;
using Content.Shared._AS.Exfiltrator.Events;

namespace Content.Client._AS.Exfiltrator.BUI;

public sealed class ExfiltratorBountyRedemptionConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ExfiltratorBountyRedemptionMenu? _menu;
    [ViewVariables]
    private EntityUid uid;

    public ExfiltratorBountyRedemptionConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        if (EntMan.TryGetComponent<ExfiltratorBountyRedemptionConsoleComponent>(owner, out var console))
            uid = owner;
    }

    protected override void Open()
    {
        base.Open();

        _menu = new ExfiltratorBountyRedemptionMenu();
        _menu.SellRequested += OnSell;
        _menu.OnClose += Close;

        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _menu?.Dispose();
        }
    }

    private void OnSell()
    {
        SendMessage(new ExfiltratorBountyRedemptionMessage());
    }

    // TODO: remove this, nothing to update
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ExfiltratorBountyRedemptionConsoleInterfaceState palletState)
            return;
    }
}
