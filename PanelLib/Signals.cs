using System.Drawing;
using System.Numerics;

namespace PanelLib
{
    public class SignalShape
    {
        readonly string _id;
        readonly HashSet<string>? _aspect;
        Shape? _shape = null;
        readonly Vector2 _offset;
        readonly float _angle;
        readonly ShapeMirror _mirror;
        readonly string? _headId;

        public string ID { get { return _id; } }
        public Shape? Shape { get { return _shape; } }

        public string? HeadID { get { return _headId; } }

        public SignalShape(string id, string[] aspect, Vector2 offset, float angle, string? headId = null, ShapeMirror mirror = ShapeMirror.None)
        {
            _id = id;
            _offset = offset;
            _angle = angle;
            _mirror = mirror;
            _headId = headId;

            if (aspect != null)
                _aspect = [.. aspect];
        }

        public void InitShape(ShapeBank bank)
        {
            _shape = bank[_id];
        }

        public bool VisibleWithAspect(string aspect)
        {
            if (aspect == null || aspect == "")
                return true;

            if (_aspect == null || _aspect.Count < 1)
                return true;

            return _aspect.Contains(aspect);
        }

        public void DrawShape(Graphics g, PointF origin, float originAngle, float scale = 1.0f)
        {
            if (_shape == null)
                return;

            Vector2 oTransformed = MathLib.RotateVector(_offset * scale, originAngle);

            _shape.DrawShape(g, origin, oTransformed, originAngle + _angle, scale, _mirror);
        }
    }

    public class SignalType
    {
        readonly string _name;
        readonly SignalRuleset? _ruleset;
        string? _startIndication = null;
        protected readonly HashSet<string> _headNames = [];
        readonly List<SignalShape> _shapes = new();

        public SignalRuleset? Ruleset { get { return _ruleset; } }
        public List<SignalShape> Shapes { get { return _shapes; } }

