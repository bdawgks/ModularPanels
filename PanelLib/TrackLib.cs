using System.Drawing;

namespace PanelLib
{
    public class TrackNode(string id, int x, int y)
    {
        public readonly string id = id;
        public readonly int x = x;
        public readonly int y = y;
        public TrackStyle style = new();

        public Point GetPoint(int gridSize = 1)
        {
            return new Point(x * gridSize, y * gridSize);
        }
    }

    public readonly struct TrackSegment(TrackStyle style, TrackNode n0, TrackNode n1)
    {
        public readonly TrackStyle style = style;
        public readonly TrackNode n0 = n0;
        public readonly TrackNode n1 = n1;
    }

    public class TrackPoints(string id, TrackNode baseNode, TrackNode routeNormal, TrackNode routeReversed, bool useBaseColor = false)
    {
        public enum PointsState
        {
            Normal,
            Reversed,
            Moving
        }

        public readonly string id = id;
        public readonly TrackNode baseNode = baseNode;
        public readonly TrackNode routeNormal = routeNormal;
        public readonly TrackNode routeReversed = routeReversed;
        public readonly bool useBaseColor = useBaseColor;

        private PointsStyle _style;

        private bool _showMoving = false;
        private PointsState _state = PointsState.Normal;
        private bool _locked = false;

        public PointsStyle Style
        {
            get => _style;
        }

        public bool ShowMoving
        {
            get => _showMoving;
            set => _showMoving = value;
        }

        public bool IsReversed
        {
            get { return _state == PointsState.Reversed; }
        }
        public bool IsNormal
        {
            get { return _state == PointsState.Normal; }
        }

        public PointsState State
        {
            get { return _state; }
        }

        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        public void SetState(PointsState state)
        {
            _state = state;
        }
    }

    public class TrackDetector
    {
        readonly string _id;
        readonly HashSet<TrackNode> _nodes = [];
        readonly Color _color;

        private bool _isOccupied;

        public TrackDetector(string id, Color color)
        {
            _id = id;
            _color = color;
        }

        public string ID
        {
            get => _id;
        }

        public Color Color
        {
            get => _color;
        }

        public bool IsOccupied
        {
            get => _isOccupied;
            set => _isOccupied = value;
        }

        public bool AddNode(TrackNode node) 
        {
            if (_nodes.Contains(node))
                return false;

            _nodes.Add(node);
            return true;
        }

        public List<TrackNode> GetNodes()
        {
            return [.. _nodes];
        }
    }
}
