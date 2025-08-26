using System.Linq;
using Editor;

namespace MANIFOLD.BHLib.Editor {
    public class TimelinePanel : Widget {
        private readonly AttackEditor editor;
        private ComboBox timelineSelector;
        private Label timeLabel;
        private IconButton playButton;
        private IconButton removeEventButton;
        private TimelineWidget timelineGraphic;

        public TimelinePanel(AttackEditor editor, Widget parent = null) : base(parent) {
            this.editor = editor;

            Layout = Layout.Column();
            Layout.Margin = 8;
            Layout.Spacing = 4;

            var row = Layout.AddRow();
            row.Spacing = 4;

            timelineSelector = row.Add(new ComboBox());
            timelineSelector.MinimumWidth = 200;
            timelineSelector.ToolTip = "Timeline selector";

            // ZOOM
            row.Add(new IconButton("zoom_in") { ToolTip = "Zoom In", OnClick = () => timelineGraphic.ZoomFactor *= 1.5f });
            row.Add(new IconButton("zoom_out") { ToolTip = "Zoom Out", OnClick = () => timelineGraphic.ZoomFactor /= 1.5f });
            row.AddStretchCell();

            // PLAYBACK
            timeLabel = row.Add(new Label("0.00") { FixedWidth = 40 });
            row.Add(new IconButton("first_page") { ToolTip = "Play", OnClick = BackToStart });
            playButton = row.Add(new IconButton("play_arrow") { ToolTip = "Back to start", OnClick = OnPlayChanged });
            row.AddStretchCell();

            // EVENT MANIPULATION
            row.Add(new IconButton("add_location_alt") { ToolTip = "Add Event", OnClick = editor.AddNewEvent });
            removeEventButton = row.Add(new IconButton("wrong_location") { ToolTip = "Remove Event", OnClick = editor.RemoveSelectedEvent, Enabled = false });

            // MENU
            row.AddSpacingCell(6);
            row.AddSeparator(4, Theme.TextButton);
            row.AddSpacingCell(6);
            row.Add(new IconButton("menu") { OnClick = CreateMenuPopup });

            // QUANTIZE
            // row.Add(new Label("1/1"));
            // row.Add(new IconButton("keyboard_double_arrow_up") { ToolTip = "Double measurement" });
            // row.Add(new IconButton("keyboard_double_arrow_down") { ToolTip = "Half measurement" });
            // row.AddSpacingCell(20);
            // row.Add(new IconButton("chevron_left") { ToolTip = "Nudge Left" });
            // row.Add(new IconButton("chevron_right") { ToolTip = "Nudge Right" });
            // row.Add(new IconButton("straighten") { ToolTip = "Quantize" });

            timelineGraphic = Layout.Add(new TimelineWidget(this));
            timelineGraphic.Range = 10;
            timelineGraphic.OnEventSelected = OnEventSelectedInternal;
            timelineGraphic.OnTimeScrubbed = OnTimeScrubbed;
        }

        protected override void OnPaint() {
            Paint.SetBrushAndPen(Theme.Base);
            Paint.DrawRect(LocalRect);
        }
        
        // OPERATIONS
        private void BackToStart() {
            editor.CurrentTime = 0;
        }
        
        // POPUPS
        private void CreateMenuPopup() {
            Menu menu = new Menu();
            menu.IsPopup = true;

            var hiddenOption = menu.AddOption("Show Hidden");
            hiddenOption.Icon = "location_off";
            hiddenOption.Checkable = true;
            hiddenOption.Checked = timelineGraphic.ShowHidden;
            hiddenOption.Triggered = OnShowHidden;

            menu.Position = Application.CursorPosition;
            menu.Show();
        }
        
        // MAIN CALLBACKS
        public void OnAttackSelected() {
            timelineSelector.Clear();
            foreach (var item in editor.AvailableTimelines) {
                var selected = editor.SelectedTimeline == item.timeline;
                var func = () => {
                    editor.SelectedTimeline = item.timeline;
                    timelineGraphic.Timeline = item.timeline;
                };

                timelineSelector.AddItem(item.name, selected: selected, onSelected: func);
            }
        }

        public void OnTimelineSelected() {
            timelineGraphic.Timeline = editor.SelectedTimeline;
        }

        public void OnAttackModified() {
            timelineGraphic.RebuildMarkers();
        }

        public void OnTimeChanged() {
            timelineGraphic.Time = editor.CurrentTime;
            timeLabel.Text = editor.CurrentTime.ToString("N2");
        }
        
        // CALLBACKS
        private void OnEventSelectedInternal(AttackEvent evt) {
            editor.SelectedEvent = evt;
            removeEventButton.Enabled = evt != null;
        }

        private void OnPlayChanged() {
            editor.IsPlaying = !editor.IsPlaying;
            playButton.Icon = editor.IsPlaying ? "stop" : "play_arrow";
        }

        private void OnTimeScrubbed() {
            editor.CurrentTime = timelineGraphic.Time;
        }

        private void OnShowHidden() {
            timelineGraphic.ShowHidden = !timelineGraphic.ShowHidden;
        }
    }
}
