using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Used as a tool in previews.
    /// </summary>
    [Hide]
    public class PreviewEntity : Component, Component.ExecuteInEditor {
        private EntityComponent[] components;
        
        public Transform InitialTransform { get; set; }
        public float SpawnTime { get; set; }

        public void Initialize() {
            GameObject.Flags |= GameObjectFlags.EditorOnly;
            components = GetComponents<EntityComponent>().ToArray();
            RecordInitialTransform();
        }
        
        public void GetAllComponents() {
            components = GetComponents<EntityComponent>().ToArray();
        }
        
        public void RecordInitialTransform() {
            InitialTransform = WorldTransform;
        }

        public void Revert() {
            WorldTransform = InitialTransform;
        }

        public void Simulate(float deltaTime) {
            foreach (var component in components) {
                component.Simulate(deltaTime);
            }
        }

        public void Resimulate(float time) {
            Revert();
            GameObject.Enabled = time >= SpawnTime;
            
            if (!GameObject.Enabled) return;
            foreach (var component in components) {
                component.Simulate(time - SpawnTime);   
            }
        }
    }
}
