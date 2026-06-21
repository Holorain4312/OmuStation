using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Physics;

namespace  Content.Omu.Shared.Darksci.Components;

[RegisterComponent]
public sealed partial class DarkswapComponent : Component
{
    [DataField("combatToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ActionProto = "DarkswapAction";

    [DataField]
    public EntityUid? ActionUid;

    [DataField]
    public EntityCoordinates? originLoc;

    //are they currently in the dark?
    [DataField]
    public bool inDark = false;
}

public sealed partial class DarkswapEvent : InstantActionEvent;
