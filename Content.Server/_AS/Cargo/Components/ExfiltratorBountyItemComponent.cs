using Robust.Shared.Prototypes;

namespace Content.Server._AS.Cargo.Components;

// Component to identify an item as matching an exfiltrator bounty.
// Each item can match at most one bounty type.
[RegisterComponent]
public sealed partial class ExfiltratorBountyItemComponent : Component
{
    // The ID of the category to match.
    [IdDataField]
    public string ID;
}
