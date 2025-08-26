using Sandbox;

namespace MANIFOLD.BHLib {
    public interface ITarget : IValid {
        public Transform WorldTransform { get; }
        public Vector3 WorldPosition { get; }
        public Rotation WorldRotation { get; }

        public void Hurt(int damage);
    }
}
