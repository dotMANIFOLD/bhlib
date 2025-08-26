using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Data structures to create an <see cref="EntityComponent"/>
    /// </summary>
    [Icon("category")]
    public abstract class ComponentDefinition {
        [Order(-1000)]
        public bool Enabled { get; set; } = true;
        
        public abstract EntityComponent Create(GameObject obj);
    }

    /// <summary>
    /// Variant used by hurting components.
    /// </summary>
    public abstract class HurtComponentDefinition : ComponentDefinition {
        public int Damage { get; set; }
        public bool DestroyOnHurt { get; set; } = true;
    }
}
