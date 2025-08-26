using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Used to let entities know what they're trying to hit.
    /// Designed to be used on a component.
    /// </summary>
    public interface ITarget : IValid {
        public Transform WorldTransform { get; }
        public Vector3 WorldPosition { get; }
        public Rotation WorldRotation { get; }

        public void Hurt(int damage);
    }
}
