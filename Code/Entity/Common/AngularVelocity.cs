using Sandbox;

namespace MANIFOLD.BHLib.Components {
    [Title("Angular Velocity")]
    [Category("Transform")]
    [Icon("360")]
    public class AngularVelocityDefiniton : ComponentDefinition {
        public Angles Velocity { get; set; }
        
        public override Component Create(GameObject obj) {
            var comp = obj.AddComponent<AngularVelocity>();
            comp.Data = this;
            return comp;
        }
    }
    
    public class AngularVelocity : EntityComponent {
        public AngularVelocityDefiniton Data { get; set; }

        public override void SimulateFrame(float deltaTime) {
            base.SimulateFrame(deltaTime);
            
            LocalRotation *= Data.Velocity * deltaTime;
        }
    }
}
