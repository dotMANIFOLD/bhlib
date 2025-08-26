using System;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.BHLib.Events {
    public abstract class PatternEvent : AttackEvent, IModifier {
        [Hide]
        public List<Guid> SpawnIDs { get; set; } = new List<Guid>();

        public virtual void OnAdd(AttackData data) {
            
        }

        public virtual void OnRemove(AttackData data) {
            
        }

        public virtual void Modify(AttackData data) {
            
        }
    }
}