        public string? StartIndication
        {
            get { return _startIndication; }
            set { _startIndication = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public SignalType(string name, SignalRuleset? ruleset)
        {
            _name = name;
            _ruleset = ruleset;
        }

        public List<string> GetHeadNames()
        {
            return [.. _headNames];
        }

        public void AddShape(string id, string[] aspect, int[] offset, float angle, string? headId = null, ShapeMirror mirror = ShapeMirror.None)
        {
            Vector2 offsetVector = new();
            if (offset.Length == 2)
            {
                offsetVector.X = offset[0];
                offsetVector.Y = offset[1];
            }

            if (headId != null)
            {
                if (!_headNames.Contains(headId))
                    _headNames.Add(headId);
            }

            _shapes.Add(new SignalShape(id, aspect, offsetVector, angle, headId, mirror));
        }
        public void LoadShapes(ShapeBank bank)
        {
            for (int i = 0; i < _shapes.Count; i++)
            {
                _shapes[i].InitShape(bank);
            }
        }
    }

    public class SignalBank
    {
        readonly ShapeBank _shapeBank = new();
        protected readonly Dictionary<string, SignalType> _types = [];
        readonly Dictionary<string, SignalRuleset> _rulesets = [];

        public ShapeBank ShapeBank
        {
            get => _shapeBank;
        }

        public void AddType(SignalType type)
        {
            _types.Add(type.Name, type);
        }

        public SignalType? this[string name]
        {
            get
            {
                if (_types.TryGetValue(name, out SignalType? value))
                    return value;

                return null;
            }
        }
        public void InitShapes()
        {
            foreach (SignalType t in _types.Values)
            {
                t.LoadShapes(_shapeBank);
            }
        }

        public Signal? CreateSignal(string id, string type)
        {
            if (_types.TryGetValue(type, out SignalType? value))
                return new(id, value);

            return null;
        }

        public SignalRuleset? GetRuleset(string name)
        {
            if (_rulesets.TryGetValue(name, out SignalRuleset? value))
                return value;

            return null;
        }

        public SignalRuleset CreateRuleset(string name)
        {
            if (_rulesets.TryGetValue(name, out SignalRuleset? value))
                return value;

            SignalRuleset newRuleset = new();
            _rulesets.Add(name, newRuleset);
            return newRuleset;
        }
    }

    public class SignalRuleIndication
    {
        readonly Dictionary<string, string> _aspects = [];
        string _defaultAspect = "0";

        public void SetAspect(string? nextAspect, string aspect)
        {
            if (nextAspect == null)
            {
                _defaultAspect = aspect;
                return;
            }

            _aspects[nextAspect] = aspect;
        }

        public string GetAspect(string? nextAspect)
        {
            if (_aspects.Count < 1)
                return _defaultAspect;

            if (nextAspect == null)
                return _defaultAspect;

            if (_aspects.ContainsKey(nextAspect))
                return _aspects[nextAspect];

            return _defaultAspect;
        }
    }

    public class SignalRuleset
    {
        readonly Dictionary<string, SignalRuleIndication> _indications = [];
        readonly List<string> _aspects = [];
        string _defaultIndication = "";

        public string DefaultIndication
        {
            get { return _defaultIndication; }
            set { _defaultIndication = value; }
        }

        public SignalRuleIndication AddIndication(string indicationName)
        {
            if (_indications.ContainsKey(indicationName))
                return _indications[indicationName];

            SignalRuleIndication newIndication = new();
            _indications.Add(indicationName, newIndication);

            if (_defaultIndication == string.Empty)
                _defaultIndication = indicationName;

            return newIndication;
        }

        public string? GetAspect(string indicationName, string? nextAspect)
        {
            if (_indications.TryGetValue(indicationName, out SignalRuleIndication? value))
                return value.GetAspect(nextAspect);

            return null;
        }

        public void SetAspectIndex(string aspect, int idx)
        {
            if (GetAspectIndex(aspect) != -1)
                return;

            _aspects.Insert(idx, aspect);
        }

        public int GetAspectIndex(string aspect)
        {
            return _aspects.FindIndex(s => s.Equals(aspect));
        }

        public string? GetAspectFromIndex(int idx)
        {
            if (idx < 0 || idx >= _aspects.Count)
                return null;

            return _aspects[idx];
        }
    }

    public struct PointsRoute
    {
        public TrackPoints points;
        public TrackPoints.PointsState state;

        public readonly bool IsSet()
        {
            return state == points.State;
        }
    }

    public class SignalRoute
    {
        readonly string _indication;
        readonly SignalHead? _nextSignal;
        readonly List<PointsRoute> _pointsRouting = [];

        public string Indication { get { return _indication; } }

        public SignalHead? NextSignal { get { return _nextSignal; } }

        public SignalRoute(string indication, SignalHead? nextSignal)
        {
            _indication = indication;
            _nextSignal = nextSignal;
        }

        public void AddPointsRoute(PointsRoute pr)
        {
            _pointsRouting.Add(pr);
        }

        public bool IsRouteSet()
        {
            foreach (PointsRoute pr in _pointsRouting)
            {
                if (pr.IsSet())
                    return false;
            }

            return true;
        }
    }

    public class SignalAspectEventArgs(string aspect) : EventArgs
    {
        public string Aspect { get; } = aspect;
    }

    public class SignalHead
    {
        readonly Signal _parent;
        readonly string _id;
        SignalHead? _precedingSignal;
        SignalHead? _advancedSignal;
        readonly List<SignalRoute> _routes = new();
        SignalRoute? _activeRoute;
        string? _indication;
        string _aspect = "0";

        public EventHandler<SignalAspectEventArgs>? AspectEvents { get; set; }

        public string ID
        {
            get { return _id; }
        }

        public string Aspect
        {
            get { return _aspect; }
            set { SetAspect(value); }
        }

        public SignalHead? PrecedingSignal
        {
            get { return _precedingSignal; }
            set { _precedingSignal = value; }
        }

        public SignalHead? AdvancedSignal
        {
            get { return _advancedSignal; }
        }

        public SignalHead(string id, Signal parent)
        {
            _id = id;
            _parent = parent;
            _indication = _parent.Type.StartIndication;
            if (parent.Type.Ruleset != null && _indication != null)
            {
                string? startAspect = parent.Type.Ruleset.GetAspect(_indication, null);
                if (startAspect != null)
                    _aspect = startAspect;
            }
        }

        public void AddRoute(SignalRoute route)
        {
            _routes.Add(route);
        }

        public void InitSignal()
        {
            UpdateRoute();
        }

        public string? GetRouteIndication()
        {
            UpdateRoute();
            if (_activeRoute != null)
            {
                return _activeRoute.Indication;
            }

            return null;
        }

        public void SetIndication(string indication)
        {
            _indication = indication;
            UpdateIndication();
        }

        private void UpdateIndication()
        {
            if (_indication == null)
                return;

            if (_parent.Type.Ruleset != null)
            {
                string? nextAspect = null;
                if (_activeRoute != null && _activeRoute.NextSignal != null)
                    nextAspect = _activeRoute.NextSignal.Aspect;

                string? newAspect = _parent.Type.Ruleset.GetAspect(_indication, nextAspect);
                SetAspect(newAspect);
            }

            if (_precedingSignal != null)
            {
                _precedingSignal.UpdateIndication();
            }
        }

        public void UpdateRoute()
        {
            if (_routes.Count < 1)
                return;

            foreach (SignalRoute r in _routes)
            {
                if (r.IsRouteSet())
                {
                    _advancedSignal = r.NextSignal;
                    if (_advancedSignal != null)
                    {
                        _advancedSignal.UpdateRoute();
                        _advancedSignal.PrecedingSignal = this;
                    }

                    _activeRoute = r;
                    return;
                }
            }
        }

        private void SetAspect(string? newAspect)
        {
            if (newAspect == null)
                return;

            if (_aspect == newAspect)
                return;

            _aspect = newAspect;
            AspectEvents?.Invoke(this, new SignalAspectEventArgs(newAspect));
        }
    }

    public class Signal
    {
        readonly string _name;
        readonly SignalType _type;
        readonly Dictionary<string, SignalHead> _heads = new();
        readonly SignalHead? _defaultHead;

        Point _pos = new();
        float _angle = 0.0f;
        float _scale = 1.0f;

        public string Name { get { return _name; } }
        public Point Pos { get { return _pos; } }
        public SignalType Type { get { return _type; } }
        public float Angle { get { return _angle; } }
        public float Scale { get { return _scale; } }

        public Signal(string name, SignalType type)
        {
            _name = name;
            _type = type;

            List<string> headNames = type.GetHeadNames();
            if (headNames.Count < 1)
            {
                _defaultHead = new("default", this);
            }
            else
            {
                foreach (string headName in headNames)
                {
                    _heads.Add(headName, new(headName, this));
                }
            }
        }

        public void SetPos(int[] pos)
        {
            if (pos.Length == 2)
            {
                _pos = new Point(pos[0], pos[1]);
            }
        }

        public void SetAngle(float angle)
        {
            _angle = angle;
        }

        public void SetScale(float scale)
        {
            _scale = scale;
        }

        public SignalHead? GetHead(string? id)
        {
            if (id == null)
                return _defaultHead;

            if (_heads.ContainsKey(id))
                return _heads[id];

            return null;
        }

        public SignalHead? GetDefaultHead()
        {
            return _defaultHead;
        }

        public void InitSignal()
        {
            foreach (SignalHead head in _heads.Values)
            {
                head.InitSignal();
            }

            if (_defaultHead != null)
                _defaultHead.InitSignal();
        }

        public static bool ParseSignalID(string id, out string name, out string? head)
        {
            int delimIndex = id.IndexOf(":");
            if (delimIndex > 0)
            {
                name = id.Substring(0, delimIndex);
                head = id.Substring(delimIndex + 1);

                return true;
            }

            name = id;
            head = null;

            return false;
        }
    }

    public class SignalSpace
    {
        private readonly Dictionary<string, Signal> _signalMap = [];
        private readonly SignalBank _bank = new();

        public SignalSpace() { }

        public Dictionary<string, Signal> SignalMap { get { return _signalMap; } }

        public SignalBank Bank
        {
            get { return _bank; }
        }

        public Signal? CreateSignal(string id, string type)
        {
            Signal? sig = _bank.CreateSignal(id, type);

            if (sig != null)
                _signalMap.Add(id, sig);

            return sig;
        }
    }
}
