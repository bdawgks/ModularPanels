using ModularPanels.DrawLib;
using ModularPanels.PanelLib;
using System.Numerics;
using static ModularPanels.ButtonLib.RotarySwitchTemplate;
using ModularPanels.CircuitLib;
using ModularPanels.JsonLib;

namespace ModularPanels.ButtonLib
{
    public class RotarySwitchTemplate: ITemplate
    {
        public readonly struct PositionTemplate
        {
            public readonly float angle;
            public readonly float size;
            public readonly bool latching;
            internal readonly IndicatorLampTemplate? lamp;

            public readonly string text = string.Empty;
            public readonly TextStyle textStyle = new();

            internal PositionTemplate(RotarySwitchPositionJsonData template)
            {
                angle = template.Angle;
                size = template.Size;
                latching = template.Latching;
                if (template.Lamp != null)
                    lamp = new(template.Lamp.Value);

                if (template.Text != null)
                    text = template.Text;

                if (template.TextStyle != null)
                {
                    StringKey<TextStyle> styleId = new(template.TextStyle);
                    GlobalBank.Instance.RegisterKey(styleId);
                    if (!styleId.IsNull)
                        textStyle = styleId.Object!;
                }
            }
        }

        readonly string _name;
        readonly float _size;
        readonly List<PositionTemplate> _positions = [];
        readonly Color _primaryColor;
        readonly Color _secondaryColor;
        readonly int _centerPos = 0;

        public string Name { get => _name; }
        public float Size { get => _size; }
        public PositionTemplate[] Positions { get => [.. _positions]; }
        public Color PrimaryColor { get => _primaryColor; }
        public Color SecondaryColor { get => _secondaryColor; }
        public int CenterPos { get => _centerPos; }

        internal RotarySwitchTemplate(RotarySwitchTemplateJsonData templateData)
        {
            _name = templateData.Name;
            _centerPos = templateData.CenterPos;
            _size = templateData.Size;
            _primaryColor = templateData.PrimaryColor;
            _secondaryColor = templateData.SecondaryColor;

            foreach (var s in templateData.Positions)
            {
                _positions.Add(new PositionTemplate(s));
            }
        }
    }

    public class RotarySwitchPosition : IDrawTransformable
    {
        readonly int _index;
        readonly RotarySwitch _switch;
        readonly float _angle;
        readonly float _size;
        readonly bool _latching = true;
        readonly CircularVolume _clickVolume;

        readonly IndicatorLamp? _lamp;
        readonly TextStyle _textStyle;
        string _text;

        InputCircuit? _activatedCircuit;

        public float Angle { get => _angle; }
        public float Size { get => _size; }
        public bool Latching { get => _latching; }
        public CircularVolume ClickVolume { get => _clickVolume; }

        public RotarySwitchPosition(RotarySwitch parent, int index, PositionTemplate template)
        {
            _index = index;
            _switch = parent;
            _angle = template.angle;
            _size = template.size;
            _latching = template.latching;

            Vector2 pSwitch = _switch.Pos.ToVector2();
            float totalAngle = (_switch.Angle + _angle) * MathF.PI / 180;
            float totalOffset = _switch.Size + _size;
            Vector2 posDir = new(MathF.Sin(totalAngle), -MathF.Cos(totalAngle));
            Vector2 pos = pSwitch + posDir * totalOffset;

            _clickVolume = new(DrawingPos.FromVector2(pos), _size);
            _clickVolume.MouseDownEvents += MouseDown;
            _clickVolume.MouseUpEvents += MouseUp;

            if (template.lamp != null)
            {
                _lamp = new(_clickVolume.Center, template.lamp.Value);
            }

            _text = template.text;
            _textStyle = template.textStyle;
        }

        private void MouseDown(object? sender, ClickEventArgs e)
        {
            _switch.SetPosition(_index);
        }

        private void MouseUp(object? sender, ClickEventArgs e)
        {
            if (!_latching)
                _switch.ReturnToCenter(_index);
        }

        public void Draw(DrawingContext context)
        {
            _lamp?.Draw(context);

            if (_text != string.Empty)
            {
                context.drawing.DrawText(context.graphics, _text, context.drawing.Transform(_clickVolume.Center), _textStyle);
            }
        }

