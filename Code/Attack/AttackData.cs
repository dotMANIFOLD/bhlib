using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.BHLib.Events;
using Sandbox;

namespace MANIFOLD.BHLib {
    [GameResource("Attack", "bhatk", "BH Attack Sequence", Category = LibraryData.CATEGORY, Icon = "stream", IconBgColor = LibraryData.BG_COLOR)]
    public class AttackData : GameResource {
        public enum CasterOrientMode { Inherit, FacePlayer, Predefined }
        
        [ReadOnly]
        public float CalculatedDuration { get; set; }
        public float? ProvidedDuration { get; set; }
        [JsonIgnore]
        public float Duration => ProvidedDuration ?? CalculatedDuration;
        
        public Timeline<SpawnEvent> SpawnTimeline { get; set; } = new Timeline<SpawnEvent>();
        public Timeline<PatternEvent> PatternTimeline { get; set; } = new Timeline<PatternEvent>();
    }
}
