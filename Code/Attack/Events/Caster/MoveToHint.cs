using System.Text.Json.Serialization;

namespace MANIFOLD.BHLib.Events {
    public class MoveToHint : CasterEvent {
        public string Hint { get; set; }
        
        [JsonIgnore]
        public override bool Blocking => false;
    }
}
