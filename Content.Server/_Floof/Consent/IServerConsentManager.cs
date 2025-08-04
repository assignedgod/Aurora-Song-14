using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Floof.Consent;
using Robust.Shared.Network;
using Robust.Shared.Player;


namespace Content.Server._Floof.Consent;

public interface IServerConsentManager
{
    void Initialize();

    Task LoadData(ICommonSession session, CancellationToken cancel);
    void OnClientDisconnected(ICommonSession session);

    /// <summary>
    /// Get player consent settings
    /// </summary>
    PlayerConsentSettings GetPlayerConsentSettings(NetUserId userId);
}
