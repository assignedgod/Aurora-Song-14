using Robust.Shared.Configuration;

namespace Content.Shared._AS.CCVar;

[CVarDefs]
public sealed partial class AuroraCVars
{
    /// <summary>
    /// How often station staff wages are paid.
    /// </summary>
    public static readonly CVarDef<int> StationPayDelay =
        CVarDef.Create(
            "station_pay.delay",
            3600,
            CVar.SERVERONLY,
            "how often station staff wages are paid"
        );

    /// <summary>
    /// How long until suit sensors for dead players are automatically toggled on, following their death.
    /// </summary>
    public static readonly CVarDef<int> SuitSensorDeathActivationDelay =
        CVarDef.Create(
            "suit_sensors.death_activation_delay",
            600,
            CVar.SERVERONLY,
            "how long before dead player's suit sensors are toggled, in seconds"
        );

    /// <summary>
    /// Whether players should start with tasks or have to pull them from a task board.
    /// </summary>
    public static readonly CVarDef<bool> StartWithTasks =
        CVarDef.Create("task.start_with_tasks", true, CVar.REPLICATED | CVar.SERVER);
}
