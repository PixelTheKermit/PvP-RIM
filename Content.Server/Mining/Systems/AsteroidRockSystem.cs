using System.Collections.Generic;
using Content.Server.DoAfter;
using Content.Server.Mining.Components;
using Content.Server.Weapon.Melee.Components;
using Content.Shared.Acts;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Mining.Systems;

public class AsteroidRockSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ActSystem _actSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MineableComponent, DestructionEventArgs>(OnAsteroidRockDestruction);
    }

    private void OnAsteroidRockDestruction(EntityUid uid, MineableComponent component, DestructionEventArgs args)
    {
        if (!_random.Prob(component.OreChance))
            return; // Nothing to do.

        HashSet<string> spawnedGroups = new();
        foreach (var entry in component.OreTable)
        {
            if (entry.GroupId is not null && spawnedGroups.Contains(entry.GroupId))
                continue;

            if (!_random.Prob(entry.SpawnProbability))
                continue;

            for (var i = 0; i < entry.Amount; i++)
            {
                Spawn(entry.PrototypeId, Transform(uid).Coordinates);
            }

            if (entry.GroupId != null)
                spawnedGroups.Add(entry.GroupId);
        }
    }
}
