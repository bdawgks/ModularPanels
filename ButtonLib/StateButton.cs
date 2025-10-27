using Microsoft.VisualBasic.Devices;
using ModularPanels.CircuitLib;
using ModularPanels.DrawLib;
using ModularPanels.PanelLib;
using System.Diagnostics.CodeAnalysis;

namespace ModularPanels.ButtonLib
{
    public class StateButton : DrawTransform, IControl
    {
        readonly InteractionComponent _parent;
        readonly StateButtonTemplate _template;
        readonly DrawingPos _pos;
        readonly float _radius;
        readonly float _rimOffset;
        readonly CircularVolume _clickVolume;
        readonly Color _colorRim;
        readonly State[] _states;

        int _activeState = 0;

        private class State(StateButton parent, Color color)
        {
            readonly StateButton _parent = parent;
            readonly Color _buttonColor = color;
            InputCircuit? _activatedCircuit;
            Circuit? _activationCircuit;
            bool _active = false;

            public bool IsActive { get => _active; }

            public Color ButtonColor { get => _buttonColor; }

            public void SetActivatedCircuit(InputCircuit inputCircuit)
            {
                _activatedCircuit = inputCircuit;
            }

            public void SetActivationCircuit(Circuit circuit)
            {
                _activationCircuit = circuit;
                _activationCircuit.ActivationEvents += OnActivationCircuit;
            }

            private void OnActivationCircuit(object? sender, CircuitActivationArgs e)
            {
                _active = e.Active;
                _parent.UpdateState();
            }

            public void Deactivate()
            {
                _activatedCircuit?.SetActive(false);
            }

            public void Activate()
            {
                _activatedCircuit?.SetActive(true);
            }
        }

        public StateButton(InteractionComponent parent, GridPos pos, StateButtonTemplate template)
        {
            _parent = parent;
            _template = template;
            _pos = Grid.Instance.TransformPos(pos);
            _radius = template.Size;
            _rimOffset = template.RimSize;
            _colorRim = template.RimColor;
            _clickVolume = new(_pos, _radius - _rimOffset);
            _activeState = template.InitState;

            _states = new State[_template.States.Count];
            for (int i = 0; i < _states.Length; i++)
            {
                _states[i] = new(this, _template.States[i].color);
            }

            _clickVolume.CanClickFunc = CanClick;
            _clickVolume.MouseDownEvents += MouseDown;
            _clickVolume.MouseUpEvents += MouseUp;

            _parent.AddControl(this);
        }

        private void MouseDown(object? sender, ClickEventArgs e)
        {
            _states[_activeState].Activate();
        }

        private void MouseUp(object? sender, ClickEventArgs e)
        {
            _states[_activeState].Deactivate();
        }

        private bool CanClick(Point point, DrawingPos pos)
        {
            return true;
        }

        private void SetState(int newState)
        {
            if (_activeState == newState)
                return;

            _states[_activeState].Deactivate();
            _activeState = newState;
        }

        private void UpdateState()
        {
            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i].IsActive)
                {
                    SetState(i);
                    return;
                }
            }
        }

        private bool TryGetState(int idx, [NotNullWhen(true)] out State? state)
        {
            state = null;
            if (idx < 0 || idx >= _states.Length)
                return false;

            state = _states[idx];
            return true;
        }

        public void SetActivatedCircuit(int idx, InputCircuit inputCircuit)
        {
            if (TryGetState(idx, out State? state))
                state.SetActivatedCircuit(inputCircuit);
        }

        public void SetActivationCircuit(int idx, Circuit circuit)
        {
            if (TryGetState(idx, out State? state))
                state.SetActivationCircuit(circuit);
        }

        void IDrawable.Draw(DrawingContext context)
        {
            Graphics g = context.graphics;
            Point point = context.drawing.Transform(_pos);

            g.TranslateTransform(point.X, point.Y);
            RectangleF rect = new(-_radius, -_radius, _radius * 2f, _radius * 2f);

            float innerRadius = _radius - _rimOffset;
            RectangleF innerRect = new(-innerRadius, -innerRadius, innerRadius * 2f, innerRadius * 2f);

            State currentState = _states[_activeState];
            Brush innerBrush = new SolidBrush(currentState.ButtonColor);

            Brush rimBrush = new SolidBrush(_colorRim);
            Brush lineBrush = new SolidBrush(Color.Black);
            Pen linePen = new(lineBrush, 1.5f);
            g.FillEllipse(rimBrush, rect);
            g.DrawEllipse(linePen, rect);
            g.FillEllipse(innerBrush, innerRect);
            g.DrawEllipse(linePen, innerRect);
            g.ResetTransform();

            rimBrush.Dispose();
            innerBrush.Dispose();
            lineBrush.Dispose();
            linePen.Dispose();
        }

        IClickable[] IControl.GetClickables()
        {
            return [_clickVolume];
        }

        List<DrawTransform> IDrawTransformable.GetTransforms()
        {
            return [this, _clickVolume];
        }
    }

    public class StateButtonTemplate : ITemplate
    {
        readonly string _name;
        readonly float _size;
        readonly float _rimSize;
        readonly Color _rimColor;
        readonly List<State> _states = [];
        readonly int _initState;

        public readonly struct State(Color color)
        {
            public readonly Color color = color;
        }

        public string Name { get => _name; }
        public float Size { get => _size; }
        public float RimSize { get => _rimSize; }
        public Color RimColor { get => _rimColor; }
        public List<State> States { get => _states; }
        public int InitState { get => _initState; }

        internal StateButtonTemplate(StateButtonTemplateJsonData data)
        {
            _name = data.Name;
            _size = data.Size;
            _rimColor = data.PrimaryColor;
            _rimSize = data.RimSize;
            _initState = data.InitState;

            foreach (var state in data.States)
            {
                State s = new(state.Color);
                _states.Add(s);
            }
        }
    }
}
