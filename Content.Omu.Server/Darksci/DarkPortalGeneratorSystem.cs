using System.Linq;
using Content.Server.Gateway.Components;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Shared.CCVar;
using Content.Shared.Dataset;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Content.Shared.Salvage;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Interaction;
using Content.Shared.Actions;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Omu.Server.Darksci.Components;
using Content.Server.Teleportation;

public sealed partial class DarkPortalGeneratorSystem : EntitySystem
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly TileSystem _tile = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkPortalGeneratorComponent, ActivateInWorldEvent>(GeneratePortal);
    }

    private void GeneratePortal(EntityUid uid, DarkPortalGeneratorComponent? generator, ref ActivateInWorldEvent args)
    {
        var currentLoc = Transform(uid).Coordinates;

        var seed = _random.Next();
        var random = new Random(seed);
        const int MaxOffset = 256;

        var tileDef = _tileDefManager["FloorChromite"];
        var tiles = new List<(Vector2i Index, Tile Tile)>();

        if (!Resolve(uid, ref generator))
            return;

        EntityUid? darkUid = null;

        var query = EntityQueryEnumerator<DarkComponent>();

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

        var origin = new Vector2i(random.Next(-MaxOffset, MaxOffset), random.Next(-MaxOffset, MaxOffset));
        var originCoords = new EntityCoordinates(darkUid.Value, origin);

        var entryUid = SpawnAtPosition(generator.Proto1, currentLoc);

        var grid = Comp<MapGridComponent>(darkUid.Value);

        for (var x = -2; x <= 2; x++)
        {
            for (var y = -2; y <= 2; y++)
            {
                tiles.Add((new Vector2i(x, y) + origin, new Tile(tileDef.TileId, variant: _tile.PickVariant((ContentTileDefinition) tileDef, random))));
            }
        }

        // Clear area nearby as a sort of landing pad.
        _maps.SetTiles(darkUid.Value, grid, tiles);

        // Create the gateway.
        var gatewayUid = SpawnAtPosition(generator.Proto2, originCoords);

        _link.TryLink(entryUid, gatewayUid);


    }
}
