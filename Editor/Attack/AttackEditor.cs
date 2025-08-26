using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using MANIFOLD.BHLib;
using MANIFOLD.BHLib.Events;
using Sandbox;
using Sandbox.UI;
using Application = Editor.Application;
using Button = Editor.Button;
using Checkbox = Editor.Checkbox;
using ControlSheet = Editor.ControlSheet;
using Label = Editor.Label;

namespace MANIFOLD.BHLib.Editor {
    [Dock("Editor", "Attack Editor", "sports_martial_arts")]
    public class AttackEditor : Widget, EditorEvent.ISceneView {
        public class TimelineInfo {
            public string name;
            public int index;
            public ITimeline timeline;
        }
        
        // UI
        private ControlBar controlBar;
        private TimelinePanel timelinePanel;
        private InspectPanel inspectPanel;
        
        // ATTACK
        private AttackCaster caster;
        private AttackData selectedAttack;
        
        private TimelineInfo[] availableTimelines;
        private ITimeline selectedTimeline;
        private int? selectedTimelineIndex;
        
        private AttackEvent selectedEvent;
        private Guid? selectedEventId;

        private bool isPlaying;
        private float currentTime;
        private bool autosave = false;

        public AttackCaster Caster => caster;

        public AttackData SelectedAttack {
            get => selectedAttack;
            set {
                bool reloadPreview = value != selectedAttack && caster.InPreviewMode;
                if (reloadPreview) {
                    caster.StopPreview();
                }
                
                selectedAttack = value;
                OnAttackChanged();
                
                if (reloadPreview) {
                    caster.StartPreview(selectedAttack);
                }
            }
        }
        
        public IReadOnlyList<TimelineInfo> AvailableTimelines => availableTimelines;
        public ITimeline SelectedTimeline {
            get => selectedTimeline;
            set {
                if (selectedTimeline == value) return;
                selectedTimeline = value;
                selectedTimelineIndex = availableTimelines.First(x => x.timeline == value).index;

                SelectedEvent = null;
                timelinePanel.OnTimelineSelected();
            }
        }

        public AttackEvent SelectedEvent {
            get => selectedEvent;
            set {
                selectedEvent = value;
                selectedEventId = value?.ID;
                inspectPanel.RebuildSheet();
            }
        }

        public bool IsPlaying {
            get => isPlaying;
            set {
                isPlaying = value;
            }
        }
        public float CurrentTime {
            get => currentTime;
            set {
                currentTime = value;
                
                if (InPreview) {
                    caster.ResimulatePreview(currentTime);
                }
                timelinePanel.OnTimeChanged();
            }
        }
        public bool InPreview {
            get => caster.InPreviewMode;
            set {
                if (value == caster.InPreviewMode) return;
                
                if (value) caster.StartPreview(selectedAttack);
                else caster.StopPreview();
            }
        }

        public bool Autosave {
            get => autosave;
            set => autosave = value;
        }

        public AttackEditor(Widget parent) : base(parent) {
            Layout = Layout.Column();
            Layout.Margin = 8;
            Layout.Spacing = 8;
            
            EditorUtility.OnInspect += OnObjectInspect;
        }

        public override void OnDestroyed() {
            EditorUtility.OnInspect -= OnObjectInspect;
        }
        
        public void DrawGizmos(Scene scene) {
            if (caster == null) return;
            if (selectedEvent is not IDrawGizmos casted) return;
            
            using (Gizmo.Scope("AttackEditor", caster.WorldTransform)) {
                casted.DrawGizmos();
            }
        }

        [EditorEvent.Frame]
        private void OnFrame() {
            if (IsPlaying) {
                CurrentTime += RealTime.Delta;
            }
        }
        
        private void OnObjectInspect(EditorUtility.OnInspectArgs args) {
            if (caster.IsValid() && caster.InPreviewMode) {
                caster.StopPreview();
            }
            
            if (args.Object == null) return;
            
            object obj;
            if (args.Object is Array arr) {
                obj = arr.Length > 0 ? arr.GetValue(0) : null;
            } else {
                obj = args.Object;
            }
            caster = (obj as GameObject)?.GetComponent<AttackCaster>();

            if (caster.IsValid() && caster.Attacks.Count > 0) {
                RegularSession();
                SelectedAttack = caster.Attacks[0];
            } else {
                StartHelper();
            }
        }

        // UI
        private void StartHelper() {
            Layout.Clear(true);
            Layout.Add(new Label("you need an attack caster component lol\n(it also needs at least one attack)"));
        }

        private void RegularSession() {
            Layout.Clear(true);
            controlBar = Layout.Add(new ControlBar(this, this));
            timelinePanel = new TimelinePanel(this, this);
            inspectPanel = new InspectPanel(this, this);
            
            var splitter = Layout.Add(new Splitter(this));
            splitter.AddWidget(timelinePanel);
            splitter.AddWidget(inspectPanel);
            
            splitter.SetCollapsible(0, false);
            splitter.SetStretch(0, 1);
            splitter.SetCollapsible(1, false);
            splitter.SetStretch(1, 1);
        }

