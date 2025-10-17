using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public abstract class CircuitCondition(Circuit circuit)
    {
        protected Circuit _circuit = circuit;

        public bool Evaluate(bool operand)
        {
            if (!_circuit.InitEvaluation)
            {
                _circuit.Reevaluate();
            }
            return EvaluateOp(operand);
        }

        protected abstract bool EvaluateOp(bool operand);

        public void AddCondition(LogicCircuit logic)
        {
            _circuit.AddToLogicCircuit(logic);
        }
    }

    public class CircuitAnd(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return operand && _circuit.Active;
        }
    }

    public class CircuitAndNot(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return operand && !_circuit.Active;
        }
    }

    public class CircuitNand(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return !(operand && _circuit.Active);
        }
    }

    public class CircuitOr(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return operand || _circuit.Active;
        }
    }

    public class CircuitNor(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return !(operand || _circuit.Active);
        }
    }

    public class CircuitNot(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return !_circuit.Active;
        }
    }

    public class CircuitEq(Circuit circuit) : CircuitCondition(circuit)
    {
        protected override bool EvaluateOp(bool operand)
        {
            return _circuit.Active;
        }
    }
}
