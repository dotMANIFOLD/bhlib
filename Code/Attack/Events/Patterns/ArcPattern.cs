using System;
using Sandbox;

namespace MANIFOLD.BHLib.Events {
    public class ArcPattern : PatternEvent, IDrawGizmos {
        [Space]
        public Vector3 Position { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public float Radius { get; set; }
        [Space]
        public int SpawnCount { get; set; }
        public float? SpawnInterval { get; set; }
        public bool FaceNormal { get; set; }
        [ShowIf(nameof(FaceNormal), true)]
        public bool FlipNormal { get; set; }
        [ShowIf(nameof(FaceNormal), false)]
        public Angles SpawnRotation { get; set; }
        [Space]
        public EntityData Data { get; set; }

        public override void OnRemove(AttackData data) {
            foreach (var id in SpawnIDs) {
                var evt = data.SpawnTimeline.GetEvent(id);
                data.SpawnTimeline.RemoveEvent(evt);
            }
        }

        public override void Modify(AttackData data) {
            HandleCountChange(data);
            UpdateEvents(data);
        }

        private void HandleCountChange(AttackData data) {
            int delta = SpawnCount - SpawnIDs.Count;
            if (delta == 0) return;
            if (delta < 0) {
                int target = SpawnIDs.Count + delta;
                for (int i = SpawnIDs.Count - 1; i >= target; i--) {
                    var evt = data.SpawnTimeline.GetEvent(SpawnIDs[i]);
                    data.SpawnTimeline.RemoveEvent(evt);
                    SpawnIDs.RemoveAt(i);
                }
            } else {
                for (int i = 0; i < delta; i++) {
                    SpawnEntity spawnEvt = new SpawnEntity();
                    SpawnIDs.Add(spawnEvt.ID);
                    data.SpawnTimeline.Events.Add(spawnEvt);
                }
            }
        }

        private void UpdateEvents(AttackData data) {
            var distribution = GetDistribution();
            for (int i = 0; i < SpawnCount; i++) {
                var evt = (SpawnEntity)data.SpawnTimeline.GetEvent(SpawnIDs[i]);
                evt.Time = Time + (SpawnInterval.HasValue ? SpawnInterval.Value * i : 0);
                evt.Name = $"PAT // {Name} // {i}";
                evt.Hidden = true;
                evt.Position = distribution[i].pos;
                evt.Rotation = distribution[i].rot;

                evt.Data = null;
                evt.Data = Data;
            }
        }

        public void DrawGizmos() {
            float delta = EndAngle - StartAngle;
            int lines = (delta / 5).FloorToInt();
            
            Gizmo.Draw.LineThickness = 2;
            Gizmo.Draw.Color = Color.White;
            for (int i = 0; i < lines; i++) {
                float factor = (float)i / (lines - 1);
                float nextFactor = ((float)i + 1) / (lines - 1);
                
                float firstRads = StartAngle.LerpTo(EndAngle, factor).DegreeToRadian();
                float secondRads = StartAngle.LerpTo(EndAngle, nextFactor).DegreeToRadian();
                Vector3 firstDir = new Vector3(MathF.Cos(firstRads), MathF.Sin(firstRads));
                Vector3 secondDir = new Vector3(MathF.Cos(secondRads), MathF.Sin(secondRads));
                
                Gizmo.Draw.Line(Position + firstDir * Radius, Position + secondDir * Radius);
            }
            
            var distribution = GetDistribution();
            for (int i = 0; i < SpawnCount; i++) {
                float factor = (float)i / (SpawnCount - 1);
                Gizmo.Draw.Color = Color.Lerp(Color.Red, Color.Green, factor);
                Gizmo.Draw.SolidSphere(distribution[i].pos, 2);

                Vector3 arrowEnd = distribution[i].pos + (distribution[i].rot.ToRotation() * Vector3.Forward * 10);
                Gizmo.Draw.Arrow(distribution[i].pos, arrowEnd, 4, 1.5f);
            }
        }

        private (Vector3 pos, Angles rot)[] GetDistribution() {
            if (SpawnCount == 0) return [];
            
            var arr = new (Vector3 pos, Angles rot)[SpawnCount];
            for (int i = 0; i < SpawnCount; i++) {
                float factor = (float)i / (SpawnCount - 1);
                float angle = StartAngle.LerpTo(EndAngle, factor);
                float rads = angle.DegreeToRadian();
                Vector3 dir = new Vector3(MathF.Cos(rads), MathF.Sin(rads));
                arr[i].pos = Position + dir * Radius;
                arr[i].rot = FaceNormal ? Rotation.LookAt(dir * (FlipNormal ? -1 : 1)) : SpawnRotation;
            }
            return arr;
        }
    }
}
