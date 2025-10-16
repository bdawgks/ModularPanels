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
}
