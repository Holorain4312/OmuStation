using System.Numerics;
using Content.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Server.Teleportation;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Actions;
using Content.Omu.Server.Darksci.Components;
using Content.Omu.Shared.Darksci.Components;
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

        // ensures originLoc is NEVER empty. this is required so the code doesnt throw errors.
        if(component.originLoc == null) {component.originLoc = Transform(uid).Coordinates;}

        //this could probably be moved into the teleport logic however i dont want to touch it in fear of everything breaking
        if(!component.inDark)
        {
            component.originLoc = Transform(uid).Coordinates;
        }

        var darkLoc = Transform(darkUid.Value).Coordinates;

        var newCoords = component.originLoc.Value;

        if(!component.inDark){
            var xform = Transform(darkUid.Value);
            var coords = xform.Coordinates;
            var MaxRandomRadius = 32;
            //var MaxRandomTeleportAttempts = 10;
            // Sets a random very far coordinate for it to attempt off of, main idea being you appear in a random part of darkspace
            var randPos = coords.Offset(_random.NextVector2(MaxRandomRadius));

            newCoords = randPos;



            // this is the evil sphagetti shitcode function of doom. In essence what we are doing here is checking a load of random positions to make sure they arent inside a wall,
            // and if they are we see if we can offset it to make it not inside a wall. Not a fan of this solution but i want to avoid teleporting people into a wall if i can help it.

            // commented out for the time being as it is not working.
            // TODO: fix this shit.

            // bool handled = false;
            // for (var i = 0; i < MaxRandomTeleportAttempts; i++)
            // {
            //     if (handled != true) {
            //         randPos = coords.Offset(_random.NextVector2(MaxRandomRadius));

            //         newCoords = randPos.Offset(new Vector2(0.5f, 0.5f));



            //         var lookup = _lookup.GetEntitiesIntersecting(newCoords);
            //         if (lookup.Count == 0)
            //         {
            //             handled = true;
            //             break;
            //         }
            //         for (var j = 0; j < MaxRandomTeleportAttempts; j++)
            //         {


            //             var randVector = new Vector2(1,1);
            //             newCoords = newCoords.Offset(randVector);
            //             lookup = _lookup.GetEntitiesIntersecting(newCoords);
            //             if (lookup.Count == 0)
            //             {
            //                 handled = true;
            //                 break;
            //             }
            //         }

            //     }
            // }





            //bandaid fix for coordinates

            _transform.SetCoordinates(uid, newCoords);

            component.inDark = true;
        }
        else
        {
            component.inDark = false;

            _transform.SetCoordinates(uid, component.originLoc.Value);
        }
    }
}
