using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularPanels.Components;
using ModularPanels.JsonLib;
using ModularPanels.SignalLib;

namespace ModularPanels.CircuitLib
{
    public class CircuitChangeEventArgs(Circuit circuit, bool active) : EventArgs
    {
        public Circuit Circuit { get; } = circuit;
        public bool Active { get; } = active;

    }

    public class CircuitComponent : Component
    {
        readonly ObjectBank _circuitBank = new();

        public event EventHandler<CircuitChangeEventArgs>? CircuitChangeEvents;

        public CircuitComponent(IParent parent) : base(parent)
        {
        }

        public bool RegisterOrCreateInputCircuit(StringKey<Circuit> key, [NotNullWhen(true)] out InputCircuit? circuit)
        {
            _circuitBank.RegisterKey(key);
            if (key.IsNull)
            {
                circuit = new(key.Key);
                _circuitBank.DefineObject(key, circuit as Circuit);
                return true;
            }

            Circuit c = key.Object!;
            if (c is InputCircuit ic)
            {
                circuit = ic;
                return true;
            }

            circuit = null;
            return false;
        }

        public void RegisterKey(StringKey<Circuit> key)
        {
            _circuitBank.RegisterKey(key);
        }

        public bool TryGetCircuit(string circuitName, [NotNullWhen(true)] out Circuit? circuit)
        {
            return _circuitBank.TryGetObject(circuitName, out circuit);
        }

        public bool TryGetCircuit<T>(string circuitName, [NotNullWhen(true)] out T? circuit) where T : Circuit
        {
            circuit = null;
            if (_circuitBank.TryGetObject(circuitName, out Circuit? untypedCircuit))
            {
                if (untypedCircuit is T)
                {
                    circuit = (untypedCircuit as T)!;
                    return true;
                }
            }
            return false;
        }

        private void OnCircuitChanged(object? sender, CircuitActivationArgs e)
        {
            if (sender is Circuit circuit)
                CircuitChangeEvents?.Invoke(this, new(circuit, e.Active));
        }

        public void AddOrUpdateInputCircuit(StringKey<Circuit> key, string? desc = "", bool? active = false)
        {
            InputCircuit? circuit = null;
            _circuitBank.RegisterKey(key);
            if (key.IsNull)
            {
                circuit = new(key.Key);
                _circuitBank.DefineObject(circuit.Name, circuit as Circuit);
            }
            else if (key.Object is InputCircuit ic)
            {
                circuit = ic;
            }

            if (circuit != null)
            {
                circuit.Description = desc ?? "";
                circuit.SetActive(active ?? false);
                InitCircuit(circuit);
            }
        }

        public void AddCircuit(Circuit circuit)
        {
            _circuitBank.DefineObject(circuit.Name, circuit);
            InitCircuit(circuit);
        }

        public void AddCircuit(StringKey<Circuit> key, Circuit circuit)
        {
            _circuitBank.RegisterKey(key);
            _circuitBank.DefineObject(circuit.Name, circuit);
            InitCircuit(circuit);
        }

        private void InitCircuit(Circuit circuit)
        {
            circuit.ActivationEvents += OnCircuitChanged;
            circuit.Init();
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
                case "ORNOT": condition = new CircuitOrNot(circuit); break;
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
