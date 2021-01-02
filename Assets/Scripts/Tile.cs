using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Tile : Actor
    {
        [SerializeField] private TileInfo _info = null;

        private static List<Tile> _tick = new List<Tile>();
        private static TickEvent _tickEvent = new TickEvent();
        private static bool _isTickFrame = false;
        private static int _tickFrame = 1;

        public TileInfo info => _info;

        /// <summary>
        /// True if the tile is being edited
        /// </summary>
        public bool isEditing => puzzle.isEditing;

        /// <summary>
        /// Is currently on a tick frame
        /// </summary>
        public bool isTickFrame => _isTickFrame;

        /// <summary>
        /// current tick frame index
        /// </summary>
        public int tickFrame => _tickFrame;

        /// <summary>
        /// Return the unique identifier for this tile type
        /// </summary>
        public System.Guid guid { get; set; }

        /// <summary>
        /// Return the tile properties array for this tile
        /// </summary>
        public TileProperty[] properties => TileDatabase.GetProperties(this);

        /// <summary>
        /// Current cell
        /// </summary>
        private Cell _cell = Cell.invalid;

        /// <summary>
        /// Inspector state stored on the tile for the next time the tile is selected
        /// </summary>
        public Editor.IInspectorState[] inspectorState { get; set; }

        /// <summary>
        /// Get/Set the puzzle this tile belongs to
        /// </summary>
        public Puzzle puzzle { get; set; }

        /// <summary>
        /// True if the tile has one or more outputs
        /// </summary>
        public bool hasOutputs {
            get {
                // TODO: cache somehow (runtime info that stores properties?)

                foreach (var property in properties)
                    if (property.type == TilePropertyType.Port && property.port.flow == PortFlow.Output)
                        return true;

                return false;
            }
        }

        /// <summary>
        /// True if the tile has one ore more inputs
        /// </summary>
        public bool hasInputs {
            get {
                foreach (var property in properties)
                    if (property.type == TilePropertyType.Port && property.port.flow == PortFlow.Input)
                        return true;

                return false;
            }
        }

        /// <summary>
        /// Current cell
        /// </summary>
        public Cell cell {
            get => _cell;
            set {
                if (_cell == value)
                    return;

                Debug.Assert(puzzle != null);

                // Unlink ourselves from the tile we are in
                if (isLinked && !puzzle.grid.isLinking)
                    puzzle.grid.UnlinkTile(this);

                var old = _cell;
                _cell = value;

                if (_cell == Cell.invalid)
                    return;

                transform.position = puzzle.grid.CellToWorld(_cell);

                // TODO: maintain a list of ports in tile?
                foreach (var property in properties)
                    if (property.type == TilePropertyType.Port)
                        foreach (var wire in property.GetValue<Port>(this).wires)
                            wire.UpdatePositions();

                if (!puzzle.grid.isLinking)
                    puzzle.grid.LinkTile(this);

                // Give our own components a chance to react to the cell change
                Send(new CellChangedEvent(this, old));
            }
        }

        /// <summary>
        /// Send an event to the actor and return true if it was handled
        /// </summary>
        /// <param name="evt">Event to send</param>
        /// <returns>True if the event has handled</returns>
        public new bool Send(ActorEvent evt)
        {
            base.Send(evt);
            return evt.IsHandled;
        }

        /// <summary>
        /// Returns true if the tile is linked into the tile grid
        /// </summary>
        public bool isLinked => cell != Cell.invalid;

        /// <summary>
        /// True if Destroy has been called but not OnDestroy 
        /// </summary>
        private bool _pendingDestroy = false;
           
        /// <summary>
        /// Send an event to a specific cell
        /// </summary>
        /// <param name="evt">Event to send</param>
        /// <param name="cell">Cell to send to</param>
        /// <param name="routing">Routing to use for event</param>
        public void SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing=CellEventRouting.All) => 
            puzzle.grid.SendToCell(evt, cell, routing);

        public void Destroy()
        {
            if (_pendingDestroy)
                return;

            _pendingDestroy = true;

            // Remove us from tick management
            _tick.Remove(this);

            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            // Destroy all connected wires
            foreach (var property in properties)
                if (property.type == TilePropertyType.Port)
                    property.GetValue<Port>(this).Clear();

            // Unlink the tile from the tile grid
            if (isLinked && !puzzle.grid.isLinking)
                puzzle.grid.UnlinkTile(this);

            cell = Cell.invalid;

            if (GameManager.isQuitting)
                DestroyImmediate(gameObject);
            else
                Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!_pendingDestroy)
                Debug.LogWarning("Tile destroyed without calling Tile.Destroy");
        }

        protected override void OnCallbackRegistered(System.Type eventType)
        {
            base.OnCallbackRegistered(eventType);

            if(eventType == typeof(TickEvent))
                _tick.Add(this);
        }

        protected override void OnCallbackUnregistered(System.Type eventType)
        {
            base.OnCallbackUnregistered(eventType);

            if (eventType == typeof(TickEvent) && !_tick.Contains(this))
                _tick.Remove(this);
        }

        public static void Tick ()
        {
            _isTickFrame = true;
            ++_tickFrame;
            foreach (var tile in _tick)
                tile.Send(_tickEvent);
            _isTickFrame = false;
        }

        protected override void OnEnable()
        {
            if (HandlesEvent(typeof(TickEvent)) && !_tick.Contains(this))
                _tick.Add(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            _tick.Remove(this);
            base.OnDisable();
        }

        /// <summary>
        /// Deprecated (Use SetProperty)
        /// </summary>
        public void SetPropertyDeprecated (string name, string value)
        {
            var property = GetProperty(name);
            if (null == property)
                return;

            switch (property.type)
            {
                case TilePropertyType.Int:
                    property.SetValue(this, int.TryParse(value, out var parsedInt) ? parsedInt : 0);
                    break;

                case TilePropertyType.Bool:
                    property.SetValue(this, bool.TryParse(value, out var parsedBool) ? parsedBool : false);
                    break;

                case TilePropertyType.Guid:
                    property.SetValue(this, Guid.TryParse(value, out var parsedGuid) ? parsedGuid : Guid.Empty);
                    break;

                case TilePropertyType.String:
                    property.SetValue(this, value);
                    break;

                case TilePropertyType.StringArray:
                    property.SetValue(this, value.Split(','));
                    break;

                case TilePropertyType.Decal:
                    property.SetValue(this, DecalDatabase.GetDecal(Guid.TryParse(value, out var decalGuid) ? decalGuid : Guid.Empty));
                    break;

                case TilePropertyType.Tile:
                    property.SetValue(this, TileDatabase.GetTile(Guid.TryParse(value, out var tileGuid) ? tileGuid : Guid.Empty));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get the tile property with the given name
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <returns>Tile property if it exists or null</returns>
        public TileProperty GetProperty(string name) => properties.Where(p => p.info.Name == name).FirstOrDefault();

        /// <summary>
        /// Set the tile property with the given name to the given value
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetPropertyValue (string name, object value) => GetProperty(name)?.SetValue(this, value);

        /// <summary>
        /// Return the property value for the given property
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>Property value</returns>
        public object GetPropertyValue(string name) => GetProperty(name)?.GetValue(this);

        /// <summary>
        /// Return the property value for the given property and cast it to the given type
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="name">Name of property</param>
        /// <returns>Property value</returns>
        public T GetPropertyValue<T>(string name) => (T)GetPropertyValue(name);

        /// <summary>
        /// Return the legacy port 
        /// </summary>
        /// <returns>Port</returns>
        public Port GetLegacyPort (PortFlow flow)
        {
            foreach (var property in properties)
                if (property.type == TilePropertyType.Port && property.port.flow == flow && property.port.legacy)
                    return property.GetValue<Port>(this);

            return null;
        }

        /// <summary>
        /// Return the input port with the given name
        /// </summary>
        /// <param name="name">Name of the port</param>
        /// <returns>Input port</returns>
        public Port GetPort (string name)
        {
            foreach (var property in properties)
                if (property.type == TilePropertyType.Port && property.name == name)
                    return property.GetValue<Port>(this);

            return null;
        }
    }
}
