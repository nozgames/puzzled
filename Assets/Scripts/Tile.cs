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

        /// <summary>
        /// Cached properties
        /// </summary>
        private TileProperty[] _properties = null;

        /// <summary>
        /// Cached port list
        /// </summary>
        private Port[][] _ports;


        public TileInfo info {
            get => _info;
            set => _info = value;
        }

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
        /// Tile layer
        /// </summary>
        public TileLayer layer => info.layer;

        /// <summary>
        /// Return the tile properties array for this tile
        /// </summary>
        public TileProperty[] properties => _properties == null ? _properties = DatabaseManager.GetProperties(this) : _properties;

        /// <summary>
        /// Cell the tile belongs to
        /// </summary>
        private Cell _cell = Cell.invalid;

        /// <summary>
        /// Inspector state stored on the tile for the next time the tile is selected
        /// </summary>
        public (string id, object value)[] inspectorState { get; set; }

        /// <summary>
        /// Get/Set the puzzle this tile belongs to
        /// </summary>
        public Puzzle puzzle { get; set; }

        /// <summary>
        /// Get the tile grid the tile belongs to.  Note that even if a tile is not linked into the grid
        /// it still belongs to the grid of the parent puzzle.
        /// </summary>
        public TileGrid grid => puzzle.grid;

        /// <summary>
        /// True if the tile has been destroyed
        /// </summary>
        public bool isDestroyed => _pendingDestroy;

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

        public Cell cell {
            get => _cell;
            set {
                // If already linked to this cell early out
                if(IsLinkedTo(value))
                    return;

                Debug.Assert(puzzle != null);

                // If this tile is linked then unlink it from the grid
                if (isLinked)
                    grid.UnlinkTile(this);

                var old = _cell;
                _cell = value;

                if(_cell == Cell.invalid || !grid.LinkTile(this))
                {
                    _cell = Cell.invalid;

                    // Give our own components a chance to react to the cell change
                    Send(new CellChangedEvent(this, old));

                    // Disable the tile since it is not linked into the grid
                    gameObject.SetActive(false);
                    return;
                }

                // Ensure the tile is parented to the grid
                if (transform.parent != grid.transform)
                    transform.SetParent(grid.transform);

                // Ensure the tile is enabled since it is linked into the grid
                gameObject.SetActive(true);

                // Move the tile to the correct world position
                transform.position = grid.CellToWorld(_cell);

                // When a tile changes positions make sure all of its wires are informed
                foreach(var port in GetPorts())
                    foreach (var wire in port.wires)
                        wire.UpdatePositions();

                // Give our own components a chance to react to the cell change
                Send(new CellChangedEvent(this, old));
            }
        }

        /// <summary>
        /// Return true if the tile has a component of the given type
        /// </summary>
        /// <typeparam name="T">Type to search for</typeparam>
        /// <returns>True if the tile has the given component</returns>
        public bool HasTileComponent<T>() => HasTileComponent(typeof(T));

        /// <summary>
        /// Return true if the tile has a component of the given type
        /// </summary>
        /// <param name="type">Component to search for</param>
        /// <returns>True if the tile has the given component</returns>
        public bool HasTileComponent (Type type)
        {
            for (int i = componentCount - 1; i >= 0; i--)
                if (type.IsAssignableFrom(GetComponent<TileComponent>().GetType()))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the tile is linked to the given cell
        /// </summary>
        public bool IsLinkedTo(Cell cell) => grid != null && grid.CellToTile(cell, layer) == this;

        /// <summary>
        /// Return the cell bounds for the given list of tiles
        /// </summary>
        /// <param name="tiles">Tiles</param>
        /// <returns>Cell bounds that encompasses the given list of tiles</returns>
        public static CellBounds GetCellBounds (Tile[] tiles)
        {
            if (tiles.Length == 0)
                return new CellBounds(Cell.invalid, Cell.invalid);

            var min = tiles[0].cell;
            var max = min;
            foreach (var tile in tiles)
            {
                min = Cell.Min(min, tile.cell);
                max = Cell.Max(max, tile.cell);
            }

            return new CellBounds(min, max);
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

            // Unlink ourself from the tile grid
            cell = Cell.invalid;

            // Let all the components know
            Send(new DestroyEvent());

            // Remove us from tick management
            _tick.Remove(this);

            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            // Destroy all connected wires
            foreach (var property in properties)
                if (property.type == TilePropertyType.Port)
                    property.GetValue<Port>(this).Clear();

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
        /// Get the tile property with the given name
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <returns>Tile property if it exists or null</returns>
        public TileProperty GetProperty(string name) => properties.Where(p => p.name == name).FirstOrDefault();

        /// <summary>
        /// Return the value of a property with the given name as a boolean regardless of its type.  Note that
        /// if the property does not exist a value of false will be returned.
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>True if the property's value evaluates to true</returns>
        public bool GetPropertyAsBool (string name)
        {
            var property = GetProperty(name);
            if (null == property)
                return false;

            return property.GetValueAsBool(this);
        }

        /// <summary>
        /// Returns true if the given property is hidden from the inspector
        /// </summary>
        /// <param name="tileProperty">Property</param>
        /// <returns>True if hidden</returns>
        public bool IsPropertyHidden(TileProperty tileProperty)
        {
            if (tileProperty.editable.hidden)
                return true;

            if (!string.IsNullOrEmpty(tileProperty.editable.hiddenIfFalse))
                if (!GetPropertyAsBool(tileProperty.editable.hiddenIfFalse))
                    return true;

            if (!string.IsNullOrEmpty(tileProperty.editable.hiddenIfTrue))
                if (GetPropertyAsBool(tileProperty.editable.hiddenIfTrue))
                    return true;

            return false;
        }

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

        /// <summary>
        /// Returns true if the tile is connected to any tile in the given cell
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <returns>True if connected to a tile in the given cell</returns>
        public bool IsConnectedTo (Cell cell)
        {
            foreach (var port in GetPorts())
                if (port.IsConnectedTo(cell))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the tile can connect to the given tile via at least one port.  This
        /// method takes into account existing connections as well.
        /// </summary>
        /// <param name="tile">Tile to connect to</param>
        /// <returns>True if a connection can be made</returns>
        public bool CanConnectTo (Tile tile, bool allowHidden = true)
        {
            if (tile == null)
                return false;

            var outputs = GetPorts(PortFlow.Output);
            var inputs = tile.GetPorts(PortFlow.Input);

            if (outputs.Length == 0 || inputs.Length == 0)
                return false;

            foreach(var output in outputs)
            {
                // Skip any outputs already connected to the tile
                if (output.IsConnectedTo(tile))
                    continue;

                // Can this output connect to any of the inputs?
                foreach (var input in inputs)
                    if(allowHidden || !input.isHidden)
                        if (output.CanConnectTo(input))
                            return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the tile can connect to any tile in the given cell.  This
        /// method takes into account existing connections as well.
        /// </summary>
        /// <param name="cell">Cell to connect to</param>
        /// <returns>True if a connection can be made</returns>
        public bool CanConnectTo(Cell cell, bool allowHidden = true)
        {
            for (int i = (int)TileLayer.InvisibleStatic; i >= 0; i--)
                if (CanConnectTo(puzzle.grid.CellToTile(cell, (TileLayer)i), allowHidden))
                    return true;

            return false;
        }


        /// <summary>
        /// Returns true if the tile can move through the edge between the current position and the offset
        /// </summary>
        /// <param name="offset">Movement offset</param>
        /// <returns>True if the tile can move through the edge</returns>
        public bool CanMoveThroughEdge(CellEdge edge)
        {
            var queryEdge = new QueryMoveEvent(this, Cell.EdgeToDirection(edge));
            SendToCell(queryEdge, new Cell(CellCoordinateSystem.SharedEdge, cell.x, cell.y, edge), CellEventRouting.FirstVisible);
            if (queryEdge.hasResult && !queryEdge.result)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the tile can move to the cell on the other side of the given edge
        /// </summary>
        /// <param name="edge">Edge to move through</param>
        /// <returns>True if the tile can move</returns>
        public bool CanMove(CellEdge edge)
        {
            // First check to make sure the edge isnt blocked
            if (!CanMoveThroughEdge(edge))
                return false;

            // Can we move wherre we are going?
            var direction = Cell.EdgeToDirection(edge);
            var query = new QueryMoveEvent(this, direction);
            SendToCell(query, cell + direction, CellEventRouting.FirstVisible);
            if (!query.result)
                return false;

            return true;
        }

        /// <summary>
        /// Return an array of ports for the given flow
        /// </summary>
        /// <param name="flow">Port flow</param>
        /// <returns>Ports</returns>
        public Port[] GetPorts(PortFlow flow) 
        {
            GetPorts();
            return _ports[1 + (int)flow];
        }

        /// <summary>
        /// Return an array of all ports
        /// </summary>
        /// <returns></returns>
        public Port[] GetPorts()
        {
            if (null == _ports)
            {
                _ports = new Port[3][];
                var ports = new List<Port>(properties.Length);
                foreach (var property in properties)
                    if (property.type == TilePropertyType.Port)
                        ports.Add(property.GetValue<Port>(this));

                _ports[0] = ports.ToArray();
                _ports[1] = ports.Where(p => p.flow == PortFlow.Input).ToArray();
                _ports[2] = ports.Where(p => p.flow == PortFlow.Output).ToArray();
            }

            return _ports[0];
        }

        public void ShowGizmos(bool show) => Send(new ShowGizmosEvent(show));

        /// <summary>
        /// Return the tile component that matches the given instance identifier
        /// </summary>
        /// <param name="instanceId">Instance identifier to match</param>
        /// <returns>Matched tile component or null if no tile component could be found</returns>
        public TileComponent GetTileComponent (ulong instanceId)
        {
            for(int i=componentCount - 1; i >= 0; i--)
            {
                var tileComponent = GetComponent(i) as TileComponent;
                if (null == tileComponent)
                    continue;

                if (tileComponent.instanceId == instanceId)
                    return tileComponent;
            }

            return null;
        }
    }
}
