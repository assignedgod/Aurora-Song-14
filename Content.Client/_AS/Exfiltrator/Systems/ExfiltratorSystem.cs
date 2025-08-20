using Content.Shared._AS.Exfiltrator;
using Robust.Client.GameObjects;

namespace Content.Client._AS.Exfiltrator.Systems;

public sealed partial class ExfiltratorSystem : SharedExfiltratorSystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
