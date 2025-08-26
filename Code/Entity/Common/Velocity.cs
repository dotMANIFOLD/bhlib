using Sandbox;

namespace MANIFOLD.BHLib.Components {
    /// <summary>
    /// Move without collision checks.
    /// </summary>
    [Title("Velocity")]
    [Category("Transform")]
    [Icon("speed")]
    public class VelocityDefinition : ComponentDefinition {
        public bool Global { get; set; }
        public Vector3 Velocity { get; set; }
        
        public override EntityComponent Create(GameObject obj) {
            var comp = obj.AddComponent<Velocity>();
            comp.Data = this;
            return comp;
        }
    }
    
    /// <summary>
    /// Moves an entity every frame.
    /// </summary>
    [Category(LibraryData.CATEGORY + "/Transform")]
    [Icon("speed")]
    [Hide]
    public class Velocity : EntityComponent {
        public VelocityDefinition Data { get; set; }

        public override void SimulateFrame(float deltaTime) {
            var vec = Data.Global ? Data.Velocity : WorldRotation * Data.Velocity;
            WorldPosition += vec * deltaTime;
        }
    }
}
