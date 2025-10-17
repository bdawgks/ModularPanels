using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularPanels.Components;
using ModularPanels.JsonLib;
using ModularPanels.SignalLib;

namespace ModularPanels.CircuitLib
{
    public class CircuitComponent : Component
    {
        readonly ObjectBank _circuitBank = new();

        public event EventHandler? CircuitChangeEvents;

        public CircuitComponent(IParent parent) : base(parent)
        {
        }

        public void RegisterKey(StringKey<Circuit> key)
        {
            _circuitBank.RegisterKey(key);
        }

        public bool TryGetCircuit(string circuitName, out Circuit? circuit)
        {
            return _circuitBank.TryGetObject(circuitName, out circuit);
        }

        private void OnCircuitChanged(object? sender, CircuitActivationArgs e)
        {
            CircuitChangeEvents?.Invoke(this, new());
        }

        public void AddCircuit(Circuit circuit)
        {
            _circuitBank.DefineObject(circuit.Name, circuit);
            circuit.ActivationEvents += OnCircuitChanged;
        }

        public void AddCircuit(StringKey<Circuit> key, Circuit circuit)
        {
            _circuitBank.RegisterKey(key);
            _circuitBank.DefineObject(circuit.Name, circuit);
            circuit.ActivationEvents += OnCircuitChanged;
        }

        public CircuitCondition? CreateCircuitOperator(string id, string op)
        {
            if (!_circuitBank.TryGetObject(id, out Circuit? circuit))
                return null;

            CircuitCondition? condition = null;
            switch (op)
            {
                case "EQ": condition = new CircuitEq(circuit); break;
                case "NOT": condition = new CircuitNot(circuit); break;
                case "AND": condition = new CircuitAnd(circuit); break;
                case "ANDNOT": condition = new CircuitAndNot(circuit); break;
                case "NAND": condition = new CircuitNand(circuit); break;
                case "OR": condition = new CircuitOr(circuit); break;
                case "NOR": condition = new CircuitNor(circuit); break;
            }

            return condition;
        }

        public void UpdateCircuits()
        {
            foreach (Circuit c in _circuitBank.GetObjects<Circuit>().Values)
            {
                if (c is LogicCircuit lc)
                    lc.Reevaluate();
            }
        }

        public void InitCircuits(CircuitDataLoader loader)
        {
            loader.Load(this);
        }

        public List<Circuit> GetCircuits()
        {
            return [.. _circuitBank.GetObjects<Circuit>().Values];
        }
    }
}
