using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class BoundaryCircuit(string id, BoundaryCircuit.BoundarySide side)
    {
        public enum BoundarySide
        {
            Left,
            Right
        }

        readonly string _id = id;
        readonly BoundarySide _side = side;

        Circuit? _outCircuit;
        InputCircuit? _inCircuit;

        BoundaryCircuit? _pairedCircuit;

        public string ID
        {
            get => _id;
        }

        public BoundarySide Side
        {
            get => _side;
        }

        public BoundaryCircuit? PairedCircuit
        {
            get => _pairedCircuit;
            set => _pairedCircuit = value;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", _side.ToString(), _id);
        }

        public void SetInputState(bool active)
        {
            if (_inCircuit == null)
                return;

            _inCircuit.SetActive(active);
        }

        public void SetOutCircuit(Circuit circuit)
        {
            if (circuit == null)
                return;

            _outCircuit = circuit;
            _outCircuit.ActivationEvents += OnOutputChanged;
        }

        public void SetInCircuit(InputCircuit circuit)
        {
            _inCircuit = circuit;
        }

        private void OnOutputChanged(object? sender, CircuitActivationArgs e)
        {
            if (_pairedCircuit == null)
                return;

            _pairedCircuit.SetInputState(e.Active);
        }
    }
}
