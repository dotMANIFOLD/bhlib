using System;
using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// A component used by an entity.
    /// </summary>
    public abstract class EntityComponent : Component {
        [Property]
        public GameObject Owner { get; set; }
        [Property]
        public ITarget Target { get; set; }
        
        protected override void OnFixedUpdate() {
            SimulateFrame(Time.Delta);
        }

        /// <summary>
        /// Simulate a single frame.
        /// </summary>
        public virtual void SimulateFrame(float deltaTime) {
            
        }

        /// <summary>
        /// Simulate the full behavior in one call. Typically used in previews.
        /// </summary>
        public virtual void Simulate(float time) {
            SimulateFrame(time);
        }
        
        protected virtual bool CheckForTarget(SceneTraceResult result) {
            if (!result.Hit) return false;
            if (result.GameObject == Target.GameObject) return true;
            if (result.GameObject.GetComponentInParent<ITarget>() == Target) return true;
            return false;
        }
    }
}
