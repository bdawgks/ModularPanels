using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        public virtual void Init() { }

        protected void SetActive(bool active)
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

        public override string ToString()
        {
            return string.Format("Circuit: {0}", Name);
        }

        public void AddToLogicCircuit(LogicCircuit logic)
        {
            _affectedCircuits.Add(logic);
        }
    }

    public class InputCircuit(string name) : Circuit(name)
    {
        public new void SetActive(bool active)
        {
            base.SetActive(active);
        }

        public override string ToString()
        {
            return string.Format("Input Circuit: {0}", Name);
        }
    }
}
