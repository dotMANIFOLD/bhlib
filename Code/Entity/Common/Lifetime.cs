using Sandbox;

namespace MANIFOLD.BHLib.Components {
    [Title("Lifetime")]
    [Icon("hourglass_empty")]
    public class LifetimeDefinition : ComponentDefinition {
        public float Duration { get; set; }
        
        public override EntityComponent Create(GameObject obj) {
            var comp = obj.AddComponent<Lifetime>();
            comp.Data = this;
            return comp;
        }
    }
    
    /// <summary>
    /// Destroys the entity after a given time frame.
    /// </summary>
    [Category(LibraryData.CATEGORY + "/Common")]
    [Icon("hourglass_empty")]
    [Hide]
    public class Lifetime : EntityComponent {
        public LifetimeDefinition Data { get; set; }

        private float passed;
        
        public override void SimulateFrame(float deltaTime) {
            base.SimulateFrame(deltaTime);

            passed += deltaTime;

            if (passed > Data.Duration) {
                if (Scene.IsEditor) GameObject.Enabled = false;
                else GameObject.Destroy();
            }
        }

        public override void Simulate(float time) {
            passed = 0;
            base.Simulate(time);
        }
    }
}
