using System;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.BHLib {
    /// <summary>
    /// Base class for all events used in attacks.
    /// </summary>
    public abstract class AttackEvent {
        [Hide]
        public Guid ID { get; set; } = Guid.NewGuid();
        public float Time { get; set; }
        public string Name { get; set; }
        [Hide]
        public bool Hidden { get; set; }
    }

    public class AttackEventComparer : IComparer<AttackEvent> {
        public int Compare(AttackEvent x, AttackEvent y) {
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;
            return x.Time.CompareTo(y.Time);
        }
    }
}
