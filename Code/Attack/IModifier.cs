namespace MANIFOLD.BHLib {
    /// <summary>
    /// Interface that lets an event modify other parts of an attack.
    /// </summary>
    public interface IModifier {
        public void OnAdd(AttackData data);
        public void OnRemove(AttackData data);
        public void Modify(AttackData data);
    }
}
