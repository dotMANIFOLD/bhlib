using MANIFOLD.BHLib.Events;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Used to listen for <see cref="CasterEvent"/>s.
    /// </summary>
    public interface IEventListener {
        public void OnCasterEvent(CasterEvent evt);
    }
}
