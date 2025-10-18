using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.TrackLib
{
    public class DetectorLatch
    {
        public class StateChangeEventArgs(State newState) : EventArgs
        {
            public State NewState { get; } = newState;
        }

        public enum State
        {
            Inactive,
            Primed,
            Tripped,
            Released
        }

        readonly TrackDetector _exitDetector;
        readonly TrackDetector _entryDetector;
        State _state = State.Inactive;

        public event EventHandler<StateChangeEventArgs>? StateChangedEvents;

        public State CurrentState
        {
            get => _state;
        }

        public DetectorLatch(TrackDetector exitDetector, TrackDetector entryDetector)
        {
            _exitDetector = exitDetector;
            _entryDetector = entryDetector;

            _exitDetector.StateChangedEvents += OnExitDetectorChanged;
            _entryDetector.StateChangedEvents += OnEntryDetectorChanged;
        }

        private void OnExitDetectorChanged(object? sender, TrackDetector.DetectorStateArgs e)
        {
            if (e.IsOccupied)
                return;

            if (_state == State.Tripped)
            {
                SetState(State.Released);
            }
        }

        private void OnEntryDetectorChanged(object? sender, TrackDetector.DetectorStateArgs e)
        {
            if (!e.IsOccupied)
                return;

            if (_state == State.Primed)
            {
                if (_exitDetector.IsOccupied)
                    SetState(State.Tripped);
                else
                    SetState(State.Released);
            }
        }

        public void Set()
        {
            if (_entryDetector.IsOccupied)
                SetState(State.Tripped);
            else
                SetState(State.Primed);
        }

        public void Unset()
        {
            SetState(State.Inactive);
        }

        private void SetState(State state)
        {
            if (_state != state)
                StateChangedEvents?.Invoke(this, new(state));
            _state = state;
        }
    }
}
