using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.Gateway.Components;
using Content.Server.Gateway.Systems;
using Content.Shared.Salvage;
using System.Linq;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Shared.CCVar;
using Content.Shared.Dataset;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.EntitySerialization;

namespace Content.Omu.Server.Darksci;

public sealed class DarkPlanetSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly StationSpawningSystem _spawningSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly GatewaySystem _gateway = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    //currently empty map that just adds the planet. useful to do it this way in case we decide to add prebuilt structures later. (shadowkin fortress when? :P)
    const string MapPath = "Maps/_Omu/Nonstations/darkoutpost.yml";

    //Works this way cause i want the place to be loaded in roundstart instead of something triggering it getting loaded, this way it's somewhat persistent and you can darkswap there roundstart
    private void OnRoundStart(RoundStartingEvent ev)
    {
        var resPath = new ResPath(MapPath);

        //Yes there is probably a better way to load maps and shit, no i haven't found it yet
        if (_mapLoader.TryLoadMap(resPath, out var map, out _, new DeserializationOptions { InitializeMaps = true }))
        {


            var entityUid = map.Value.Owner;
            var mapId = map.Value.Comp.MapId;

            var restricted = new RestrictedRangeComponent
            {
                Range = 64f
            };
            AddComp(entityUid, restricted);

            //_metadata.SetEntityName(entityUid,"The Dark");
            _mapSystem.SetPaused(mapId, false);

            //Alternate solution to come back to later
            //GenerateDestination();
        }
    }

    private void GenerateDestination(GatewayGeneratorComponent? generator = null)
    {
        //TODO: Figure out how to use the shit on the line below
        //EntityUid uid,

        //if (!Resolve(uid, ref generator))
        //return;

        var tileDef = _tileDefManager["FloorSteel"];
        const int MaxOffset = 256;
        var tiles = new List<(Vector2i Index, Tile Tile)>();
        var seed = _random.Next();
        var random = new Random(seed);
        var mapId = _mapSystem.CreateMap(true);
        var mapUid = mapId;

        var gatewayName = "The Dark";
        _metadata.SetEntityName(mapUid, gatewayName);

        var origin = new Vector2i(random.Next(-MaxOffset, MaxOffset), random.Next(-MaxOffset, MaxOffset));
        var restricted = new RestrictedRangeComponent
        {
            Origin = origin
        };
        AddComp(mapUid, restricted);

        _biome.EnsurePlanet(mapUid, _protoManager.Index<BiomeTemplatePrototype>("Dark"), seed);

        var grid = Comp<MapGridComponent>(mapUid);

        for (var x = -2; x <= 2; x++)
        {
            for (var y = -2; y <= 2; y++)
            {
                tiles.Add((new Vector2i(x, y) + origin, new Tile(tileDef.TileId, variant: _tile.PickVariant((ContentTileDefinition) tileDef, random))));
            }
        }

        // Clear area nearby as a sort of landing pad.
        _mapSystem.SetTiles(mapUid, grid, tiles);

        _metadata.SetEntityName(mapUid, gatewayName);
        var originCoords = new EntityCoordinates(mapUid, origin);

        //Commenting out for the time being till I figure out a solution here
        //var genDest = AddComp<GatewayGeneratorDestinationComponent>(mapUid);
        //genDest.Origin = origin;
        //genDest.Seed = seed;
        //genDest.Generator = uid;

        // Create the gateway.
        //Commented out for the time being
        //var gatewayUid = SpawnAtPosition(generator.Proto, originCoords);
        //var gatewayComp = Comp<GatewayComponent>(gatewayUid);
        //_gateway.SetDestinationName(gatewayUid, FormattedMessage.FromMarkupOrThrow($"[color=#D381C996]{gatewayName}[/color]"), gatewayComp);
        //_gateway.SetEnabled(gatewayUid, true, gatewayComp);
        //generator.Generated.Add(mapUid);
    }
}
