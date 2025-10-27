using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace ModularPanels.CircuitLib
{
    public class PointsCircuit
    {
        readonly TrackPoints _points;

        InputCircuit? _outputPointsNormalized;
        InputCircuit? _outputPointsReversed;
        Circuit? _inputPointsNormal;
        Circuit? _inputPointsReverse;

        public PointsCircuit(TrackPoints points)
        {
            _points = points;
            _points.StateChangeEvents += OnPointsChange;
        }

        private void OnInputNormal(object? sender, CircuitActivationArgs args)
        {
            if (args.Active)
                _points.SetState(TrackPoints.PointsState.Normal);
        }

        private void OnInputReverse(object? sender, CircuitActivationArgs args)
        {
            if (args.Active)
                _points.SetState(TrackPoints.PointsState.Reversed);
        }

        private static bool AssignInputCircuit([NotNullWhen(true)] ref Circuit? member, Circuit? value)
        {
            if (value == null)
                return false;

            if (member != null)
                return false;

            member = value;
            return true;
        }

        private static void AssignOutputCircuit(ref InputCircuit? member, InputCircuit? value)
        {
            if (value == null)
                return;

            if (member != null)
                return;

            member = value;
        }

        private void OnPointsChange(object? sender, TrackPoints.PointsStateChangeArgs args)
        {
            if (_outputPointsNormalized != null)
            {
                _outputPointsNormalized.SetActive(args.NewState == TrackPoints.PointsState.Normal);
            }
            if (_outputPointsReversed != null)
            {
                _outputPointsReversed.SetActive(args.NewState == TrackPoints.PointsState.Reversed);
            }
        }

        public void SetOutputs(InputCircuit? outputNormalized, InputCircuit? outputReversed)
        {
            AssignOutputCircuit(ref _outputPointsNormalized, outputNormalized);
            AssignOutputCircuit(ref _outputPointsReversed, outputReversed);
        }

        public void SetInputs(Circuit? inputNormal, Circuit? inputReverse)
        {
            if (AssignInputCircuit(ref _inputPointsNormal, inputNormal))
                _inputPointsNormal.ActivationEvents += OnInputNormal;
            if (AssignInputCircuit(ref _inputPointsReverse, inputReverse))
                _inputPointsReverse.ActivationEvents += OnInputReverse;
        }
    }
}
