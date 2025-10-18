using ModularPanels.DrawLib;

namespace ModularPanels.TrackLib
{
    public class TrackNode(string id, GridPos pos)
    {
        public readonly string id = id;
        public readonly GridPos pos = pos;
        public TrackStyle style = new();
        public bool squareEnd = false;
        public float segDir = 0f;
    }

    public class TrackSegment(string id, TrackStyle style, TrackNode n0, TrackNode n1)
    {
        public readonly string id = id;
        public readonly TrackStyle style = style;
        public readonly TrackNode n0 = n0;
        public readonly TrackNode n1 = n1;
    }

    public class TrackPoints(string id, TrackNode baseNode, TrackNode routeNormal, TrackNode routeReversed, bool useBaseColor = false)
    {
        public class PointsStateChangeArgs(TrackPoints points, PointsState newState) : EventArgs
        {
            public TrackPoints Points { get; } = points;
            public PointsState NewState { get; } = newState;
        }

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

        private PointsStyle _style = new();

        private bool _showMoving = false;
        private PointsState _state = PointsState.Normal;
        private bool _locked = false;

        public event EventHandler<PointsStateChangeArgs>? StateChangeEvents;

        public PointsStyle Style
        {
            get => _style;
            set => _style = value;
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
            if (_state == state)
                return;

            _state = state;
            StateChangeEvents?.Invoke(this, new PointsStateChangeArgs(this, state));
        }
    }

    public class TrackDetector
    {
        public class DetectorStateArgs(bool occupied) : EventArgs 
        {
            public bool IsOccupied { get; } = occupied;
        }

        readonly string _id;
        readonly HashSet<TrackSegment> _segments = [];

        DetectorStyle _style = new DetectorStyleRectangle();
        bool _isOccupied;

        public event EventHandler<DetectorStateArgs>? StateChangedEvents;

        public TrackDetector(string id)
        {
            _id = id;
        }

        public string ID
        {
            get => _id;
        }

        public DetectorStyle Style
        {
            get => _style;
            set => _style = value;
        }

        public bool IsOccupied
        {
            get => _isOccupied;
            set => SetOccupied(value);
        }

        public void SetOccupied(bool occupied)
        {
            if (occupied != _isOccupied)
                StateChangedEvents?.Invoke(this, new(occupied));
            _isOccupied = occupied;
        }

        public bool AddSegment(TrackSegment seg) 
        {
            if (_segments.Contains(seg))
                return false;

            _segments.Add(seg);
            return true;
        }

        public List<TrackSegment> GetSegments()
        {
            return [.. _segments];
        }
    }
}
