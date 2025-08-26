using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.BHLib {
    [GameResource("Entity Data", "bhent", "BH Entity data", Category = LibraryData.CATEGORY, Icon = "my_location", IconBgColor = LibraryData.BG_COLOR)]
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
    }
}