        // EVENT OPERATIONS
        public void AddNewEvent() {
            var popup = new Dialog();
                popup.Window.Title = "Create New Event";
                popup.Window.Size = new Vector2(400, 100);
                
                // var popup = new PopupWidget(null);
                popup.Layout = Layout.Column();
                popup.Layout.Margin = 16;
                popup.Layout.Spacing = 8;
            
                // popup.Layout.Add(new Label("Create a new event?") { VerticalSizeMode = SizeMode.CanShrink });
                var lineEdit = popup.Layout.Add(new LineEdit() { PlaceholderText = "Event name"});

                var types = EditorTypeLibrary.GetTypes(selectedTimeline.EventType).Where(x => !x.IsAbstract).ToArray();
                var typeCombo = popup.Layout.Add(new ComboBox());
                foreach (var type in types) {
                    typeCombo.AddItem(type.Name);
                }

                var bottomRow = popup.Layout.AddRow();
                bottomRow.AddStretchCell();
                bottomRow.Add(new Button.Primary("Confirm") {
                    Clicked = () => {
                        if (string.IsNullOrWhiteSpace(lineEdit.Text)) {
                            Log.Error("Invalid event name");
                            return;
                        }

                        AttackEvent evt = types[typeCombo.CurrentIndex].Create<AttackEvent>();
                        evt.Name = lineEdit.Text;
                        evt.Time = currentTime;
                        selectedTimeline.AddEvent(evt);

                        if (evt is IModifier modifier) {
                            modifier.OnAdd(selectedAttack);
                        }
                        
                        // timelineGraphic.RebuildMarkers();
                        
                        AttackModified();
                        popup.Window.Destroy();
                    }
                });

                popup.Position = Application.CursorPosition;
                popup.ConstrainToScreen();
                popup.Show();
        }
        
        public void RemoveSelectedEvent() {
            if (SelectedEvent == null) return;

            if (SelectedEvent is IModifier modifier) {
                modifier.OnRemove(selectedAttack);
            }
            
            SelectedTimeline.RemoveEvent(SelectedEvent);
            SelectedEvent = null;
            
            AttackModified();
        }

        public void SelectedEventModified() {
            if (selectedEvent is IModifier modifier) {
                modifier.Modify(selectedAttack);
            }
            
            AttackModified(false);

            if (caster.InPreviewMode) {
                bool resimulate = false;
                if (selectedEvent is SpawnEntity spawnEvt) {
                    caster.RebuildEntity(spawnEvt);
                    resimulate = true;
                } else if (selectedEvent is PatternEvent patternEvt) {
                    caster.RebuildPreviewEntities(true); // rebuild everything just in case
                    resimulate = true;
                }

                if (resimulate) caster.ResimulatePreview(currentTime);
            }
        }
        
        // SAVING
        public void SaveAttack() {
            foreach (var item in availableTimelines) {
                item.timeline.Sort();
            }

            float longestTime = 0;
            foreach (var item in availableTimelines) {
                longestTime = MathF.Max(longestTime, item.timeline.Events[^1].Time);
            }
            selectedAttack.CalculatedDuration = longestTime;

            AttackData reloadedData;
            if (!selectedAttack.EmbeddedResource.HasValue) {
                var asset = AssetSystem.FindByPath(selectedAttack.ResourcePath);
                asset.SaveToDisk(selectedAttack);

                reloadedData = asset.LoadResource<AttackData>();
            } else {
                Log.Warning("Resource is embedded! Can't save directly.");
                reloadedData = SelectedAttack;
            }
            
            // RELOAD ALL THE UI BECAUSE FUCK IT
            // SAVING BREAKS A BUNCH OF REFERENCES FOR SOME REASON
            var indexCopy = selectedTimelineIndex.Value;
            var idCopy = selectedEventId;
            
            RegularSession();
            
            SelectedAttack = reloadedData;
            SelectedTimeline = availableTimelines[indexCopy].timeline;
            if (idCopy.HasValue) {
                SelectedEvent = selectedTimeline.GetEvent(idCopy.Value);
            }
            
            timelinePanel.OnTimelineSelected();
        }
        
        // CALLBACKS
        public void AttackModified(bool allowSave = true) {
            selectedAttack.StateHasChanged();
            if (allowSave && autosave) {
                SaveAttack();
            }
            timelinePanel.OnAttackModified();
        }

        private void OnAttackChanged() {
            availableTimelines = selectedAttack.GetType().GetProperties()
                .Where(x => x.PropertyType.IsAssignableTo(typeof(ITimeline)))
                .Index()
                .Select(x => new TimelineInfo() {
                    name = x.Item.Name,
                    index = x.Index,
                    timeline = (ITimeline)x.Item.GetValue(selectedAttack)
                }).ToArray();

            SelectedTimeline = availableTimelines[0].timeline;
            
            controlBar.OnAttackSelected();
            timelinePanel.OnAttackSelected();
        }
    }
}