        public List<DrawTransform> GetTransforms()
        {
            List<DrawTransform> transforms = [];
            transforms.Add(_clickVolume);
            if (_lamp != null)
                transforms.Add(_lamp);
            return transforms;
        }

        public void EnterPostion()
        {
            _activatedCircuit?.SetActive(true);
        }

        public void LeavePosition()
        {
            _activatedCircuit?.SetActive(false);
        }

        public void SetActivatedCircuit(InputCircuit circuit)
        {
            _activatedCircuit = circuit;
        }

        public void SetLampActivationCircuit(Circuit circuit)
        {
            if (_lamp == null)
                return;

            circuit.ActivationEvents += (sender, e) =>
            {
                _lamp.LampOn = e.Active;
            };
            _lamp.LampOn = circuit.Active;
        }

        public void SetText(string text)
        {
            _text = text;
        }
    }

    public class RotarySwitch : DrawTransform, IControl
    {
        readonly InteractionComponent _parent;

        readonly RotarySwitchTemplate _template;

        readonly float _size;
        readonly DrawingPos _pos;
        readonly float _angle = 0f;
        readonly int _centerPos = 0;

        readonly CircularVolume _switchVolume;
        readonly RotarySwitchPosition[] _positions = [];

        int _curPos = 0;
        Circuit? _interlockCircuit;
        bool _locked = false;

        public RotarySwitchPosition[] Positions
        {
            get => _positions;
        }

        public float Size { get => _size; }
        public DrawingPos Pos { get => _pos; }
        public float Angle { get => _angle; }
        public int CenterPos { get => _centerPos; }

        public RotarySwitch(InteractionComponent parent, GridPos pos, RotarySwitchTemplate template)
        {
            _parent = parent;

            _template = template;
            _pos = Grid.Instance.TransformPos(pos);
            _size = template.Size;
            _centerPos = template.CenterPos;
            _curPos = _centerPos;
            _switchVolume = new(_pos, _size);
            _positions = new RotarySwitchPosition[template.Positions.Length];
            for (int i = 0; i < _positions.Length; i++)
            {
                _positions[i] = new(this, i, template.Positions[i]);
            }
            _switchVolume.CanClickFunc = CanClick;
            _switchVolume.MouseDownEvents += MouseDown;
            _switchVolume.MouseUpEvents += MouseUp;

            _parent.AddControl(this);
        }

        private bool CanClick(Point p, DrawingPos pos)
        {
            return !_locked;
        }
        private void MouseDown(object? sender, ClickEventArgs e)
        {
            int posIdx = GetClickedPosition(e.Pos);
            if (posIdx != -1)
            {
                SetPosition(posIdx);
            }
        }
        private void MouseUp(object? sender, ClickEventArgs e)
        {
            if (!_positions[_curPos].Latching)
            {
                ReturnToCenter(_curPos);
            }
        }

        private int GetClickedPosition(DrawingPos clickPos)
        {
            Vector2 vClick = clickPos.ToVector2();
            Vector2 vCenter = _pos.ToVector2();
            Vector2 clickDir = Vector2.Normalize(vClick - vCenter);

            Vector2[] posVectors = new Vector2[_positions.Length];
            for (int i = 0; i < _positions.Length; i++)
            {
                float radAngle = float.DegreesToRadians(_angle + _positions[i].Angle);
                posVectors[i] = new(MathF.Sin(radAngle), -MathF.Cos(radAngle));
            }
            const float maxAngle = 35f;
            for (int i = 0; i < _positions.Length; i++)
            {
                Vector2 thisDir = posVectors[i];
                float thisAngle = MathF.Abs(Drawing.VectorAngle(thisDir, clickDir));
                if (thisAngle > maxAngle)
                    continue;

                if (i > 0)
                {
                    Vector2 prevDir = posVectors[i - 1];
                    float prevAngle = MathF.Abs(Drawing.VectorAngle(prevDir, clickDir));
                    if (thisAngle > prevAngle)
                        continue;
                }
                if (i + 1 < _positions.Length)
                {
                    Vector2 nextDir = posVectors[i + 1];
                    float nextAngle = MathF.Abs(Drawing.VectorAngle(nextDir, clickDir));
                    if (thisAngle > nextAngle)
                        continue;
                }
                return i;
            }

            return -1;
        }

