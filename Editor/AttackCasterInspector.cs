using System;
using System.Linq;
using Editor;
using Sandbox;
using Sandbox.UI;
using Application = Editor.Application;
using Button = Editor.Button;
using ControlSheet = Editor.ControlSheet;
using Label = Editor.Label;

namespace MANIFOLD.BHLib.Editor {
    [CustomEditor(typeof(AttackCaster))]
    public class AttackCasterInspector : ComponentEditorWidget {
        public class AttackProgress : Widget {
            public const float BAR_MARGIN = 2;
            public const float BAR_SIZE = 30;

            private readonly AttackCaster caster;
            private readonly AttackCaster.Instance instance;
            private readonly Label label;

            public AttackProgress(AttackCaster caster, AttackCaster.Instance instance, Widget parent = null) : base(parent) {
                this.caster = caster;
                this.instance = instance;
                Layout = Layout.Column();
                Layout.Margin = new Margin(8, 0);
                
                FixedHeight = 40;

                var row = Layout.AddRow();
                label = row.Add(new Label(instance.Attack.ResourceName ?? "Embedded Resource"));
                row.AddStretchCell();
                row.Add(new Button("Stop") { Clicked = () => {
                    caster.StopAttack(instance);
                }});
                
                Layout.AddStretchCell();

                VerticalSizeMode = SizeMode.CanGrow;
                HorizontalSizeMode = SizeMode.Default;
            }

            [EditorEvent.Frame]
            private void OnFrame() {
                Update();
            }
            
            protected override void OnPaint() {
                base.OnPaint();
                
                var rect = LocalRect;
                rect.Shrink(Layout.Margin);
                rect.Top = label.Height + BAR_MARGIN;
                
                Paint.SetBrushAndPen(Theme.ControlBackground);
                Paint.DrawRect(rect);
                
                rect.Width *= instance.NormalizedTime;
                
                Paint.SetBrushAndPen(Theme.Green);
                Paint.DrawRect(rect);
                
                Paint.ClearBrush();
                Paint.SetPen(Theme.TextDark);
                Paint.DrawText(rect.Shrink(1), $"{(instance.NormalizedTime * 100).FloorToInt()}%", TextFlag.Right);
            }
        }
        
        private ControlSheet sheet;
        private Widget runtimeCanvas;
        private Layout attackListLayout;
        private Layout instanceListLayout;
        
        private int lastInstanceCount;
        
        public AttackCaster Target => SerializedObject.Targets.FirstOrDefault() as AttackCaster;
        
        public AttackCasterInspector(SerializedObject obj) : base(obj) {
            Layout = Layout.Column();
            Layout.Margin = 0;
            Layout.Spacing = 4;

            sheet = new ControlSheet();
            Layout.Add(sheet);
            RebuildSheet();
            
            runtimeCanvas = Layout.Add(new Widget());
            runtimeCanvas.Layout = Layout.Column();
            runtimeCanvas.Layout.Margin = new Margin(8, 0);

            runtimeCanvas.Layout.AddSeparator();
            
            // ATTACK LIST
            runtimeCanvas.Layout.Add(new Label.Header("Attacks"));
            attackListLayout = runtimeCanvas.Layout.AddColumn();
            // RebuildAttackList();
            
            // INSTANCE LIST
            runtimeCanvas.Layout.Add(new Label.Header("Instances"));
            instanceListLayout = runtimeCanvas.Layout.AddColumn();
            // RebuildInstanceList();
            
            if (Target.Scene.IsEditor) OnSceneStop();
            else OnScenePlay();
        }
        
        [Event("scene.play")]
        private void OnScenePlay() {
            runtimeCanvas.Visible = true;
            lastInstanceCount = 0;
            RebuildAttackList();
        }

        [Event("scene.stop")]
        private void OnSceneStop() {
            runtimeCanvas.Visible = false;
        }

        [EditorEvent.Frame]
        private void OnFrame() {
            if (Target == null) return;
            if (Target.Scene.IsEditor) return;
            if (Target.Instances.Count != lastInstanceCount) {
                RebuildInstanceList();
                lastInstanceCount = Target.Instances.Count;
            }
        }

        private void RebuildSheet() {
            sheet.Clear(true);
            sheet.AddObject(SerializedObject, PropertyFilter);
        }
        
        private void RebuildAttackList() {
            attackListLayout.Clear(true);
            attackListLayout.Margin = new Margin(8, 0);
            attackListLayout.Spacing = 4;

            for (int i = 0; i < Target.Attacks.Count; i++) {
                var attack = Target.Attacks[i];
                var row = attackListLayout.AddRow();
                row.Add(new Label(attack.ResourceName ?? "Embedded Resource"));
                row.AddStretchCell();
                row.Add(new Button("Cast") {
                    Clicked = () => {
                        Target.PlayAttack(attack);
                    }
                });
            }
        }

        private void RebuildInstanceList() {
            instanceListLayout.Clear(true);
            instanceListLayout.Margin = new Margin(8, 0);
            instanceListLayout.Spacing = 4;

            for (int i = 0; i < Target.Instances.Count; i++) {
                var inst = Target.Instances[i];
                instanceListLayout.Add(new AttackProgress(Target, inst));
            }
        }

        private bool PropertyFilter(SerializedProperty o) {
            var hideInEventTab = o.PropertyType.IsAssignableTo( typeof( Delegate ) ) && o.Name.StartsWith( "OnComponent" );
            if (hideInEventTab) return false;
            if (!Target.Scene.IsEditor && o.Name == nameof(AttackCaster.Attacks)) return false;
            if (!o.HasAttribute<PropertyAttribute>()) return false;
            return true;
        }
    }
}
