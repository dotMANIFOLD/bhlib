namespace MANIFOLD.BHLib {
    public interface IModifier {
        public void OnAdd(AttackData data);
        public void OnRemove(AttackData data);
        public void Modify(AttackData data);
    }
}
