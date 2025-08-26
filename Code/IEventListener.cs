using MANIFOLD.BHLib.Events;

namespace MANIFOLD.BHLib {
    public interface IEventListener {
        public void OnCasterEvent(CasterEvent evt);
    }
}
