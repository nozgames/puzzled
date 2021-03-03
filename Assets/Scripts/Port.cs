using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public enum PortType
    {
        Power,
        Signal,
        Number
    }

    public enum PortFlow
    {
        Input,
        Output
    }

    [Flags]
    public enum PortFlags
    {
        None = 0,
        AllowSelfWire = 1
    }

    public class PortAttribute : Attribute
    {
        public PortType type { get; private set; }
        public PortFlow flow { get; private set; }
        public PortFlags flags { get; private set; }
        public Type signalEvent { get; set; }
        
        /// <summary>
        /// The legacy port is the port that V1 files will connect their inputs to
        /// </summary>
        public bool legacy { get; set; }

        public PortAttribute(PortFlow flow, PortType type, PortFlags flags = PortFlags.None) 
        {
            this.type = type;
            this.flow = flow;
            this.flags = flags;
        }
    }

    public class Port
    {
        /// <summary>
        /// List of all wires attached to the port
        /// </summary>
        public List<Wire> wires { get; private set; } = new List<Wire>();

        /// <summary>
        /// Property that this port represents
        /// </summary>
        private TileProperty _property;

        /// <summary>
        /// Tile the port is attached to
        /// </summary>
        public Tile tile { get; private set; }

        /// <summary>
        /// Returns the port type
        /// </summary>
        public PortType type => _property.port.type;

        /// <summary>
        /// Port flow type
        /// </summary>
        public PortFlow flow => _property.port.flow;

        /// <summary>
        /// Port flags
        /// </summary>
        public PortFlags flags => _property.port.flags;

        /// <summary>
        /// Returns the port signal event type
        /// </summary>
        public Type signalEventType => _property.port.signalEvent;

        /// <summary>
        /// True if this port is the legacy port that inputs were connected to
        /// </summary>
        public bool isLegacy => _property.port.legacy;

        /// <summary>
        /// Is the port hidden in the editor?
        /// </summary>
        public bool isHidden => tile.IsPropertyHidden(_property);

        /// <summary>
        /// Name of the port
        /// </summary>
        public string name => _property.name;

        /// <summary>
        /// Display name of the port
        /// </summary>
        public string displayName => _property.displayName;

        /// <summary>
        /// Number of wires connected to the port
        /// </summary>
        public int wireCount => wires.Count;

        /// <summary>
        /// True if one or more wires are powered
        /// </summary>
        public bool hasPower {
            get {
                foreach (var wire in wires)
                    if (wire.hasPower)
                        return true;
                
                return false;
            }
        }

        private bool _desiredPowered = false;

        public Port(Tile tile, TileProperty tileProperty)
        {
            this.tile = tile;
            _property = tileProperty;
        }

        /// <summary>
        /// Remove all connected wires
        /// </summary>
        public void Clear()
        {
            foreach (var wire in wires)
                GameObject.Destroy(wire.gameObject);

            wires.Clear();
        }

        /// <summary>
        /// Returns true if the port is connected to the given port
        /// </summary>
        /// <param name="port">Port to check</param>
        /// <returns>True if the two ports are connected</returns>
        public bool IsConnectedTo(Port port)
        {
            foreach (var wire in wires)
                if (wire.to.port == port || wire.from.port == port)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the port is connected to a given tile
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>True if the port is connected to the tile</returns>
        public bool IsConnectedTo(Tile tile)
        { 
            foreach (var wire in wires)
                if (wire.GetOppositeConnection(this).tile == tile)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the port is connected to a given cell
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <returns>True if connected to the given cell</returns>
        public bool IsConnectedTo(Cell cell)
        {
            foreach (var wire in wires)
                if (wire.GetOppositeConnection(this).cell == cell)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the port can connect to the given port.
        /// </summary>
        /// <param name="port">Port to check</param>
        /// <returns>True if a connection can be made to the given port</returns>
        public bool CanConnectTo(Port port) => 
            (type == port.type || (type == PortType.Power && port.type == PortType.Signal)) &&
            (port.flags.HasFlag(PortFlags.AllowSelfWire) || tile != port.tile);

        /// <summary>
        /// Get the wire at the given index
        /// </summary>
        /// <param name="index">Wire index</param>
        /// <returns>Wire</returns>
        public Wire GetWire(int index) => wires[index];

        /// <summary>
        /// Send number value to the target ports
        /// </summary>
        /// <param name="value"Value to send></param>
        public void SendValue(int value, bool persist=false)
        {
            if (type != PortType.Number || flow != PortFlow.Output)
            {
                Debug.LogWarning("SendValue requires number output port");
                return;
            }

            foreach (var wire in wires)
                wire.SendValue(value, persist);
        }

        /// <summary>
        /// Send signal to all wires
        /// </summary>
        public void SendSignal ()
        {
            if (type != PortType.Signal || flow != PortFlow.Output)
            {
                Debug.LogWarning("SendSignal requires signal output port");
                return;
            }

            foreach (var wire in wires)
                wire.SendSignal();
        }

        /// <summary>
        /// Set the powered state of all wires
        /// </summary>
        /// <param name="powered">Powered state</param>
        public void SetPowered (bool powered)
        {
            if (type != PortType.Power || flow != PortFlow.Output)
            {
                Debug.LogWarning("SetPowered requires power output port");
                return;
            }

            // this is to avoid issues with changing powered state causing the desired powered state to change while we are looping
            _desiredPowered = powered;

            foreach (var wire in wires)
                wire.enabled = _desiredPowered;
        }

        /// <summary>
        /// Set the powered state for a given wire
        /// </summary>
        /// <param name="wireIndex">Wire index</param>
        /// <param name="powered">Wire power state</param>
        public void SetPowered (int wireIndex, bool powered)
        {
            if(type != PortType.Power || flow != PortFlow.Output)
            {
                Debug.LogWarning("SetPowered requires power output port");
                return;
            }

            wires[wireIndex].enabled = powered;
        }

        /// <summary>
        /// Return the wire connection for the given wire index
        /// </summary>
        /// <param name="wireIndex">index of wire</param>
        /// <returns>Wire connection that represents the given port</returns>
        private Wire.Connection GetWireConnection(int wireIndex) => GetWire(wireIndex).GetConnection(this);

        /// <summary>
        /// Set an option for a specific wire
        /// </summary>
        /// <param name="wireIndex">Index of wire to set option for</param>
        /// <param name="option">Index of option</param>
        /// <param name="value">Value to set</param>
        public void SetWireOption (int wireIndex, int option, int value) =>
            GetWireConnection(wireIndex).SetOption(option, value);

        /// <summary>
        /// Get an option value for the given wire
        /// </summary>
        /// <param name="wireIndex">Wire index</param>
        /// <param name="option">Option to retrieve</param>
        /// <returns></returns>
        public int GetWireOption (int wireIndex, int option) =>
            GetWireConnection(wireIndex).GetOption(option);
    }
}
