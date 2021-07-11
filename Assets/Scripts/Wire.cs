using System;
using UnityEngine;

namespace Puzzled
{
    public class Wire : MonoBehaviour
    {
        /// <summary>
        /// Represents a wire connection
        /// </summary>
        public class Connection
        {
            /// <summary>
            /// Maximum number of options per connection
            /// </summary>
            public const int MaxOptions = 2;

            /// <summary>
            /// Connected tile
            /// </summary>
            public Tile tile => port.tile;

            /// <summary>
            /// Optional parameters
            /// </summary>
            public int[] options;

            /// <summary>
            /// Cell of the connected tile
            /// </summary>
            public Cell cell => tile.cell;

            /// <summary>
            /// True if the connection has options
            /// </summary>
            public bool hasOptions => options != null;

            /// <summary>
            /// World position of the connected tile
            /// </summary>
            public Vector3 position => tile.transform.position;

            /// <summary>
            /// Port the wire is connected to.
            /// </summary>
            public Port port { get; set; }

            /// <summary>
            /// Sets the option at the given index
            /// </summary>
            /// <param name="index">Option index</param>
            /// <param name="value">Option value</param>
            public void SetOption (int index, int value)
            {
                if (options == null)
                    options = new int[MaxOptions];

                options[index] = value;
            }

            /// <summary>
            /// Set options using an array of values
            /// </summary>
            /// <param name="values">Value array</param>
            public void SetOptions (int[] values)
            {
                if (null == values)
                    return;

                for (int i = 0; i < values.Length && i < MaxOptions; i++)
                    SetOption(i, values[i]);
            }

            /// <summary>
            /// Returns the option at the given index
            /// </summary>
            /// <param name="index">Option index</param>
            /// <returns>Value of option</returns>
            public int GetOption(int index) => options?[index] ?? 0;
        }

        [SerializeField] private WireVisuals _visuals = null;

        public Connection from { get; private set; } = new Connection();
        public Connection to { get; private set; } = new Connection();

        /// <summary>
        /// Return the value that is persisted on the wire
        /// </summary>
        public int value { get; private set; } = -1;

        /// <summary>
        /// Puzzle the wire belongs to
        /// </summary>
        public Puzzle puzzle { get; set; }

        /// <summary>
        /// True if the wire is being edited
        /// </summary>
        public bool isEditing => puzzle.isEditing;

        /// <summary>
        /// True if the wire is powered
        /// </summary>
        public bool hasPower => enabled;

        /// <summary>
        /// Returns true if the wire has a persistent value
        /// </summary>
        public bool hasValue => value >= 0;

        /// <summary>
        /// Control the wire visual state
        /// </summary>
        public WireVisuals visuals => _visuals;

        public bool visible {
            get => _visuals.gameObject.activeSelf;
            set {
               UpdatePositions();
                _visuals.gameObject.SetActive(value);
            }
        }

        private void OnEnable()
        {
            _visuals.flowing = true;

            // When a wire is powered and the output port is a port port then
            // send a power event to the tile
            if (to.port.type == PortType.Power)
                to.tile.Send(new WirePowerChangedEvent(this));
            // When a wire is powered and is connected to a signal port then fire the signal
            else if (to.port.type == PortType.Signal)
                SendSignalInternal();

            UpdatePositions();
        }

        private void OnDisable()
        {
            _visuals.flowing = false;

            if (to?.port?.type == PortType.Power)
                to.tile.Send(new WirePowerChangedEvent(this));
        }

        private void OnDestroy()
        {
            from.port?.wires.Remove(this);
            to.port?.wires.Remove(this);
        }

        public void Destroy()
        {
            from.port?.wires.Remove(this);
            to.port?.wires.Remove(this);
            from.port = null;
            to.port = null;
            transform.SetParent(null);
            Destroy(gameObject);
        }

        /// <summary>
        /// Return the connection that is connected to the given tile
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>Connection or null if not connected to the tile</returns>
        public Connection GetConnection (Tile tile) => from.tile == tile ? from : (to.tile == tile ? to : null);

        /// <summary>
        /// Return the connection that is connected to the given port
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns>Connection or null if not connecte to the port</returns>
        public Connection GetConnection(Port port) => from.port == port ? from : (to.port == port ? to : null);

        /// <summary>
        /// Return the connection that opposite to the given tile
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>Connection or null if not connected to the tile</returns>
        public Connection GetOppositeConnection (Tile tile) => from.tile == tile ? to : (to.tile == tile ? from : null);

        /// <summary>
        /// Return the connection that is opposite to the given port
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns>Connection or null if not connecte to the port</returns>
        public Connection GetOppositeConnection (Port port) => from.port == port ? to : (to.port == port ? from : null);


        public void UpdatePositions()
        {
            transform.position = from.tile.wireAttach.position;
            _visuals.target = to.tile.wireAttach.position;

            _visuals.portTypeFrom = from.port.type;
            _visuals.portTypeTo = to.port.type;
        }

        /// <summary>
        /// Signal a value port with the given value
        /// </summary>
        /// <param name="value">Value to signal with</param>
        public void SendValue(int value, bool persist = false)
        {
            if (from.port.type != PortType.Number || from.port.flow != PortFlow.Output)
            {
                Debug.LogWarning("SendValue requires a number output port");
                return;
            }

            if (persist)
                this.value = value;

            // Send generic value signal event if no signal event was specified
            if (null == to.port.signalEventType)
            {
                to.tile.Send(new ValueEvent(this,value));
                return;
            }

            // Create the custom signal event
            var evt = Activator.CreateInstance(to.port.signalEventType, this, value) as ValueEvent;
            if(null == evt)
            {
                Debug.LogError($"Failed to create signal event of type '{to.port.signalEventType.Name}'");
                return;
            }

            // Send the event to the tile
            to.tile.Send(evt);
        }

        private void SendSignalInternal ()
        {
            // Send generic value signal event if no signal event was specified
            if (null == to.port.signalEventType)
            {
                to.tile.Send(new SignalEvent(this));
                return;
            }

            // Create the custom signal event
            var evt = Activator.CreateInstance(to.port.signalEventType, new object[] { this } ) as SignalEvent;
            if (null == evt)
            {
                Debug.LogError($"Failed to create signal event of type '{to.port.signalEventType.Name}'");
                return;
            }

            // Send the event to the tile
            to.tile.Send(evt);
        }

        /// <summary>
        /// Signal the target
        /// </summary>
        public void SendSignal()
        {
            if (from.port.flow != PortFlow.Output || from.port.type != PortType.Signal)
            {
                Debug.LogWarning($"SendSignal requires a signal output port");
                return;
            }

            SendSignalInternal();
        }

#if false
        public static Tuple<Port,Port>[] GetValidConnections (Tile from, Tile to)
        {
            
        }
#endif
    }
}
