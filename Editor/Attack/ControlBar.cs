using System;
using Editor;
using Sandbox.UI;
using Button = Editor.Button;
using Checkbox = Editor.Checkbox;

namespace MANIFOLD.BHLib.Editor {
    public class ControlBar : Widget {
        private readonly AttackEditor editor;
        private ComboBox attackSelector;
        private Checkbox previewSwitch;
        private Checkbox autosaveSwitch;

        public ControlBar(AttackEditor editor, Widget parent = null) : base(parent) {
            this.editor = editor;

            VerticalSizeMode = SizeMode.CanShrink;

            Layout = Layout.Row();
            Layout.Margin = new Margin(0, 0, 0, 8);
            Layout.Spacing = 8;

            attackSelector = Layout.Add(new ComboBox());
            attackSelector.MinimumWidth = 300;
            Layout.AddSeparator();
            previewSwitch = Layout.Add(new Checkbox("Preview"));
            previewSwitch.Value = editor.Caster.InPreviewMode;
            previewSwitch.StateChanged += OnTogglePreview;
            Layout.Add(new Button("Rebuild") { Clicked = RebuildPreview });

            Layout.AddStretchCell();
            Layout.Add(new Button("Save") { Clicked = editor.SaveAttack });
            autosaveSwitch = Layout.Add(new Checkbox("Autosave"));
            autosaveSwitch.Value = editor.Autosave;
            autosaveSwitch.StateChanged += OnToggleAutosave;
            autosaveSwitch.ToolTip = "Autosave feature, useful but may interrupt work in certain cases. Sorry!";
        }

        public void OnAttackSelected() {
            attackSelector.Clear();
            for (int i = 0; i < editor.Caster.Attacks.Count; i++) {
                var attackData = editor.Caster.Attacks[i];
                var selected = editor.SelectedAttack == attackData;
                var name = attackData.EmbeddedResource.HasValue ? "Embedded Attack" : attackData.ResourceName;
                attackSelector.AddItem($"[{i}] {name}", selected: selected,
                    onSelected: () => { editor.SelectedAttack = attackData; });
            }
        }

        private void OnTogglePreview(CheckState state) {
            editor.InPreview = state == CheckState.On;
        }
        
        private void RebuildPreview() {
            if (!editor.Caster.InPreviewMode) return;
            editor.Caster.RebuildPreviewEntities(true);
        }

        private void OnToggleAutosave(CheckState state) {
            editor.Autosave = state == CheckState.On;
        }
    }
}
