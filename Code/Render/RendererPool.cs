using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Manages renderer pools.
    /// </summary>
    public static class RendererPool {
        public class Pool {
            public readonly GameObject prefab;
            public readonly int initialCapacity;
            
            public HashSet<GameObject> objects = new HashSet<GameObject>();
            public int capacity;
            
            public Pool(GameObject prefab, int capacity) {
                this.prefab = prefab;
                initialCapacity = capacity;
                objects = new HashSet<GameObject>();

                AddCapacity(capacity);
            }
            
            public GameObject Request() {
                if (objects.Count == 0) {
                    return CreateNewInstance();
                }
                var obj = objects.First();
                obj.Enabled = true;
                objects.Remove(obj);
                return obj;
            }

            public void Release(GameObject obj) {
                objects.Add(obj);
                obj.Parent = PoolRoot;
                obj.Enabled = false;
            }

            public void AddCapacity(int capacity) {
                for (int i = 0; i < capacity; i++) {
                    Release(CreateNewInstance());
                }
                this.capacity = capacity;
            }
            
            private GameObject CreateNewInstance() {
                return prefab.Clone();
            }
        }
        
        public static GameObject PoolRoot { get; private set; }

        private static Dictionary<GameObject, Pool> pools = new();
        private static Dictionary<GameObject, Pool> directory = new();

        public static void CreatePool(GameObject prefab, int capacity) {
            if (!PoolRoot.IsValid()) {
                CreatePoolRoot();
            }
            
            if (pools.ContainsKey(prefab)) {
                var pool = pools[prefab];
                if (pool.capacity < capacity) {
                    pool.AddCapacity(capacity - pool.capacity);
                }
            } else {
                pools.Add(prefab, new Pool(prefab, capacity));
            }
        }

        public static GameObject Request(GameObject prefab) {
            if (Game.ActiveScene.IsEditor) return null; // return immediately in editor mode
            if (!pools.ContainsKey(prefab)) {
                throw new InvalidOperationException("No pool was created for this prefab");
            }
            
            var pool = pools[prefab];
            var obj = pool.Request();
            directory.Add(obj, pool);
            return obj;
        }

        public static void Release(GameObject obj) {
            if (!directory.ContainsKey(obj)) {
                throw new InvalidOperationException("This object doesn't belong to a pool");
            }
            
            var pool = directory[obj];
            pool.Release(obj);
            directory.Remove(obj);
        }
        
        private static void CreatePoolRoot() {
            PoolRoot = Game.ActiveScene.CreateObject();
            PoolRoot.Name = "RendererPool_Root";
            pools.Clear(); // if the root is gone then assume all pools are broken
            directory.Clear();
        }
    }
}
