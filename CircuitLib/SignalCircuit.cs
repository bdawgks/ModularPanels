using ModularPanels.SignalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class SignalCircuit(SignalHead signal, string indication, bool fixedIndication = false)
    {
        readonly SignalHead _signal = signal;
        readonly string _indication = indication;
        readonly bool _fixedIndication = fixedIndication;
        Circuit? _input;
        SimpleCircuit? _output;
        string? _dropIndication;

        public void SetInput(Circuit? input)
        {
            if (_input != null)
                return;

            _input = input;
            if (_input == null)
                return;

            _input.ActivationEvents += OnInputChange;
        }

        public void SetOutput(SimpleCircuit? output)
        {
            if (_output != null)
                return;

            _output = output;
            if (_output == null)
                return;

            _signal.StateChangedEvents += OnSignalChange;
        }

        public void SetDropIndication(string dropIndication)
        {
            _dropIndication = dropIndication;
        }

        private void OnInputChange(object? sender, CircuitActivationArgs e)
        {
            if (e.Active)
            {
                if (_fixedIndication)
                    _signal.SetIndication(_indication, _dropIndication == null);
                else
                    _signal.SetRouteIndication(_indication);

                if (_dropIndication != null)
                    _signal.SetAutoDropIndication(_dropIndication);
            }
        }

        private void OnSignalChange(object? sender, SignalStateChangeArgs e)
        {
            if (e.Indication != null && _output != null)
            {
                if (e.Indication == _indication)
                    _output.SetActive(true);
            }
            else
                _output?.SetActive(false);
        }
    }
}
