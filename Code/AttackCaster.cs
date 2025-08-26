using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.BHLib.Events;
using Sandbox;

namespace MANIFOLD.BHLib {
    public class AttackCaster : Component {
        public class Instance {
            private readonly AttackCaster caster;
            private readonly AttackData attack;

            private TimelineSampler<SpawnEvent> spawnSampler;
            private TimelineSampler<CasterEvent> casterSampler;

            public AttackData Attack => attack;
            public float Time => spawnSampler.Time;
            public float NormalizedTime => spawnSampler.Time / attack.Duration;
            
            public Instance(AttackCaster caster, AttackData data) {
                this.caster = caster;
                attack = data;

                spawnSampler = new TimelineSampler<SpawnEvent>(attack.SpawnTimeline);
                casterSampler = new TimelineSampler<CasterEvent>(attack.CasterTimeline);
            }

            public bool Update(float deltaTime) {
                var spawnEvents = spawnSampler.Read(deltaTime);
                foreach (var evt in spawnEvents) {
                    float timePassed = spawnSampler.Time - evt.Time;
                    if (evt is SpawnEntity entEvt) {
                        caster.CreateEntity(entEvt.Data, new Transform(evt.Position, evt.Rotation), timePassed);
                    } else {
                        Log.Warning($"No spawn implementation for type: {evt.GetType()}. Skipping...");
                    }
                }

                var casterEvents = casterSampler.Read(deltaTime);
                foreach (var evt in casterEvents) {
                    foreach (IEventListener listener in caster.listeners) {
                        listener.OnCasterEvent(evt);
                    }
                }

                return spawnSampler.Time >= attack.Duration;
            }
        }
        
        [Property]
        public List<AttackData> Attacks { get; set; } = new List<AttackData>();

        [Property]
        public GameObject Target {
            get => gameObjTarget;
            set {
                gameObjTarget = value;
                RealTarget = gameObjTarget.GetComponent<ITarget>();
            }
        }

        private List<Instance> instances;
        private IEventListener[] listeners;
        private GameObject gameObjTarget;
        
        // PREVIEW
        private bool inPreview;
        private AttackData previewData;
        private Dictionary<SpawnEvent, PreviewEntity> previewEntities;
        
        // ACCESSORS
        public ITarget RealTarget { get; set; }
        public IReadOnlyList<Instance> Instances => instances;
        
        // PREVIEW ACCESSORS
        public bool InPreviewMode => inPreview;
        public AttackData PreviewedAttack => previewData;

        protected override void OnAwake() {
            instances = new List<Instance>();
            listeners = GetComponents<IEventListener>().ToArray();
        }

        protected override void OnStart() {
            Dictionary<GameObject, int> cache = new Dictionary<GameObject, int>();
            foreach (var attack in Attacks) {
                foreach (var renderer in attack.RenderPoolingData) {
                    if (cache.TryGetValue(renderer.Prefab, out var capacity)) {
                        int newCapacity = Math.Max(capacity, renderer.UseCount);
                        cache[renderer.Prefab] = newCapacity;
                    } else {
                        cache.Add(renderer.Prefab, renderer.UseCount);
                    }
                }
            }

            foreach (var pair in cache) {
                RendererPool.CreatePool(pair.Key, (pair.Value * 1.5f).CeilToInt());
            }
        }

        protected override void OnFixedUpdate() {
            for (int i = 0; i < instances.Count; i++) {
                bool finished = instances[i].Update(Time.Delta);
                if (finished) {
                    instances.RemoveAt(i);
                    i--;
                }
            }
        }

        public void PlayAttack(AttackData data) {
            PlayAttack(data, WorldTransform);
        }

        public void PlayAttack(AttackData data, Transform transform) {
            instances.Add(new Instance(this, data));
        }
        
        // UTILITY
        private GameObject CreateEntity(EntityData data, Transform localTransform, float? simulate = null) {
            GameObject go = Scene.CreateObject();
            go.WorldTransform = LocalTransform.ToWorld(localTransform);
            foreach (var component in data.Components) {
                if (!component.Enabled) continue;
                var inst = component.Create(go);
                inst.Target = RealTarget;
                if (simulate.HasValue) {
                    inst.SimulateFrame(simulate.Value);
                }
            }
            return go;
        }
        
        // PREVIEW UTILITY
        public void StartPreview(AttackData data) {
            if (inPreview) return;
            
            previewEntities = new Dictionary<SpawnEvent, PreviewEntity>();
            previewData = data;
            inPreview = true;
            
            RebuildPreviewEntities();
        }

        public void StopPreview() {
            if (!inPreview) return;
            
            CleanupPreviewEntities();

            previewEntities = null;
            previewData = null;
            inPreview = false;
        }

        public void ResimulatePreview(float time) {
            foreach (var entity in previewEntities.Values) {
                entity.Resimulate(time);
            }
        }

        public void RebuildEntity(SpawnEntity evt) {
            if (previewEntities.TryGetValue(evt, out PreviewEntity existingEnt)) {
                existingEnt.GameObject.DestroyImmediate();
                previewEntities.Remove(evt);
            }
            
            var result = CreatePreviewEntity(evt);
            if (result.IsValid()) previewEntities.Add(evt, result);
        }
        
        /// <summary>
        /// This will catch any timeline additions/removals.
        /// </summary>
        /// <param name="full"></param>
        public void RebuildPreviewEntities(bool full = false) {
            if (full) {
                CleanupPreviewEntities();
            }

            // additions
            foreach (var evt in previewData.SpawnTimeline.Events) {
                if (evt is not SpawnEntity entSpawn) continue;
                if (previewEntities.ContainsKey(evt)) continue;

                var result = CreatePreviewEntity(entSpawn);
                if (result.IsValid()) previewEntities.Add(evt, result);
            }
        }

        private void CleanupPreviewEntities() {
            foreach (var entity in previewEntities.Values) {
                entity.GameObject.DestroyImmediate();
            }
            previewEntities.Clear();
        }
        
        private PreviewEntity CreatePreviewEntity(SpawnEntity evt) {
            if (evt.Data == null) {
                Log.Warning($"Tried to create preview entity with no data! Event: {evt.Name} ({evt.ID})");
                return null;
            }
            
            var go = CreateEntity(evt.Data, new Transform(evt.Position, evt.Rotation));
            var preview = go.AddComponent<PreviewEntity>();
            preview.SpawnTime = evt.Time;
            preview.Initialize();
            return preview;
        }
    }
}
