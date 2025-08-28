using System.Linq;
using Editor;
using Sandbox;
using Sandbox.UI;
using ControlSheet = Editor.ControlSheet;
using Label = Editor.Label;

namespace MANIFOLD.BHLib.Editor {
    [Inspector(typeof(AttackData))]
    public class AttackDataInspector : InspectorWidget {
        public class TimelineInfo : Widget {
            public TimelineInfo(string name, ITimeline timeline, Widget parent = null) : base(parent) {
                var grid = Layout.Grid();
                grid.Margin = 4;
                grid.VerticalSpacing = 8;
                
                Layout = grid;

                grid.AddCell(0, 0, new Label(name));
                grid.AddCell(1, 0, new Label(timeline.EventType.Name), alignment: TextFlag.Right);
                grid.AddCell(0, 1, new Label($"Event Count: {timeline.Events.Count}"));
                grid.AddCell(0, 2, new Label($"Duration: {(timeline.Events.Count > 0 ? timeline.Events[^1].Time : 0).ToString("N2")}"));
            }

            protected override void OnPaint() {
                var rect = LocalRect;
                rect.Bottom = Theme.RowHeight;
                
                Paint.SetBrushAndPen(Theme.BaseAlt);
                Paint.DrawRect(rect);
            }
        }

        public class RendererInfo : Widget {
            public RendererInfo(AttackData.RendererData data, Widget parent = null) : base(parent) {
                Layout = Layout.Row();

                var prefab = (PrefabScene)data.Prefab;
                
                var renderer = Layout.Add(new SceneRenderingWidget());
                renderer.Scene = Scene.CreateEditorScene();
                renderer.FixedSize = 80;
                
                using (renderer.Scene.Push()) {
                    var camera = new GameObject("Camera").AddComponent<CameraComponent>();
                    camera.BackgroundColor = new Color(0, 0.05f, 0.15f);
                    camera.WorldPosition = new Vector3(-60, 60, 100);
                    camera.WorldRotation = Rotation.LookAt((camera.WorldPosition * -1).Normal);
                    
                    var clone = prefab.Clone();
                    clone.WorldTransform = Transform.Zero;
                }
                
                Layout.AddStretchCell();

                var column = Layout.AddColumn();
                column.Alignment = TextFlag.RightTop;
                
                column.Add(new Label.Header(prefab.Name) { Alignment = TextFlag.Right });
                column.Add(new Label($"Path: {prefab.Source.ResourcePath}") { Alignment = TextFlag.Right });
                column.Add(new Label($"Use Count: {data.UseCount}") { Alignment = TextFlag.Right });
            }
        }
        
        private AttackData target;
        
        private ControlSheet sheet;
        private Layout timelineLayout;
        private Layout rendererLayout;

        public AttackDataInspector(SerializedObject so) : base(so) {
            Layout = Layout.Column();
            Layout.Margin = 8;
            Layout.Spacing = 4;
            
            if (so.Targets.FirstOrDefault() is not AttackData resource) return;
            target = resource;

            sheet = new ControlSheet();
            Layout.Add(sheet);
            sheet.AddObject(so, SheetFilter);

            Layout.Add(new Label.Header("Timelines"));
            timelineLayout = Layout.AddColumn();
            timelineLayout.Margin = new Margin(8, 0);
            timelineLayout.Spacing = 4;

            Layout.Add(new Label.Header("Renderers"));
            rendererLayout = Layout.AddColumn();
            rendererLayout.Margin = new Margin(8, 0);
            rendererLayout.Spacing = 4;
            
            Layout.AddStretchCell();
            
            RebuildTimelines();
            RebuildRenderers();
        }

        private void RebuildTimelines() {
            timelineLayout.Clear(true);
            
            var timelines = EditorTypeLibrary
                .GetType(target.GetType())
                .Properties.Where(x => x.PropertyType.IsAssignableTo(typeof(ITimeline)));
            foreach (var prop in timelines) {
                var timeline = (ITimeline)prop.GetValue(target);
                timelineLayout.Add(new TimelineInfo(prop.Name, timeline, this));
            }
        }

        private void RebuildRenderers() {
            rendererLayout.Clear(true);

            foreach (var data in target.RenderPoolingData) {
                rendererLayout.Add(new RendererInfo(data, this));
            }
        }
        
        private bool SheetFilter(SerializedProperty prop) {
            if (prop.HasAttribute<HideAttribute>()) return false;
            if (prop.PropertyType.IsAssignableTo(typeof(ITimeline))) return false;
            if (prop.Name == nameof(AttackData.RenderPoolingData)) return false;
            return true;
        }
    }
}
