namespace Puzzled
{
    public class ValueEvent : WireEvent
    {
        /// <summary>
        /// Value that the signal was sent with
        /// </summary>
        public int value { get; private set; }

        public ValueEvent(Wire wire, int value) : base(wire)
        {
            this.value = value;
        }
    }
}
