using ModularPanels.DrawLib;
using ModularPanels.TrackLib;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels
{
    public class ScrollMap(Layout layout, DrawPanel mapPanel, DrawPanel mainPanel, Control window, ScrollMap.MapStyle? style = null)
    {
        [JsonConverter(typeof(MapStyleJsonConverter))]
        public struct MapStyle
        {
            public Color colorHighlight = Color.FloralWhite;
            public Color colorBackground = Color.Gainsboro;
            public Color colorViewBorder = Color.SteelBlue;
            public Color colorTrack = Color.Black;
            public Color colorTrackOccupied = Color.Magenta;
            public Color colorModuleBorder = Color.Black;

            public float borderWidth = 1f;
            public float viewBorderWidth = 2f;
            public float trackWidth = 2f;
            public float trackWidthOccupied = 2.5f;

            public MapStyle() { }
        }

        private struct MapStyleJsonData
        {
            public ColorJS? ColorBackground { get; set; }
            public ColorJS? ColorViewBackground { get; set; }
            public ColorJS? ColorViewBorder { get; set; }
            public ColorJS? ColorModuleBorder { get; set; }
            public ColorJS? ColorTrack { get; set; }
            public ColorJS? ColorTrackOccupied { get; set; }
            public float? BorderWidth { get; set; }
            public float? ViewBorderWidth { get; set; }
            public float? TrackWidth { get; set; }
            public float? TrackWidthOccupied { get; set; }
        }

        private class MapStyleJsonConverter : JsonConverter<MapStyle>
        {
            public override MapStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                MapStyleJsonData? data = JsonSerializer.Deserialize<MapStyleJsonData>(ref reader, options);
                MapStyle style = new();
                if (data == null)
                    return style;

                if (data.Value.ColorTrack != null)
                    style.colorTrack = data.Value.ColorTrack;

                if (data.Value.ColorTrackOccupied != null)
                    style.colorTrackOccupied = data.Value.ColorTrackOccupied;

                if (data.Value.ColorBackground != null)
                    style.colorBackground = data.Value.ColorBackground;

                if (data.Value.ColorModuleBorder != null)
                    style.colorModuleBorder = data.Value.ColorModuleBorder;

                if (data.Value.ColorViewBackground != null)
                    style.colorHighlight = data.Value.ColorViewBackground;

                if (data.Value.ColorViewBorder != null)
                    style.colorViewBorder = data.Value.ColorViewBorder;

                if (data.Value.BorderWidth.HasValue)
                    style.borderWidth = data.Value.BorderWidth.Value;

                if (data.Value.ViewBorderWidth.HasValue)
                    style.viewBorderWidth = data.Value.ViewBorderWidth.Value;

                if (data.Value.TrackWidth.HasValue)
                    style.trackWidth = data.Value.TrackWidth.Value;

                if (data.Value.TrackWidthOccupied.HasValue)
                    style.trackWidthOccupied = data.Value.TrackWidthOccupied.Value;

                return style;
            }

            public override void Write(Utf8JsonWriter writer, MapStyle value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        readonly Layout _layout = layout;
        readonly DrawPanel _mapPanel = mapPanel;
        readonly DrawPanel _mainPanel = mainPanel;
        readonly Control _window = window;
        readonly MapStyle _style = style ?? new();

        int _length = 0;
        int _moduleMaxH = 0;
        float _scale = 1f;
        float _leftEdge = 0;
        float _width = 1f;
        float _viewWidth = 1f;
        int _panelOffsetY = 0;

        bool _mouseHold = false;

        const int _borderH = 5;
        const int _maxMapH = 50;

        public void Init()
        {
            foreach (Module mod in _layout.Modules)
            {
                _length += Grid.Instance.Scale(mod.Width);
                _moduleMaxH = Math.Max(_moduleMaxH, Grid.Instance.Scale(mod.Height));

                foreach (TrackDetector det in mod.TrackDetectors.Values)
                {
                    det.StateChangedEvents += (obj, e) =>
                    {
                        _mapPanel.Invalidate();
                    };
                }
            }

            // Set background color
            _mapPanel.BackColor = _style.colorBackground;

            _panelOffsetY = _window.Height - _mapPanel.Bottom;

            _mapPanel.MouseDown += OnMouseDown;
            _mapPanel.MouseUp += OnMouseUp;
            _mapPanel.MouseMove += OnMouseMove;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _mouseHold = true;
            CenterView(e.X);
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _mouseHold = false;
            CenterView(e.X);
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (_mouseHold)
                CenterView(e.X);
        }

        private void CenterView(float posX)
        {
            float newEdge = posX - _leftEdge - _viewWidth / 2f;
            if (newEdge < 0)
                newEdge = 0;

            if (newEdge > _width - _viewWidth)
                newEdge = _width - _viewWidth;

            float relPos = newEdge / (_width - _viewWidth);

            _mainPanel.ScrollTo(relPos);
        }

        private static PointF GetGridPos(GridPos gp, float scale, float offsetX, float offsetY)
        {
            DrawingPos dp = gp.ToDrawingPos();
            float x = offsetX + (dp.x * scale);
            float y = offsetY + (dp.y * scale);
            return new(x, y);
        }

        public void Draw(Graphics g)
        {
            float offsetX = 0;
            float offsetY = 1f;

            _width = _window.Width;
            _scale = _width / _length;
            float h = _moduleMaxH * _scale;
            if (h > _maxMapH)
            {
                h = _maxMapH;
                _scale = h / _moduleMaxH;
                _width = _length * _scale;
                offsetX = (_mapPanel.Width / 2f) - (_width / 2f);
            }
            _leftEdge = offsetX;

            // Adjust panel sizes
            _mapPanel.Size = new() { Height = (int)MathF.Ceiling(h) + 2, Width = _mapPanel.Width };
            _mapPanel.Location = new() { X = _mapPanel.Location.X, Y = _window.Height - _panelOffsetY -  _mapPanel.Height };

            _mainPanel.Size = new() { Width = _mainPanel.Width, Height = _mapPanel.Top - _mainPanel.Top - _borderH };

            Pen borderPen = new(new SolidBrush(_style.colorModuleBorder), _style.borderWidth);
            Pen trackPen = new(new SolidBrush(_style.colorTrack), _style.trackWidth);
            Pen trackOccupiedPen = new(new SolidBrush(_style.colorTrackOccupied), _style.trackWidthOccupied);

            // Get current view
            float viewOffsetLeft = -_mainPanel.Left;
            float viewLeft = _leftEdge + viewOffsetLeft * _scale;
            _viewWidth = _window.Width * _scale;

            RectangleF viewRect = new(viewLeft, offsetY, _viewWidth, h);
            Brush viewBrush = new SolidBrush(_style.colorHighlight);
            g.FillRectangle(viewBrush, viewRect);

            // Draw track
            foreach (Module mod in _layout.Modules)
            {
                float len = Grid.Instance.Scale(mod.Width) * _scale;
                float height = Grid.Instance.Scale(mod.Height) * _scale;

                RectangleF rect = new(offsetX, offsetY, len, height);

                g.DrawRectangle(borderPen, rect);

                foreach (TrackSegment seg in mod.TrackSegments.Values)
                {
                    PointF p1 = GetGridPos(seg.n0.pos, _scale, offsetX, offsetY);
                    PointF p2 = GetGridPos(seg.n1.pos, _scale, offsetX, offsetY);

                    if (seg.IsOccupied())
                        g.DrawLine(trackOccupiedPen, p1, p2);
                    else
                        g.DrawLine(trackPen, p1, p2);
                }

                offsetX += len;
            }

            // Draw view border
            Pen viewPen = new(new SolidBrush(_style.colorViewBorder), _style.viewBorderWidth);
            g.DrawRectangle(viewPen, viewRect);

            borderPen.Dispose();
            trackPen.Dispose();
            trackOccupiedPen.Dispose();
            viewPen.Dispose();
            viewBrush.Dispose();
        }
    }
}
