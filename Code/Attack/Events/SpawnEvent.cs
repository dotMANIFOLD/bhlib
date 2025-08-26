namespace MANIFOLD.BHLib.Events {
    public abstract class SpawnEvent : AttackEvent {
        public Vector3 Position { get; set; }
        public Angles Rotation { get; set; }
    }
}
