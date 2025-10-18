using ModularPanels.TrackLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    public class SignalLatchIndication(SignalHead head, string indication)
    {
        readonly string _indication = indication;
        readonly SignalHead _head = head;
        DetectorLatch? _latch;

        public bool IsLatched
        {
            get
            {
                if (_latch == null)
                    return false;

                return _latch.CurrentState != DetectorLatch.State.Released &&
                    _latch.CurrentState != DetectorLatch.State.Inactive;
            }
        }

        public void SetDetectorLatch(DetectorLatch latch)
        {
            _latch = latch;
            _latch.StateChangedEvents += OnStateChanged;
        }

        public void Set()
        {
            _latch?.Set();
        }

        public void Unset()
        {
            _latch?.Unset();
        }

        private void OnStateChanged(object? sender, DetectorLatch.StateChangeEventArgs e)
        {
            if (e.NewState == DetectorLatch.State.Released)
            {
                _head.SetIndicationFixed(_indication);
            }
        }
    }
}
