using PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.ButtonLib
{
    public class ClickEventArgs(Point point) : EventArgs
    {
        public Point Point { get; set; } = point;
    }

    public interface IClickable
    {
        /// <summary>
        /// Return whether given point lies within the clickable volume.
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>true if point is in volume, false otherwise</returns>
        public bool PointInVolume(Point p);

        /// <summary>
        /// Return whether the click interaction can be currently processed
        /// </summary>
        /// <param name="p">Click position</param>
        /// <returns>true if click can be processed, false otherwise</returns>
        public bool CanClick(Point p);

        /// <summary>
        /// Process a mouse down event
        /// </summary>
        /// <param name="p">Click position</param>
        public void MouseDown(Point p);

        /// <summary>
        /// Process a mouse up event
        /// </summary>
        /// <param name="p">Click position</param>
        public void MouseUp(Point p);
    }

    public class CircularVolume(Point center, float radius) : IClickable
    {
        readonly Point _center = center;
        readonly float _radius = radius;

        Func<Point, bool>? _canClickFunc;

        public event EventHandler<ClickEventArgs>? MouseDownEvents;
        public event EventHandler<ClickEventArgs>? MouseUpEvents;

        public Point Center { get { return _center; } }
        public float Radius { get { return _radius; } }
        public Func<Point, bool> CanClickFunc
        {
            set { _canClickFunc = value; }
        }


        public bool PointInVolume(Point p)
        {
            Vector2 vP = Drawing.PointToVector(p);
            Vector2 vC = Drawing.PointToVector(_center);
            return Vector2.Distance(vP, vC) <= _radius;
        }

        public void MouseDown(Point p)
        {
            MouseDownEvents?.Invoke(this, new ClickEventArgs(p));
        }

        public void MouseUp(Point p)
        {
            MouseUpEvents?.Invoke(this, new ClickEventArgs(p));
        }

        public bool CanClick(Point p)
        {
            if (_canClickFunc == null)
                return true;

            return _canClickFunc.Invoke(p);
        }
    }

    public class InteractionSpace
    {
        readonly DrawPanel _panel;
        readonly List<IClickable> _clickables = [];
        readonly List<IControl> _allControls = [];
        readonly Dictionary<string, Circuit> _circuits = [];

        IClickable? _lastClicked;

        public InteractionSpace(DrawPanel panel)
        {
            _panel = panel;

            _panel.MouseDown += OnMouseDown;
            _panel.MouseUp += OnMouseUp;
            _panel.Paint += OnPaint;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            foreach (var clickable in _clickables)
            {
                if (clickable.PointInVolume(e.Location) && clickable.CanClick(e.Location))
                {
                    clickable.MouseDown(e.Location);
                    _lastClicked = clickable;
                    _panel.Invalidate();
                }
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (_lastClicked != null)
            {
                _lastClicked.MouseUp(e.Location);
                _lastClicked = null;
                _panel.Invalidate();
            }
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            foreach (var c in _allControls)
            {
                c.Draw(e.Graphics);
            }
        }

        public void AddControl(IControl control)
        {
            _allControls.Add(control);
            _clickables.AddRange(control.GetClickables());
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
                foreach (var sc in circuitData.SignalCircuits)
                {
                    if (!ModularPanels.Layout.SignalSpace.SignalMap.TryGetValue(sc.SigID, out PanelLib.Signal? sig))
                        continue;

                    if (!_circuits.TryGetValue(sc.Circuit, out Circuit? circuit))
                        continue;

                    PanelLib.SignalHead? sigHead = sig.GetDefaultHead();
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
