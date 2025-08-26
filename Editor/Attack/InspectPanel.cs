using System;
using Editor;
using MANIFOLD.BHLib.Events;
using Sandbox;

namespace MANIFOLD.BHLib.Editor {
    public class InspectPanel : Widget {
        private readonly AttackEditor editor;
        private ControlSheet sheet;
        private object valueCache;

        public InspectPanel(AttackEditor editor, Widget parent = null) : base(parent) {
            this.editor = editor;

            MinimumWidth = 400;

            Layout = Layout.Column();
            Layout.Margin = 8;

            sheet = new ControlSheet();

            var scroll = Layout.Add(new ScrollArea(this));
            scroll.HorizontalScrollbarMode = ScrollbarMode.Off;
            scroll.Canvas = new Widget(scroll);
            scroll.Canvas.Layout = Layout.Column();
            scroll.Canvas.Layout.Add(sheet);
            scroll.Canvas.Layout.AddStretchCell();
        }

        [EditorEvent.Hotload]
        public void RebuildSheet() {
            sheet.Clear(true);
            if (editor.SelectedEvent != null) {
                var obj = editor.SelectedEvent.GetSerialized();
                obj.OnPropertyChanged += OnPropertyChanged;
                obj.OnPropertyStartEdit += OnPropertyStartChange;
                obj.OnPropertyFinishEdit += OnPropertyFinishedChange;
                sheet.AddObject(obj);
            }
        }

        private void OnPropertyChanged(SerializedProperty property) {
            editor.SelectedEventModified();
        }

        private void OnPropertyStartChange(SerializedProperty property) {
            valueCache = property.GetValue<object>();
        }
        
        private void OnPropertyFinishedChange(SerializedProperty property) {
            if (property.GetValue<object>() != valueCache) {
                editor.AttackModified();
            }
        }
    }
}
