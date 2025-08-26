using System.Text.Json.Serialization;

namespace MANIFOLD.BHLib.Events {
    public class SetLookMode : CasterEvent {
        public enum LookMode { Velocity, Target, Predefined }
        
        public LookMode Mode { get; set; }
        [ShowIf(nameof(Mode), LookMode.Predefined)]
        public Angles Angles { get; set; }

        [JsonIgnore]
        public override bool Blocking => false;
    }
}
