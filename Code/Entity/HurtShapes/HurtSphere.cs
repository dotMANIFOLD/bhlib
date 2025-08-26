using Sandbox;

namespace MANIFOLD.BHLib.Components {
    [Title("Sphere")]
    [Category("Hurt Shapes")]
    [Icon("circle")]
    public class HurtSphereDefintion : HurtComponentDefinition {
        public float Radius { get; set; } = 10;
            
        public override EntityComponent Create(GameObject obj) {
            var comp = obj.AddComponent<HurtSphere>();
            comp.Data = this;
            return comp;
        }
    }
    
    /// <summary>
    /// Hurts targets within a sphere.
    /// </summary>
    [Category(LibraryData.CATEGORY + "/Hurt Shapes")]
    [Icon("circle")]
    [Hide]
    public class HurtSphere : EntityComponent {
        public HurtSphereDefintion Data { get; set; }

        protected override void DrawGizmos() {
            if (Data == null) return;

            Gizmo.Draw.Color = Color.Red;
            Gizmo.Draw.LineSphere(0, Data.Radius);
        }

        public override void SimulateFrame(float deltaTime) {
            base.SimulateFrame(deltaTime);
            
            UpdatePhysics();
        }

        private void UpdatePhysics() {
            var result = Scene.Trace.Sphere(Data.Radius, WorldPosition, WorldPosition).Run();
            ProcessTrace(result);
        }
    }
}
