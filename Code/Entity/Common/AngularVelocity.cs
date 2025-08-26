using Sandbox;

namespace MANIFOLD.BHLib.Components {
    [Title("Angular Velocity")]
    [Category("Transform")]
    [Icon("360")]
    public class AngularVelocityDefiniton : ComponentDefinition {
        public Angles Velocity { get; set; }
        
        public override EntityComponent Create(GameObject obj) {
            var comp = obj.AddComponent<AngularVelocity>();
            comp.Data = this;
            return comp;
        }
    }
    
    /// <summary>
    /// Rotates the entity every frame.
    /// </summary>
    [Category(LibraryData.CATEGORY + "/Transform")]
    [Icon("360")]
    [Hide]
    public class AngularVelocity : EntityComponent {
        public AngularVelocityDefiniton Data { get; set; }

        public override void SimulateFrame(float deltaTime) {
            base.SimulateFrame(deltaTime);
            
            LocalRotation *= Data.Velocity * deltaTime;
        }
    }
}
