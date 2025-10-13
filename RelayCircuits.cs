using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels
{
    public class CircuitActivationArgs(bool active) : EventArgs
    {
        public bool Active { get; set; } = active;
    }

    public abstract class Circuit
    {
        readonly string _name;
        protected HashSet<LogicCircuit> _affectedCircuits = [];
        protected bool _active = false;

        public event EventHandler<CircuitActivationArgs>? ActivationEvents;

        public bool Active { get => _active; }

        public string Name { get => _name; }

        public Circuit(string name)
        {
            _name = name;
        }

        public void SetActive(bool active)
        {
            bool changed = _active != active;
            _active = active;

            if (changed)
            {
                ActivationEvents?.Invoke(this, new(_active));
                foreach (var c in _affectedCircuits)
                {
                    c.Reevaluate();
                }
            }
        }

        public void AddToLogicCircuit(LogicCircuit logic)
        {
            _affectedCircuits.Add(logic);
        }
    }

    public class SimpleCircuit : Circuit
    {
        public SimpleCircuit(string name) : base(name) { }
    }

    public abstract class CircuitCondition(Circuit circuit)
    {
        protected Circuit _circuit = circuit;

        public abstract bool Evaluate(bool operand);

        public void AddCondition(LogicCircuit logic)
        {
            _circuit.AddToLogicCircuit(logic);
        }
    }

    public class CircuitAnd(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return operand && _circuit.Active;
        }
    }

    public class CircuitAndNot(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return operand && !_circuit.Active;
        }
    }

    public class CircuitNand(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return !(operand && _circuit.Active);
        }
    }

    public class CircuitOr(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return operand || _circuit.Active;
        }
    }

    public class CircuitNor(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return !(operand || _circuit.Active);
        }
    }

    public class CircuitNot(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return !_circuit.Active;
        }
    }

    public class CircuitEq(Circuit circuit) : CircuitCondition(circuit)
    {
        public override bool Evaluate(bool operand)
        {
            return _circuit.Active;
        }
    }

    public class LogicCircuit : Circuit
    {
        List<CircuitCondition> _conditionOn = [];
        List<CircuitCondition> _conditionOff = [];

        public LogicCircuit(string name) : base(name) { }

        public void AddOnCondition(CircuitCondition cond)
        {
            _conditionOn.Add(cond);
            cond.AddCondition(this);
        }

        public void AddOffCondition(CircuitCondition cond)
        {
            _conditionOff.Add(cond);
            cond.AddCondition(this);
        }

        private bool EvaluateCondition(ref List<CircuitCondition> operators)
        {
            bool result = true;
            foreach (var c in operators)
            {
                result = c.Evaluate(result);
            }
            return result;
        }

        public void Reevaluate()
        {
            bool change;
            if (_active)
                change = EvaluateCondition(ref _conditionOff);
            else
                change = EvaluateCondition(ref _conditionOn);

            if (change)
                SetActive(!_active);
        }
    }
}
