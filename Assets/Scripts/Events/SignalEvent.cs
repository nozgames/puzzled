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
    public class IncrementSignal : SignalEvent { public IncrementSignal(Wire wire) : base(wire) { } }
    public class UpSignal : SignalEvent { public UpSignal(Wire wire) : base(wire) { } }
    public class DownSignal : SignalEvent { public DownSignal(Wire wire) : base(wire) { } }
    public class LeftSignal : SignalEvent { public LeftSignal(Wire wire) : base(wire) { } }
    public class RightSignal : SignalEvent { public RightSignal(Wire wire) : base(wire) { } }
    public class UseSignal : SignalEvent { public UseSignal(Wire wire) : base(wire) { } }
}
