using ModularPanels.SignalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class SignalCircuit(SignalHead signal, string indication)
    {
        readonly SignalHead _signal = signal;
        readonly string _indication = indication;
        Circuit? _input;
        SimpleCircuit? _output;

        public void SetInput(Circuit? input)
        {
            if (_input != null)
                return;

            _input = input;
        }

        public void SetOutput(SimpleCircuit? output)
        {
            if (_output != null)
                return;

            _output = output;
        }
    }
}
