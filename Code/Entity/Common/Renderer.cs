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
    
    /// <summary>
    /// Spawns a renderer for an entity. Pulls from the <see cref="RendererPool"/>.
    /// </summary>
    [Category(LibraryData.CATEGORY)]
    [Icon("visibility")]
    [Hide]
    public class Renderer : EntityComponent {
        public RendererDefinition Data { get; set; }

        private GameObject obj;

        public GameObject Held => obj;
        
        protected override void OnStart() {
            obj = RendererPool.Request(Data.Prefab);
        }
        
        protected override void OnDestroy() {
            RendererPool.Release(obj);
        }

        protected override void OnPreRender() {
            obj.WorldTransform = WorldTransform;
        }
    }
}
