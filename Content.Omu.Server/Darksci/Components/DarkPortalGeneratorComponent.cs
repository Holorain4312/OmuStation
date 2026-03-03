using Robust.Shared.Prototypes;

namespace Content.Omu.Server.Darksci.Components
{
    [RegisterComponent]
    public sealed partial class DarkPortalGeneratorComponent : Component
    {
        /// <summary>
        /// Prototype to spawn on the generated map if applicable.
        /// </summary>
        [DataField]
        public EntProtoId? Proto1 = "DarkPortal";

        public EntProtoId? Proto2 = "DarkPortal";
    }
}
