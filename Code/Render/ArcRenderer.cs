using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.BHLib {
    [Category(LibraryData.CATEGORY + "/Rendering")]
    [Icon("wifi_tethering")]
    public class ArcRenderer : Component {
       [Property]
       public ModelRenderer Model { get; set; }
       [Property]
       public Angles ModelRotationOffset { get; set; }
       [Property]
       public float ModelRadius { get; set; }
       [Property, Range(0, 360), Space]
       public float Angle { get; set; } = 90f;
       [Property]
       public bool Centered { get; set; }
       [Property]
       public float StartLength { get; set; }
       [Property]
       public float EndLength { get; set; }

       protected override void OnEnabled() {
           StartLength = 0;
           EndLength = 0;
       }

       protected override void OnPreRender() {
           float angle = Centered ? Angle * 0.5f : Angle;
           Model.LocalRotation = ModelRotationOffset + new Angles(0f, angle, 0f);
           Model.LocalScale = EndLength / ModelRadius;
           Model.SceneObject.Batchable = false;
           Model.Attributes.Set("Arc", 1 - (Angle / 360f));
           Model.Attributes.Set("StartLength", 1 - ((EndLength - StartLength) / EndLength));
       }
    }
}
