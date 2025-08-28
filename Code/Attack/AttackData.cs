using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.BHLib.Events;
using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Stores all kinds of events for an attack.
    /// </summary>
    [AssetType(Name = "Attack", Category = LibraryData.CATEGORY, Extension = "bhatk")]
    public class AttackData : GameResource {
        public class RendererData {
            public GameObject Prefab { get; set; }
            public int UseCount { get; set; }
        }
        
        [ReadOnly]
        public float CalculatedDuration { get; set; }
        public float? ProvidedDuration { get; set; }
        [JsonIgnore, Hide]
        public float Duration => ProvidedDuration ?? CalculatedDuration;
        
        // Adding a new timeline is simple.
        // Just add a new property that uses or inherits the Timeline<> class.
        // They can also technically implement ITimeline but that would leave it incompatible with how other things are done.
        // Such as sampling.
        
        public Timeline<SpawnEvent> SpawnTimeline { get; set; } = new Timeline<SpawnEvent>();
        public Timeline<PatternEvent> PatternTimeline { get; set; } = new Timeline<PatternEvent>();
        public Timeline<CasterEvent> CasterTimeline { get; set; } = new Timeline<CasterEvent>();
        
        /// <summary>
        /// Automatically populated during each save. It contains statistics for each renderer used.
        /// </summary>
        public List<RendererData> RenderPoolingData { get; set; } = new List<RendererData>();

        protected override Bitmap CreateAssetTypeIcon(int width, int height) {
            return CreateSimpleAssetTypeIcon("stream", width, height, LibraryData.BG_COLOR);
        }
    }
}
