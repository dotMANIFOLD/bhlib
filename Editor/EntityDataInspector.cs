using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;
using Application = Editor.Application;

namespace MANIFOLD.BHLib.Editor {
    [Inspector(typeof(EntityData))]
    public class EntityDataInspector : InspectorWidget {
        public class ComponentView : Widget {
            private readonly ComponentDefinition component;

            private InspectorHeader header;
            private ControlSheet controlSheet;

            public Action OnPropertyChanged;
            public Action<ComponentDefinition> OnDeleteRequested;
            
            public ComponentView(ComponentDefinition component, Widget parent = null) : base(parent) {
                this.component = component;
                
                Layout = Layout.Column();
                header = Layout.Add(new InspectorHeader());
                
                var type = EditorTypeLibrary.GetType(component.GetType());
                header.Title = type.Title;
                header.Icon = type.Icon;
                header.Color = Theme.Primary;
                header.IsExpanded = true;
                header.ContextMenu = BuildMenu;
                header.BuildUI();
                
                controlSheet = new ControlSheet();
                Layout.Add(controlSheet);
                
                RebuildSheet();
            }

            [EditorEvent.Hotload]
            private void RebuildSheet() {
                controlSheet.Clear(true);
                
                var so = component.GetSerialized();
                controlSheet.AddObject(so);
                so.OnPropertyChanged = PropertyChange;
            }

            private void BuildMenu(Menu menu) {
                menu.AddOption("Remove", "remove", RequestDelete);
            }

            private void PropertyChange(SerializedProperty prop) {
                OnPropertyChanged?.Invoke();
            }
            
            private void RequestDelete() {
                OnDeleteRequested?.Invoke(component);
            }
        }
        
        private EntityData target;

        public EntityDataInspector(SerializedObject so) : base(so) {
            Layout = Layout.Column();
            Layout.Margin = 8;

            if (so.Targets.FirstOrDefault() is not EntityData resource) return;
            target = resource;
            
            CreateUI();
        }

        private void CreateUI() {
            Layout.Clear(true);
            
            Layout.Add(new Label($"Component count: {target.Components.Count}"));
            Layout.AddSpacingCell(20);
            
            foreach (var component in target.Components) {
                var view = Layout.Add(new ComponentView(component, this));
                view.OnDeleteRequested = RemoveComponent;
                view.OnPropertyChanged = ComponentPropertyChanged;
            }
            
            Layout.AddSpacingCell(20);
            var finalRow = Layout.AddRow();
            finalRow.AddStretchCell();
            finalRow.Add(new Button.Primary("Add Component", "add") { Clicked = NewComponentPopup });
            finalRow.AddStretchCell();
        }

        private void NewComponentPopup() {
            Menu menu = new Menu();
            menu.IsPopup = true;

            foreach (var type in GetAllComponentTypes()) {
                Menu localMenu;
                if (!string.IsNullOrEmpty(type.Group)) localMenu = menu.FindOrCreateMenu(type.Group);
                else localMenu = menu;
                
                var option = localMenu.AddOption(type.Title, type.Icon);
                option.ToolTip = type.Description;
                option.Triggered = () => {
                    AddNewComponent(type);
                };
            }
            
            menu.Position = Application.CursorPosition;
            menu.Show();
        }

        private void AddNewComponent(TypeDescription type) {
            var component = type.Create<ComponentDefinition>();
            target.Components.Add(component);
            target.StateHasChanged();

            CreateUI();
        }

        private void RemoveComponent(ComponentDefinition component) {
            target.Components.Remove(component);
            target.StateHasChanged();
            CreateUI();
        }

        private void ComponentPropertyChanged() {
            target.StateHasChanged();
        }
        
        private IEnumerable<TypeDescription> GetAllComponentTypes() {
            return EditorTypeLibrary.GetTypes<ComponentDefinition>().Where(x => !x.IsAbstract);
        }
    }
}
