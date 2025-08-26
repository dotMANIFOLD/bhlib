using Sandbox;

namespace MANIFOLD.BHLib.Components {
    [Title("Renderer")]
    [Icon("visibility")]
    public class RendererDefinition : ComponentDefinition {
        public GameObject Prefab { get; set; }
        
        public override EntityComponent Create(GameObject obj) {
            var comp = obj.AddComponent<Renderer>();
            comp.Data = this;
            return comp;
        }
    }
    
    public class Renderer : EntityComponent {
        public RendererDefinition Data { get; set; }
        
        protected override void OnStart() {
            base.OnStart();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
        }
    }
}
