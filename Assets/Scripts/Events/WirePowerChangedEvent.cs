namespace Puzzled
{
    /// <summary>
    /// Sent when the power of a wire changes for a given port
    /// </summary>
    public class WirePowerChangedEvent : WireEvent
    {
        /// <summary>
        /// True if the wire has power
        /// </summary>
        public bool hasPower => wire.enabled;

        public WirePowerChangedEvent(Wire wire) : base(wire)
        {
        }
    }

}
