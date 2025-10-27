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
        InputCircuit? _output;
        string? _dropIndication;
        bool _resetLatch = false;

        public void SetInput(Circuit? input)
        {
            if (_input != null)
                return;

            _input = input;
            if (_input == null)
                return;

            _input.ActivationEvents += OnInputChange;
        }

        public void SetOutput(InputCircuit? output)
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

        public void SetResetLatch(bool resetLatch)
        {
            _resetLatch = resetLatch;
        }

        private void OnInputChange(object? sender, CircuitActivationArgs e)
        {
            if (e.Active)
            {
                if (_resetLatch)
                    _signal.ResetLatch();

                if (_fixedIndication)
                    _signal.SetIndicationFixed(_indication);
                else
                    _signal.SetRouteIndication(_indication);

                if (_dropIndication != null)
                    _signal.SetAutoDropIndication(_dropIndication);
            }
        }

        private void OnSignalChange(object? sender, SignalStateChangeArgs e)
        {
            if (e.Indication != null && _output != null)
                _output.SetActive(e.Indication == _indication);
            else
                _output?.SetActive(false);
        }
    }
}
