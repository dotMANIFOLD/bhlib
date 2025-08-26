using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.BHLib {
    public interface ITimeline {
        public IReadOnlyList<AttackEvent> Events { get; }
        public Type EventType { get; }

        public AttackEvent GetEvent(Guid id);
        public void AddEvent(AttackEvent evt);
        public void RemoveEvent(AttackEvent evt);
        public void Sort();
    }
    
    public class Timeline<T> : ITimeline where T : AttackEvent {
        public const string TYPE_FIELD = "__type";
        
        [JsonIgnore]
        public List<T> Events { get; set; } = new List<T>();

        IReadOnlyList<AttackEvent> ITimeline.Events => Events;
        [Hide, JsonIgnore]
        public Type EventType => typeof(T);
        
        [Hide]
        public JsonArray SerializedEvents {
            get {
                JsonArray arr = new JsonArray();
                foreach (var module in Events) {
                    var jsonNode = Json.ToNode(module);
                    jsonNode[TYPE_FIELD] = Json.ToNode(module.GetType(), typeof(Type));
                    arr.Add(jsonNode);
                }
                return arr;
            }
            set {
                Events.Clear();
                foreach (var node in value) {
                    var type = Json.FromNode<Type>(node[TYPE_FIELD]);
                    var deserialized = (T)Json.Deserialize(node.ToString(), type);
                    Events.Add(deserialized);
                }
            }
        }

        public T GetEvent(Guid id) {
            return Events.FirstOrDefault(x => x.ID == id);
        }
        AttackEvent ITimeline.GetEvent(Guid id) => GetEvent(id);
        
        public void AddEvent(AttackEvent evt) {
            if (evt is not T casted) throw new ArgumentException("Invalid event type", nameof(evt));
            Events.Add(casted);
        }

        public void RemoveEvent(AttackEvent evt) {
            if (evt is not T casted) throw new ArgumentException("Invalid event type", nameof(evt));
            Events.Remove(casted);
        }

        public void Sort() {
            Events.Sort(new AttackEventComparer());
        }
    }

    public class TimelineSampler<T> where T : AttackEvent {
        private readonly Timeline<T> timeline;

        private float time;
        private int currentIndex;
        private T[] cache;

        public float Time => time;
        public bool Exhausted => currentIndex == timeline.Events.Count;
        
        public TimelineSampler(Timeline<T> timeline) {
            this.timeline = timeline;
            time = 0;
            currentIndex = 0;
            cache = new T[20]; // just a guess
        }

        public IEnumerable<T> Read(float delta) {
            if (Exhausted) return [];
            if (delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));

            time += delta;
            
            int localIndex = 0;
            while (currentIndex < timeline.Events.Count) {
                var evt = timeline.Events[currentIndex];
                if (evt.Time <= time) {
                    cache[localIndex] = evt;
                    localIndex++;
                } else {
                    break;
                }
                currentIndex++;
            }

            if (localIndex == 0) return [];
            return cache[..localIndex];
        }
    }
}
