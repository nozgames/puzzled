using System;
using System.Collections.Generic;
using UnityEngine;
using NoZ;

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

    public class PortAttribute : Attribute
    {
        public PortType type { get; private set; }
        public PortFlow flow { get; private set; }
        public Type signalEvent { get; set; }
        
        /// <summary>
        /// The legacy port is the port that V1 files will connect their inputs to
        /// </summary>
        public bool legacy { get; set; }

        public PortAttribute(PortFlow flow, PortType type) 
        {
            this.type = type;
            this.flow = flow;
        }
    }

    public class Port
    {
        /// <summary>
        /// List of all wires attached to the port
        /// </summary>
        public List<Wire> wires { get; private set; } = new List<Wire>();

        /// <summary>
        /// Port attributes
        /// </summary>
        private PortAttribute _attribute;

        /// <summary>
        /// Tile the port is attached to
        /// </summary>
        public Tile tile { get; private set; }

        /// <summary>
        /// Returns the port type
        /// </summary>
        public PortType type => _attribute.type;

        /// <summary>
        /// Port flow type
        /// </summary>
        public PortFlow flow => _attribute.flow;

        /// <summary>
        /// Returns the port signal event type
        /// </summary>
        public Type signalEventType => _attribute.signalEvent;

        /// <summary>
        /// True if this port is the legacy port that inputs were connected to
        /// </summary>
        public bool isLegacy => _attribute.legacy;

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
                    if (wire.isPowered)
                        return true;
                
                return false;
            }
        }

        public Port(Tile tile, PortAttribute attr)
        {
            this.tile = tile;
            _attribute = attr;
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
        /// Returns true if the given port is connected to this port
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
        /// Get the wire at the given index
        /// </summary>
        /// <param name="index">Wire index</param>
        /// <returns>Wire</returns>
        public Wire GetWire(int index) => wires[index];

        /// <summary>
        /// Send number value to the target ports
        /// </summary>
        /// <param name="value"Value to send></param>
        public void SendValue(int value)
        {
            if (type != PortType.Number || flow != PortFlow.Output)
            {
                Debug.LogWarning("SendValue requires number port output port");
                return;
            }

            foreach (var wire in wires)
                wire.SendValue(value);
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

            foreach(var wire in wires)
                wire.enabled = powered;
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
        private Wire.Connection GetWireConnection(int wireIndex)
        {
            var wire = GetWire(wireIndex);
            if (wire.from.port == this)
                return wire.from;
            return wire.to;
        }

        /// <summary>
        /// Set an option for a specific wire
        /// </summary>
        /// <param name="wireIndex">Index of wire to set option for</param>
        /// <param name="option">Index of option</param>
        /// <param name="value">Value to set</param>
        public void SetOption(int wireIndex, int option, int value) =>
            GetWireConnection(wireIndex).SetOption(option, value);

        /// <summary>
        /// Get an option value for the given wire
        /// </summary>
        /// <param name="wireIndex">Wire index</param>
        /// <param name="option">Option to retrieve</param>
        /// <returns></returns>
        public int GetOption(int wireIndex, int option) =>
            GetWireConnection(wireIndex).GetOption(option);
    }



    public class SignalEvent : WireEvent
    {
        public SignalEvent(Wire wire) : base(wire) { }
    }

    public class ToggleSignalEvent : SignalEvent 
    {
        public ToggleSignalEvent(Wire wire) : base(wire) { }
    }
    
    public class ResetSignalEvent : SignalEvent 
    {
        public ResetSignalEvent(Wire wire) : base(wire) { }
    }

    public class OffSignalEvent : SignalEvent
    {
        public OffSignalEvent(Wire wire) : base(wire) { }
    }

    public class OnSignalEvent : SignalEvent
    {
        public OnSignalEvent(Wire wire) : base(wire) { }
    }

    public class ValueSignalEvent : SignalEvent
    {
        /// <summary>
        /// Value that the signal was sent with
        /// </summary>
        public int value { get; private set; }

        public ValueSignalEvent(Wire wire, int value) : base(wire) 
        {
            this.value = value;
        }
    }

    public class WireEvent : ActorEvent
    {
        public Wire wire { get; private set; }
        public Port port => wire.to.port;

        public WireEvent (Wire wire)
        {
            this.wire = wire;
        }
    }

    /// <summary>
    /// Sent when the power of a wire changes for a given port
    /// </summary>
    public class WirePowerEvent : WireEvent
    {
        public bool isPowered => wire.enabled;
        public bool isOff => wire.enabled;

        public WirePowerEvent (Wire wire) : base(wire)
        {
        }
    }
}