        public List<DrawTransform> GetTransforms()
        {
            List<DrawTransform> transforms = [];
            transforms.Add(this);
            transforms.Add(_switchVolume);
            foreach (var pos in _positions)
            {
                transforms.AddRange(pos.GetTransforms());
            }
            return transforms;
        }

        public IClickable[] GetClickables()
        {
            IClickable[] array = new IClickable[_positions.Length + 1];
            array[0] = _switchVolume;
            for (int i = 0; i < _positions.Length; i++)
            {
                array[i + 1] = _positions[i].ClickVolume;
            }
            return array;
        }

        public void Draw(DrawingContext context)
        {
            Graphics g = context.graphics;
            Point point = context.drawing.Transform(_pos);

            float curPosAngle = _angle + _positions[_curPos].Angle;
            g.TranslateTransform(point.X, point.Y);
            g.RotateTransform(curPosAngle);
            RectangleF rect = new(-_size, -_size, _size * 2f, _size * 2f);

            float handleWidth = _size * 0.25f;
            float handleLength = _size * 0.9f;
            RectangleF rectHandle = new(-handleWidth, -handleLength, handleWidth * 2f, handleLength * 2f);

            float notchWidth = handleWidth * 0.5f;
            RectangleF rectNotch = new(-notchWidth, -handleLength, notchWidth * 2f, handleLength);

            Brush primaryBrush = new SolidBrush(_template.PrimaryColor);
            Brush secondaryBrush = new SolidBrush(_template.SecondaryColor);
            Brush lineBrush = new SolidBrush(Color.Black);
            Pen linePen = new(lineBrush, 1.5f);
            g.FillEllipse(primaryBrush, rect);
            g.DrawEllipse(linePen, rect);
            g.DrawRectangle(linePen, rectHandle);
            g.FillRectangle(secondaryBrush, rectNotch);
            g.DrawRectangle(linePen, rectNotch);
            g.ResetTransform();

            foreach (RotarySwitchPosition pos in _positions)
            {
                pos.Draw(context);
            }

            primaryBrush.Dispose();
            secondaryBrush.Dispose();
            lineBrush.Dispose();
            linePen.Dispose();
        }

        public void SetInterlockingCircuit(string circuitName)
        {
            CircuitComponent? circuits = _parent.Parent.GetComponent<CircuitComponent>();
            if (circuits == null)
                return;

            circuits.TryGetCircuit(circuitName, out _interlockCircuit);

            if (_interlockCircuit != null)
                _interlockCircuit.ActivationEvents += (sender, e) =>
                {
                    _locked = e.Active;
                };
        }

        public void SetActivatedCircuit(int posIdx, InputCircuit circuit)
        {
            if (circuit == null) 
                return;

            _positions[posIdx].SetActivatedCircuit(circuit);
        }

        public void SetLampActivationCircuit(int posIdx, Circuit circuit)
        {
            if (circuit == null)
                return;

            _positions[posIdx].SetLampActivationCircuit(circuit);
        }

        public void SetTextLabel(int posIdx, string text)
        {
            _positions[posIdx].SetText(text);
        }

        public void SetPosition(int idx)
        {
            if (_locked)
                return;

            int startPos = _curPos;
            _curPos = idx;
            UpdatePosition(startPos);
        }

        public void ReturnToCenter(int from)
        {
            int startPos = _curPos;
            while (_curPos != _centerPos && !_positions[_curPos].Latching)
            {
                int next = -1;
                if (from <= _centerPos)
                {
                    next = 1;
                }
                _curPos += next;
            }
            UpdatePosition(startPos);
        }

        public void Init()
        {
            _positions[_curPos].EnterPostion();
        }

        private void UpdatePosition(int startPos)
        {
            if (_curPos == startPos)
                return;

            _positions[startPos].LeavePosition();
            _positions[_curPos].EnterPostion();
        }
    }
}
