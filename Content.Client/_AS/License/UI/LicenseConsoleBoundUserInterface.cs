

using Content.Shared._AS.License.Events;
using Robust.Client.UserInterface;

namespace Content.Client._AS.License.UI;

public sealed class LicenseConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private LicenseConsoleMenu? _menu;

    public LicenseConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<LicenseConsoleMenu>();
        _menu.PrintLicense += OnPrint;
    }

    private void OnPrint()
    {
        SendMessage(new PrintLicenseMessage());
    }
}
