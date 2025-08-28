using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Stores a list of <see cref="EntityComponent"/>s.
    /// </summary>
    [AssetType(Name = "Entity Data", Category = LibraryData.CATEGORY, Extension = "bhent")]
    public class EntityData : GameResource {
        public const string TYPE_FIELD = "__type";
        
        [JsonIgnore]
        public List<ComponentDefinition> Components { get; set; } = new();
        
        [Hide]
        public JsonArray SerializedComponents {
            get {
                JsonArray arr = new JsonArray();
                foreach (var module in Components) {
                    var jsonNode = Json.ToNode(module);
                    jsonNode[TYPE_FIELD] = Json.ToNode(module.GetType(), typeof(Type));
                    arr.Add(jsonNode);
                }
                return arr;
            }
            set {
                Components.Clear();
                foreach (var node in value) {
                    var type = Json.FromNode<Type>(node[TYPE_FIELD]);
                    var deserialized = (ComponentDefinition)Json.Deserialize(node.ToString(), type);
                    Components.Add(deserialized);
                }
            }
        }

        protected override Bitmap CreateAssetTypeIcon(int width, int height) {
            return CreateSimpleAssetTypeIcon("my_location", width, height, LibraryData.BG_COLOR);
        }
    }
}
