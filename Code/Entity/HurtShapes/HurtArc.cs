using System;
using Sandbox;

namespace MANIFOLD.BHLib.Components {
    [Title("Arc")]
    [Category("Hurt Shapes")]
    [Icon("signal_wifi_0_bar")]
    public class HurtArcDefinition : HurtComponentDefinition {
        [Range(0f, 360f), Space]
        public float Angle { get; set; }
        public bool Centered { get; set; } = true;
        [Space]
        public float EndDistance { get; set; }
        public float Thickness { get; set; }
        public float DistanceVelocity { get; set; }
        
        public override EntityComponent Create(GameObject obj) {
            var comp = obj.AddComponent<HurtArc>();
            comp.Data = this;
            return comp;
        }
    }
    
    /// <summary>
    /// Hurts targets within in arc.
    /// </summary>
    [Category(LibraryData.CATEGORY + "/Hurt Shapes")]
    [Icon("signal_wifi_0_bar")]
    [Hide]
    public class HurtArc : EntityComponent {
        public const float MIN_THICKNESS = 0.1f;

        private bool triedRender;
        private ArcRenderer renderer;
        private float additive;
        
        public HurtArcDefinition Data { get; set; }

        public float RealEndDistance => Data.EndDistance + additive;

        protected override void OnFixedUpdate() {
            if (!triedRender) {
                TryGetRenderer();
                triedRender = true;
            }
            
            base.OnFixedUpdate();
        }

        public override void SimulateFrame(float deltaTime) {
            base.SimulateFrame(deltaTime);
            
            additive += Data.DistanceVelocity * deltaTime;
            
            if (Target.IsValid()) CheckPhysics();
            if (renderer.IsValid()) UpdateModel();
        }

        public override void Simulate(float time) {
            additive = 0;
            base.Simulate(time);
        }

        protected override void DrawGizmos() {
            float rads = Data.Angle.DegreeToRadian();
            float startRads = Data.Centered ? rads * -0.5f : 0;
            float endRads = Data.Centered ? rads * 0.5f : rads;
            
            Vector3 startDir = new Vector3(MathF.Cos(startRads), MathF.Sin(startRads), 0f);
            Vector3 endDir = new Vector3(MathF.Cos(endRads), MathF.Sin(endRads), 0f);
            float startDistance = MathF.Max(RealEndDistance - Data.Thickness, 0);
            
            Gizmo.Draw.Color = Color.Red;
            Gizmo.Draw.Line(startDir * startDistance, startDir * RealEndDistance);
            Gizmo.Draw.Line(endDir * startDistance, endDir * RealEndDistance);
            DrawArc(startRads, endRads, RealEndDistance);

            if (Data.Thickness > 0 && !Data.Thickness.AlmostEqual(0)) {
                if (startDistance > 0) {
                    DrawArc(startRads, endRads, startDistance);

                    Gizmo.Draw.Color = Color.White;
                    Gizmo.Draw.Line(0, Vector3.Forward * startDistance);
                }
            }
        }

        private void TryGetRenderer() {
            var indirect = GetComponent<Renderer>();
            if (indirect.IsValid()) {
                renderer = indirect.Held.GetComponent<ArcRenderer>();
            }
        }
        
        private void CheckPhysics() {
            Vector3 toPlayer = Target.WorldPosition - WorldPosition;
            Vector3 localToPlayer = WorldRotation.Inverse * toPlayer;
            
            float startDistance = MathF.Max(RealEndDistance - Data.Thickness, 0);
            float realThickness = RealEndDistance - startDistance;
            float halfThickness = realThickness * 0.5f;
            float sampleLength = RealEndDistance.LerpTo(startDistance, 0.5f);
            
            float minAngle = (Data.Centered ? Data.Angle * -0.5f : 0).DegreeToRadian();
            float maxAngle = (Data.Centered ? Data.Angle * 0.5f : Data.Angle).DegreeToRadian();

            float circumference = 2 * MathF.PI * sampleLength;
            float posSoftMargin = (halfThickness / circumference) * MathF.PI * 2;
            float posHardMargin = (MIN_THICKNESS / circumference) * MathF.PI * 2;
            
            var softEdges = GetMargins(minAngle, maxAngle, posSoftMargin);
            var hardEdges = GetMargins(minAngle, maxAngle, posHardMargin);

            float rawRads = MathF.Atan2(localToPlayer.y, localToPlayer.x);
            float posRads = rawRads.Clamp(hardEdges.left, hardEdges.right);
            float rotRads = rawRads.Clamp(minAngle, maxAngle);
            float sizeFactor = posRads.LerpInverse(hardEdges.left, softEdges.left) * posRads.LerpInverse(hardEdges.right, softEdges.right);

            Vector3 sampleDir = new Vector3(MathF.Cos(posRads), MathF.Sin(posRads), 0f);
            Vector3 samplePos = WorldTransform.PointToWorld(sampleDir * sampleLength);
            Rotation sampleRot = new Angles(0f, rotRads.RadianToDegree(), 0f);
            Vector3 sampleSize = realThickness;
            sampleSize.y = MathF.Min(sampleSize.y, (Data.Angle / 360) * circumference);
            sampleSize.y = sampleSize.y.LerpTo(MIN_THICKNESS, 1 - sizeFactor);

            var result = Scene.Trace.Box(sampleSize, samplePos, samplePos).Rotated(sampleRot).WithTag("player").Run();
            var wasTarget = CheckForTarget(result);
            if (wasTarget) {
                var successful = Target.Hurt(new DamageInfo() {
                    attacker = GameObject,
                    damage = Data.Damage,
                    impulseDirection = (Target.WorldPosition - samplePos).WithZ(0).Normal
                });
                if (successful && Data.DestroyOnHurt) {
                    GameObject.Destroy();
                }
            }
            
            // DebugOverlay.Box(0, Thickness, Color.Red, Time.Delta, new Transform(samplePos, WorldRotation * sampleRot));
            DebugOverlay.Sphere(new Sphere(WorldPosition + (sampleDir * sampleLength), 8), Color.Blue, Time.Delta);
            DebugOverlay.Trace(result);
        }
        
        private void UpdateModel() {
            renderer.Angle = Data.Angle;
            renderer.Centered = Data.Centered;
            renderer.StartLength = RealEndDistance - Data.Thickness;
            renderer.EndLength = RealEndDistance;
        }

        private (float left, float right) GetMargins(float left, float right, float margin) {
            return (MathF.Min(left + margin, Data.Centered ? 0 : margin), MathF.Max(right - margin, Data.Centered ? 0 : -margin));
        }
        
        protected void DrawArc(float startRads, float endRads, float distance, int resolution = 24) {
            Vector3 prevPoint = new Vector3(MathF.Cos(startRads), MathF.Sin(startRads), 0f) * distance;

            for (int i = 0; i < resolution; i++) {
                float factor = (i + 1) / (float)resolution;
                float rads = startRads.LerpTo(endRads, factor);
                Vector3 nextPoint = new Vector3(MathF.Cos(rads), MathF.Sin(rads), 0f) * distance;
                
                Gizmo.Draw.Line(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }
}
