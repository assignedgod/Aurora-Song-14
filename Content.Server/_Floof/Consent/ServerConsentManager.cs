using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._Floof.Consent;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;


namespace Content.Server._Floof.Consent;

public sealed class ServerConsentManager : IServerConsentManager
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private readonly HashSet<ConsentTogglePrototype> _consentTogglePrototypes = new();
    private ISawmill? _sawmill = null;

    public void Initialize()
    {
        _sawmill = _logManager.GetSawmill("serverconsent");
        _netManager.RegisterNetMessage<MsgUpdateConsent>(HandleUpdateConsentMessage);

        _prototypeManager.PrototypesReloaded += _ => GenerateConsentTogglePrototypes();
    }

    private void GenerateConsentTogglePrototypes()
    {
        _consentTogglePrototypes.Clear();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<ConsentTogglePrototype>())
            _consentTogglePrototypes.Add(proto);
    }

    private async void HandleUpdateConsentMessage(MsgUpdateConsent message)
    {
        var userId = message.MsgChannel.UserId;
        var consentSystem = _entityManager.System<ConsentSystem>();

        if (!consentSystem.TryGetConsent(userId, out _))
            return;

        message.Consent.EnsureValid(_configManager, _prototypeManager, _consentTogglePrototypes);
        consentSystem.SetConsent(userId, message.Consent);

        var session = _playerManager.GetSessionByChannel(message.MsgChannel);
        var togglesPretty = String.Join(", ", message.Consent.Toggles.Select(t => $"[{t.Key}: {t.Value}]"));
        _adminLogger.Add(LogType.Consent, LogImpact.Medium,
            $"{session:Player} updated consent setting to: '{message.Consent.Freetext}' with toggles {togglesPretty}");

        if (ShouldStoreInDb(message.MsgChannel.AuthType))
            await _db.SavePlayerConsentSettingsAsync(userId, message.Consent);

        // send it back to confirm to client that consent was updated
        _netManager.ServerSendMessage(message, message.MsgChannel);
    }

    public async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var consent = new PlayerConsentSettings();
        var consentSystem = _entityManager.System<ConsentSystem>();

        if (_consentTogglePrototypes.Count == 0)
            GenerateConsentTogglePrototypes();

        if (ShouldStoreInDb(session.AuthType))
            consent = await _db.GetPlayerConsentSettingsAsync(session.UserId);

        consent.EnsureValid(_configManager, _prototypeManager, _consentTogglePrototypes);
        consentSystem.SetConsent(session.UserId, consent);

        var message = new MsgUpdateConsent() { Consent = consent };
        _netManager.ServerSendMessage(message, session.Channel);
    }

    public void OnClientDisconnected(ICommonSession session)
    {
        var consentSystem = _entityManager.System<ConsentSystem>();
        consentSystem.SetConsent(session.UserId, null);
    }

    /// <inheritdoc />
    public PlayerConsentSettings GetPlayerConsentSettings(NetUserId userId)
    {
        var consentSystem = _entityManager.System<ConsentSystem>();
        if (consentSystem.TryGetConsent(userId, out var consent))
            return consent;

        // A player that has disconnected does not consent to anything.
        return new();
    }

    private static bool ShouldStoreInDb(LoginType loginType)
    {
        return loginType.HasStaticUserId();
    }
}
