using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Content.Shared._NF.SectorServices.Prototypes;
using Content.Server.GameTicking;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Administration.Logs.Converters;


namespace Content.Server._AS.SectorServices;

/// <summary>
/// System that manages sector-wide services.
/// Allows service components to be registered and unregistered on a singular entity
/// </summary>
[PublicAPI]
public sealed class SectorServiceSystem : EntitySystem
{
    [Robust.Shared.IoC.Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Robust.Shared.IoC.Dependency] private readonly IEntityManager _entityManager = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    private EntityUid _entity = EntityUid.Invalid; // The station entity that's storing our services.

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationSectorServiceHostComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<StationSectorServiceHostComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(EntityUid uid, StationSectorServiceHostComponent component, ComponentStartup args)
    {
        Log.Error($"OnComponentStartup! Entity: {uid} internal: {_entity}");
        if (_entity == EntityUid.Invalid)
        {
            _entity = uid;

            foreach (var servicePrototype in _prototypeManager.EnumeratePrototypes<SectorServicePrototype>())
            {
                Log.Error($"Adding component: {servicePrototype.Components}");
                _entityManager.AddComponents(_entity,
                    servicePrototype.Components,
                    false); // removeExisting false - do not override existing components.
            }
        }
    }

    private void OnComponentShutdown(EntityUid uid, StationSectorServiceHostComponent component, ComponentShutdown args)
    {
        Log.Error($"OnComponentShutdown! Entity: {_entity}");
        if (_entity != EntityUid.Invalid)
        {
            foreach (var servicePrototype in _prototypeManager.EnumeratePrototypes<SectorServicePrototype>())
            {
                Log.Error($"Removing component: {servicePrototype.Components}");
                _entityManager.RemoveComponents(_entity, servicePrototype.Components);
            }

            _entity = EntityUid.Invalid;
        }
    }

    public EntityUid GetServiceEntity()
    {
        return _entity;
    }
}
