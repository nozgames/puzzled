namespace Puzzled
{
    public class SignalEvent : WireEvent
    {
        public SignalEvent(Wire wire) : base(wire) { }
    }

    public class ResetSignal : SignalEvent { public ResetSignal (Wire wire) : base(wire) { } }
    public class ToggleSignal : SignalEvent { public ToggleSignal (Wire wire) : base(wire) { } }
    public class OnSignal : SignalEvent { public OnSignal (Wire wire) : base(wire) { } }
    public class OffSignal : SignalEvent { public OffSignal (Wire wire) : base(wire) { } }
}
