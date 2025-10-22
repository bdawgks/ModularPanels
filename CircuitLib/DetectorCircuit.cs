using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.CircuitLib
{
    public class DetectorCircuit(TrackDetector detector)
    {
        readonly TrackDetector _detector = detector;
        InputCircuit? _outputCircuit;

        public void SetOutput(InputCircuit circuit)
        {
            if (_outputCircuit != null)
                return;

            _outputCircuit = circuit;
            _detector.StateChangedEvents += OnDetectorStateChanged;
        }

        private void OnDetectorStateChanged(object? sender, TrackDetector.DetectorStateArgs e)
        {
            if (_outputCircuit == null)
                return;

            _outputCircuit.SetActive(e.IsOccupied);
        }
    }
}
