using System;
using Sandbox;

namespace MANIFOLD.BHLib {
    public abstract class AttackEvent {
        [Hide]
        public Guid ID { get; set; } = Guid.NewGuid();
        public float Time { get; set; }
        public string Name { get; set; }
        [Hide]
        public bool Hidden { get; set; }
    }
}
