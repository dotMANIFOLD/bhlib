using Sandbox;

namespace MANIFOLD.BHLib.Events {
    public class SpawnEntity : SpawnEvent {
        [Order(1000)]
        public EntityData Data { get; set; }
    }
}
