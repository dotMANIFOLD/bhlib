using Sandbox;

namespace MANIFOLD.BHLib {
    [Icon("category")]
    public abstract class ComponentDefinition {
        [Order(-1000)]
        public bool Enabled { get; set; } = true;
        
        public abstract EntityComponent Create(GameObject obj);
    }

    public abstract class HurtComponentDefinition : ComponentDefinition {
        public int Damage { get; set; }
        public bool DestroyOnHurt { get; set; } = true;
    }
}
