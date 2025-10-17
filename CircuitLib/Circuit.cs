using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class CircuitActivationArgs(bool active) : EventArgs
    {
        public bool Active { get; set; } = active;
    }

    public abstract class Circuit(string name)
    {
        readonly string _name = name;
        protected HashSet<LogicCircuit> _affectedCircuits = [];
        protected bool _active = false;
        private string _description = "";

        internal virtual bool InitEvaluation { get => true; }

        public event EventHandler<CircuitActivationArgs>? ActivationEvents;

        public bool Active { get => _active; }

        public string Name { get => _name; }

        public string Description { get => _description; set => _description = value; }

        public virtual void Reevaluate() { }

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

    public class LogicCircuit(string name) : Circuit(name)
    {
        List<CircuitCondition> _conditionOn = [];
        List<CircuitCondition> _conditionOff = [];
        private bool _singleCondition = false;
        private bool _initEvaluation = false;

        internal override bool InitEvaluation { get => _initEvaluation; }

        public void AddCondition(CircuitCondition cond)
        {
            _singleCondition = true;
            _conditionOn.Add(cond);
            cond.AddCondition(this);
        }

        public void AddOnCondition(CircuitCondition cond)
        {
            _singleCondition = false;
            _conditionOn.Add(cond);
            cond.AddCondition(this);
        }

        public void AddOffCondition(CircuitCondition cond)
        {
            _singleCondition = false;
            _conditionOff.Add(cond);
            cond.AddCondition(this);
        }

        private static bool EvaluateCondition(ref List<CircuitCondition> operators)
        {
            bool result = true;
            foreach (var c in operators)
            {
                result = c.Evaluate(result);
            }
            return result;
        }

        public override void Reevaluate()
        {
            _initEvaluation = true;

            if (_singleCondition)
            {
                bool active = EvaluateCondition(ref _conditionOn);

                if (active != _active)
                {
                    SetActive(active);
                }
                return;
            }

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
