using System;
using Sandbox;

namespace MANIFOLD.BHLib {
    public abstract class EntityComponent : Component {
        [Property]
        public GameObject Owner { get; set; }
        [Property]
        public ITarget Target { get; set; }
        
        protected override void OnFixedUpdate() {
            SimulateFrame(Time.Delta);
        }

        public virtual void SimulateFrame(float deltaTime) {
            
        }

        public virtual void Simulate(float time) {
            SimulateFrame(time);
        }
        
        protected virtual void ProcessTrace(SceneTraceResult result) {
            if (result.Hit) {
                if (result.GameObject.Tags.Has("player")) {
                    Log.Info("player should be hurt");
                }
            }
        }
    }
}
