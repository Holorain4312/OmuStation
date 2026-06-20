using System.Numerics;
using Content.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Server.Teleportation;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Actions;
using Content.Omu.Server.Darksci.Components;
using Robust.Shared.Map;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Random;
using Robust.Shared.Physics;
using Content.Shared.Coordinates;


namespace Content.Omu.Server.Darksci;

public sealed class DarkswapSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    //[Dependency] private readonly TeleportSystem _teleportSys = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    EntityCoordinates originLoc;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkswapComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DarkswapComponent, DarkswapEvent>(OnDarkswap);
    }

    private void OnInit(EntityUid uid, DarkswapComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnDarkswap(EntityUid uid, DarkswapComponent component, ref DarkswapEvent args)
    {
        if (TryComp<PullableComponent>(uid, out var pullable) && _pullingSystem.IsPulled(uid, pullable))
            return;

        var query = EntityQueryEnumerator<DarkComponent>();

        EntityUid? darkUid = null;

        while (query.MoveNext(out var targetUid))
        {
            //Finds the place which has the Component "DarkComponent". Only searches for one because there should ideally be only one object with this tag, if there are more then there is probably other issues as well.
            darkUid = targetUid.Owner;
            break;
        }

        if (darkUid == null)
        {
            return;
        }


        if(!component.inDark){
            originLoc = Transform(uid).Coordinates;
        }

        var darkLoc = Transform(darkUid.Value).Coordinates;

        var newCoords = originLoc;

        if(!component.inDark){
            var xform = Transform(darkUid.Value);
            var coords = xform.Coordinates;
            var MaxRandomRadius = 30;
            var MaxRandomTeleportAttempts = 10;
            //Sets a random coordinate for it to attempt off of, main idea being you appear in a random part of darkspace
            var randPos = coords.Offset(_random.NextVector2(MaxRandomRadius));
            //
            newCoords = randPos;
            for (var i = 0; i < MaxRandomTeleportAttempts; i++)
            {
                //should work the vast majority of cases, because this is teleporting you to a planet map, we don't need to worry about throwing them into space and the right position isn't quite as important
                var randVector = new Vector2(1,1);
                newCoords = randPos.Offset(randVector);
                if (!_lookup.AnyEntitiesIntersecting(_transform.ToMapCoordinates(newCoords), LookupFlags.Static))
                {
                    break;
                }
            }
            _transform.SetCoordinates(uid, newCoords);

            component.inDark = true;
        }
        else
        {
            component.inDark = false;

            _transform.SetCoordinates(uid, originLoc);
        }
    }
}
