using System.ComponentModel.DataAnnotations;

namespace Content.Shared._AS.Licence;

[RegisterComponent]
public sealed partial class LicenceComponent : Component
{
    [DataField]
    public LocId NoOwnerLoc = "licence-name-no-owner";

    [DataField]
    public LocId OwnerLoc = "licence-name-owner";

    [DataField]
    public string? LicenceName;
}
