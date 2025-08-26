using System.Text.Json.Serialization;

namespace MANIFOLD.BHLib.Events {
    public abstract class CasterEvent : AttackEvent {
        [JsonIgnore]
        public abstract bool Blocking { get; }
    }
}
