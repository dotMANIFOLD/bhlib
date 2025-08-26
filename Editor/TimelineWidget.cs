using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.BHLib.Editor {
    public class TimelineWidget : GraphicsView {
        public class EventMarker : GraphicsItem {
            public float time;
            public List<AttackEvent> events;
            public bool selected;
            public int selectedIndex;
            public Action<EventMarker> onSelected;
            
            private TimelineWidget timeline;

            public EventMarker(TimelineWidget widget) {
                timeline = widget;
                events = new List<AttackEvent>();
                ZIndex = 0;
                HoverEvents = true;
            }

            protected override void OnMousePressed(GraphicsMouseEvent e) {
                if (e.LeftMouseButton) {
                    if (!selected) selectedIndex = 0;
                    else {
                        selectedIndex++;
                        if (selectedIndex >= events.Count) selectedIndex = 0;
                    }
                    selected = true;
                    Update();
                    onSelected?.Invoke(this);
                    e.Accepted = true;
                }
            }

            protected override void OnPaint() {
                base.OnPaint();
                
                Paint.Antialiasing = false;

                Color unselectedColor = Color.White.Darken(0.05f);
                Color color;
                if (selected) color = Color.Orange;
                else if (Paint.HasMouseOver) color = Gizmo.Colors.Hovered;
                else color = unselectedColor;
                Paint.SetPen(color, 2);
                
                var rect = LocalRect.Shrink(2);
                Paint.DrawLine(new Vector2(rect.Left, rect.Bottom), new Vector2(rect.Left, rect.Top));
                // Paint.DrawText(rect.Shrink(4, 0, 0, 6), string.Join("\n", events.Select(x => x.Name)), TextFlag.LeftBottom);

                rect = rect.Shrink(4, 0, 0, 6);
                for (int i = 0; i < events.Count; i++) {
                    Color textColor;
                    if (selected) {
                        textColor = i == selectedIndex ? color : unselectedColor;
                    } else {
                        textColor = color;
                    }
                    
                    Paint.SetPen(textColor);
                    Paint.DrawText(rect, events[i].Name, TextFlag.LeftBottom);
                    rect = rect.Shrink(0, 0, 0, 9);
                }
            }
        }
        
        public class TimeAxis : GraphicsItem {
            private TimelineWidget timeline;

            public TimeAxis(TimelineWidget widget) {
                timeline = widget;
                ZIndex = 10;
                HoverEvents = true;
            }

            protected override void OnPaint() {
                base.OnPaint();

                Paint.Antialiasing = false;
                Paint.ClearPen();
                Paint.SetBrush(Theme.ControlBackground);
                Paint.DrawRect(LocalRect);
                
                Paint.SetDefaultFont(7);

                var rect = LocalRect.Shrink(1);
                var zoom = timeline.ZoomFactor;
                var spacing = 100 * zoom;
                var lines = rect.Width / spacing;
                var w = spacing;
                var subdivisions = (int)(3 * zoom);
                var subLineSpacing = w / subdivisions;

                for (int i = 0; i < lines; i++) {
                    float xPos = rect.Left + w * i;
                    
                    Paint.SetPen(Theme.Text.WithAlpha(0.5f));
                    Paint.DrawLine(new Vector2(xPos, rect.Bottom), new Vector2(xPos, rect.Bottom - 8));
                    Paint.DrawText(new Vector2(xPos, rect.Top), $"{i}");
                    Paint.SetPen(Theme.Text.WithAlpha(0.2f));

                    for (int j = 0; j < subdivisions; j++) {
                        var sublineX = w * i + subLineSpacing * j;
                        Paint.DrawLine(new Vector2(rect.Left + sublineX, rect.Bottom), new Vector2(rect.Left + sublineX, rect.Bottom - 4));
                    }
                }
            }

            protected override void OnMousePressed(GraphicsMouseEvent e) {
                base.OnMousePressed(e);

                if (e.LeftMouseButton) {
                    timeline.Time = timeline.TimeFromPosition(e.LocalPosition.x);
                }
            }
        }

        public class Scrubber : GraphicsItem {
            private TimelineWidget timeline;

            public Scrubber(TimelineWidget widget) {
                timeline = widget;
                ZIndex = 20;
                HoverEvents = true;
                Cursor = CursorShape.SizeH;
                Movable = true;
                Selectable = true;
            }

            protected override void OnPaint() {
                base.OnPaint();

                Paint.Antialiasing = false;
                Paint.ClearPen();
                Paint.SetBrush(Theme.Green.WithAlpha(0.7f));
                Paint.DrawRect(new Rect(0, new Vector2(LocalRect.Width, Theme.RowHeight + 1)));
                Paint.SetPen(Theme.Green.WithAlpha(0.7f));
                Paint.DrawLine(new Vector2(4, Theme.RowHeight + 1), new Vector2(4, LocalRect.Bottom));
            }

            protected override void OnMoved() {
                base.OnMoved();

                timeline.ScrubTo(timeline.TimeFromPosition(Position.x));
                
                Position = Position.WithY(0);
                Position = Position.WithX(MathF.Max(-4, Position.x));
            }
        }
        
        private ITimeline timeline;
        private float time;
        private float zoomFactor;
        private bool showHidden;
        
        private TimeAxis timeAxis;
        private Scrubber scrubber;
        private List<EventMarker> markers;
        private Dictionary<AttackEvent, EventMarker> eventToMarker;
        private EventMarker selectedMarker;

        public ITimeline Timeline {
            get => timeline;
            set {
                timeline = value;
                RebuildMarkers();
                DoLayout();
            }
        }
        
        public float Range { get; set; }

        public float ZoomFactor {
            get => zoomFactor;
            set {
                zoomFactor = value;
                DoLayout();
                timeAxis.Update();
                scrubber.Update();
            }
        }

        public float Time {
            get => time;
            set {
                time = value;
                DoLayout();
            }
        }

        public bool ShowHidden {
            get => showHidden;
            set {
                showHidden = value;
                RebuildMarkers();
            }
        }
        
        public AttackEvent SelectedEvent => selectedMarker?.events[selectedMarker.selectedIndex];
        
        public Action<AttackEvent> OnEventSelected { get; set; }
        public Action OnTimeScrubbed { get; set; }
        
        public TimelineWidget(Widget parent = null) : base(parent) {
            Antialiasing = false;
            BilinearFiltering = false;
            
            SceneRect = new Rect(0, Size);
            HorizontalScrollbar = ScrollbarMode.Auto;
            VerticalScrollbar = ScrollbarMode.Off;
            MouseTracking = true;
            
            Scale = 1;
            zoomFactor = 1;
            
            timeAxis = new TimeAxis(this);
            Add(timeAxis);
            scrubber = new Scrubber(this);
            Add(scrubber);

            markers = new List<EventMarker>();
            eventToMarker = new Dictionary<AttackEvent, EventMarker>();
        }

        protected override void DoLayout() {
            base.DoLayout();

            var size = Size;
            size.x = MathF.Max(size.x, PositionFromTime(Range + 3));
            SceneRect = new Rect(0, size);
            timeAxis.Size = new Vector2(size.x, Theme.RowHeight);
            scrubber.Size = new Vector2(9, size.y);

            var rect = SceneRect;
            rect.Top = timeAxis.SceneRect.Bottom;

            scrubber.Position = scrubber.Position.WithX(PositionFromTime(Time) - 3).SnapToGrid(1f);
            
            foreach (var marker in markers) {
                var markerRect = rect;
                markerRect.Left = PositionFromTime(marker.time);
                markerRect.Width = 80;
                marker.SceneRect = markerRect;
            }
        }
        
        protected override void OnWheel(WheelEvent e) {
            e.Accept();
        }
        
        public void RebuildMarkers() {
            var previousSelectedEvent = SelectedEvent;
            
            foreach (var marker in markers) {
                marker.Destroy();
            }
            markers.Clear();
            eventToMarker.Clear();

            if (timeline != null) {
                foreach (var evt in Timeline.Events) {
                    if (!ShowHidden && evt.Hidden) continue;
                    AddEvent(evt);
                }
            }
            DoLayout();

            if (previousSelectedEvent != null) {
                if (eventToMarker.TryGetValue(previousSelectedEvent, out EventMarker marker)) {
                    marker.selected = true;
                    marker.selectedIndex = marker.events.IndexOf(previousSelectedEvent);
                    try {
                        marker.Update();
                    } catch {
                        // ignored
                    }
                }
            }
        }

        public void ScrubTo(float time) {
            Time = time;
            OnTimeScrubbed?.Invoke();
        }
        
        public float PositionFromTime(float time) {
            return 100 * ZoomFactor * time;
        }

        public float TimeFromPosition(float position) {
            return (ZoomFactor / 100) * position;
        }

        private void AddEvent(AttackEvent evt) {
            var existing = markers.FirstOrDefault(x => x.time.AlmostEqual(evt.Time));
            if (existing != null) {
                existing.events.Add(evt);
                eventToMarker.Add(evt, existing);
            } else {
                EventMarker marker = new EventMarker(this);
                marker.time = evt.Time;
                marker.events.Add(evt);
                marker.onSelected = OnMarkerSelected;
                Add(marker);
                
                markers.Add(marker);
                eventToMarker.Add(evt, marker);
            }
        }

        private void OnMarkerSelected(EventMarker marker) {
            if (marker != selectedMarker && selectedMarker != null) {
                selectedMarker.selected = false;
                try {
                    selectedMarker.Update();
                } catch {
                    // ignored
                }
            }
            selectedMarker = marker;
            OnEventSelected?.Invoke(SelectedEvent);
        }
    }
}
