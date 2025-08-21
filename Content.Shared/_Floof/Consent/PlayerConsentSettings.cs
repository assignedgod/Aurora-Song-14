using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Floofstation; // Flooftier
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared._Floof.Consent;

[Serializable, NetSerializable]
public sealed class PlayerConsentSettings
{
    public string Freetext;
    public Dictionary<ProtoId<ConsentTogglePrototype>, string> Toggles;

    public PlayerConsentSettings()
    {
        Freetext = string.Empty;
        Toggles = new();
    }

    public PlayerConsentSettings(
        string freetext,
        Dictionary<ProtoId<ConsentTogglePrototype>, string> toggles)
    {
        Freetext = freetext;
        Toggles = toggles;
    }

    public void EnsureValid(IConfigurationManager configManager,
        IPrototypeManager prototypeManager,
        HashSet<ConsentTogglePrototype> togglesPrototypes)
    {
        var maxLength = configManager.GetCVar(CCVars.ConsentFreetextMaxLength); // Flooftier
        Freetext = Freetext.Trim();
        if (Freetext.Length > maxLength)
            Freetext = Freetext.Substring(0, maxLength);

        Toggles = GetValidToggles(Toggles, prototypeManager, togglesPrototypes);
    }

    private Dictionary<ProtoId<ConsentTogglePrototype>, string> GetValidToggles(
    Dictionary<ProtoId<ConsentTogglePrototype>, string> toggles,
        IPrototypeManager prototypeManager,
        HashSet<ConsentTogglePrototype> togglesPrototypes
    )
    {
        var result = new Dictionary<ProtoId<ConsentTogglePrototype>, string>();

        foreach (var toggle in toggles)
        {
            var proto = togglesPrototypes
                .FirstOrDefault(proto => proto.ID == toggle.Key);

            if (proto == null)
                continue;

            var defaultValue = proto.DefaultValue ? "on" : "off";

            if (defaultValue == toggle.Value)
                continue;

            result.Add(proto, toggle.Value);
        }

        return result;
    }
}
