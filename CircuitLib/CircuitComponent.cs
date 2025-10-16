using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularPanels.Components;
using ModularPanels.SignalLib;

namespace ModularPanels.CircuitLib
{
    public class CircuitComponent : Component
    {
        readonly Dictionary<string, Circuit> _circuits = [];

        public CircuitComponent(IParent parent) : base(parent)
        {
        }

        public bool TryGetCircuit(string circuitName, out Circuit? circuit)
        {
            return _circuits.TryGetValue(circuitName, out circuit);
        }

        private void AddCircuit(Circuit circuit)
        {
            _circuits.Add(circuit.Name, circuit);
        }

        private CircuitCondition? CreateCircuitOperator(string id, string op)
        {
            if (!_circuits.TryGetValue(id, out Circuit? circuit))
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
            foreach (Circuit c in _circuits.Values)
            {
                if (c is LogicCircuit lc)
                    lc.Reevaluate();
            }
        }

        public void InitCircuits(JSON_Module_RelayCircuits circuitData)
        {
            if (circuitData.SimpleCircuits != null)
            {
                foreach (var sc in circuitData.SimpleCircuits)
                {
                    SimpleCircuit circuit = new(sc.ID);
                    if (sc.Active)
                        circuit.SetActive(true);
                    AddCircuit(circuit);
                }
            }
            if (circuitData.LogicCircuits != null)
            {
                foreach (var lc in circuitData.LogicCircuits)
                {
                    LogicCircuit circuit = new(lc.ID);
                    AddCircuit(circuit);
                }

                foreach (var lc in circuitData.LogicCircuits)
                {
                    Circuit circuit = _circuits[lc.ID];
                    if (circuit is LogicCircuit logicCircuit)
                    {
                        if (lc.Condition != null)
                        {
                            foreach (var condData in lc.Condition)
                            {
                                CircuitCondition? cond = CreateCircuitOperator(condData.Circuit, condData.Op);
                                if (cond == null)
                                    continue;

                                logicCircuit.AddOnCondition(cond);
                                logicCircuit.AddOffCondition(cond);
                            }
                        }
                        if (lc.ConditionOn != null)
                        {
                            foreach (var condData in lc.ConditionOn)
                            {
                                CircuitCondition? cond = CreateCircuitOperator(condData.Circuit, condData.Op);
                                if (cond == null)
                                    continue;

                                logicCircuit.AddOnCondition(cond);
                            }
                        }
                        if (lc.ConditionOff != null)
                        {
                            foreach (var condData in lc.ConditionOff)
                            {
                                CircuitCondition? cond = CreateCircuitOperator(condData.Circuit, condData.Op);
                                if (cond == null)
                                    continue;

                                logicCircuit.AddOffCondition(cond);
                            }
                        }
                    }
                }
            }
            if (circuitData.SignalCircuits != null)
            {
                SignalComponent? signalComp = Parent.Components.GetComponent<SignalComponent>();
                if (signalComp != null)
                {
                    foreach (var sc in circuitData.SignalCircuits)
                    {
                        if (!signalComp.SignalMap.TryGetValue(sc.SigID, out Signal? sig))
                            continue;

                        if (!_circuits.TryGetValue(sc.Circuit, out Circuit? circuit))
                            continue;

                        SignalHead? sigHead = sig.GetDefaultHead();
                        if (sigHead == null)
                            continue;

                        circuit.ActivationEvents += (sender, e) =>
                        {
                            sigHead.SetIndication(sc.Indication);
                        };
                    }
                }

            }
        }
    }
}
