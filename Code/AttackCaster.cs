using System;
using System.Collections.Generic;
using MANIFOLD.BHLib.Events;
using Sandbox;

namespace MANIFOLD.BHLib {
    public class AttackCaster : Component {
        [Property]
        public List<AttackData> Attacks { get; set; } = new List<AttackData>();

        private bool inPreview;
        private AttackData previewData;
        private Dictionary<SpawnEvent, PreviewEntity> previewEntities;
        
        public bool InPreviewMode => inPreview;
        public AttackData PreviewedAttack => previewData;

        // UTILITY
        private GameObject CreateEntity(EntityData data, Transform localTransform) {
            GameObject go = Scene.CreateObject();
            go.WorldTransform = LocalTransform.ToWorld(localTransform);
            foreach (var component in data.Components) {
                if (!component.Enabled) continue;
                component.Create(go);
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
