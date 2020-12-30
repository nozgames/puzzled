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
        private static bool _tickFrame = false;

        public List<Wire> inputs { get; private set; } = new List<Wire>();
        public List<Wire> outputs { get; private set; } = new List<Wire>();

        public TileInfo info => _info;

        /// <summary>
        /// True if the tile is being edited
        /// </summary>
        public bool isEditing => puzzle.isEditing;

        /// <summary>
        /// Is currently on a tick frame
        /// </summary>
        public bool isTickFrame => _tickFrame;

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

                foreach (var input in inputs)
                    input.UpdatePositions();

                foreach (var output in outputs)
                    output.UpdatePositions();

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

            // Destroy all input wires
            foreach (var input in inputs)
                Destroy(input.gameObject);

            // Destroy all output wires
            foreach (var output in outputs)
                Destroy(output.gameObject);

            inputs.Clear();
            outputs.Clear();

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

        /// <summary>
        /// Returns the number of active inputs
        /// </summary>
        public int activeInputCount {
            get {
                if (inputs == null)
                    return 0;

                var count = 0;
                foreach (var input in inputs)
                    if (input.enabled)
                        count++;

                return count;
            }
        }

        /// <summary>
        /// Total number of inputs
        /// </summary>
        public int inputCount => inputs.Count;

        /// <summary>
        /// Total number of outputs
        /// </summary>
        public int outputCount => outputs.Count;

        /// <summary>
        /// True if all inputs are active
        /// </summary>
        public bool allInputsActive => hasActiveInput && activeInputCount == inputCount;

        /// <summary>
        /// True if the tile has at least one active input
        /// </summary>
        public bool hasActiveInput => activeInputCount > 0;

        /// <summary>
        /// Returns true if the given tile is an output of the tile
        /// </summary>
        /// <param name="tile">Tile to check</param>
        /// <returns>True if the given tile is an output of the tile</returns>
        public bool HasOutput (Tile tile)
        {
            if (null == outputs)
                return false;
            
            foreach(var output in outputs)
                if (tile == output.to.tile)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if the tile has an input from the given tile
        /// </summary>
        /// <param name="tile">Tile to check</param>
        /// <returns>True if the given tile is an input of the tile</returns>
        public bool HasInput (Tile tile)
        {
            if (null == inputs)
                return false;

            foreach (var input in inputs)
                if (tile == input.from.tile)
                    return true;

            return false;
        }

        /// <summary>
        /// Set all outputs to the given active state
        /// </summary>
        /// <param name="active">New active state</param>
        public void SetOutputsActive (bool active)
        {
            if (null == outputs)
                return;

            foreach (var output in outputs)
                output.enabled = active;
        }

        /// <summary>
        /// Set the active state for a given output
        /// </summary>
        /// <param name="output"></param>
        /// <param name="active"></param>
        public void SetOutputActive(int output, bool active)
        {
            if (null == outputs)
                return;

            Debug.Assert(output < outputs.Count);
            outputs[output].enabled = active;
        }

        /// <summary>
        /// Set the output value for a specific output
        /// </summary>
        /// <param name="output">Index of the output</param>
        /// <param name="value">Output value</param>
        public void SetOutputValue(int output, int value) => outputs[output].value = value;        

        /// <summary>
        /// Set the output value for all outputs to the given value
        /// </summary>
        /// <param name="value">New value of the output</param>
        public void SetOutputValue(int value)
        {
            foreach (var output in outputs)
                output.value = value;
        }

        /// <summary>
        /// Activate and then deactivate all outputs
        /// </summary>
        public void PulseOutputs()
        {
            SetOutputsActive(true);
            SetOutputsActive(false);
        }

        /// <summary>
        /// Set an option for a specific input
        /// </summary>
        /// <param name="input">Index of input to set option for</param>
        /// <param name="option">Index of option</param>
        /// <param name="value">Value to set</param>
        public void SetInputOption(int input, int option, int value) => 
            inputs[input].to.SetOption(option, value);

        /// <summary>
        /// Get an option value for a specific input
        /// </summary>
        /// <param name="input">Index of the input</param>
        /// <param name="option">Index of the option</param>
        /// <returns>Value of the option</returns>
        public int GetInputOption(int input, int option) =>
            inputs[input].to.GetOption(option);

        /// <summary>
        /// Set an option for a specific output
        /// </summary>
        /// <param name="output">Index of the output</param>
        /// <param name="option">Index of the option</param>
        /// <param name="value">Value of the option</param>
        public void SetOutputOption(int output, int option, int value) =>
            outputs[output].from.SetOption(option, value);

        /// <summary>
        /// Get an option value for a specific output
        /// </summary>
        /// <param name="output">Index of the output</param>
        /// <param name="option">Index of the option</param>
        /// <returns>Value of the option</returns>
        public int GetOutputOption(int output, int option) =>
            outputs[output].from.GetOption(option);

        /// <summary>
        /// Return the index of the given input wire within inputs list
        /// </summary>
        /// <param name="input">Input wire</param>
        /// <returns>The index of the input wire within the inputs list or -1 if not found</returns>
        public int GetInputIndex (Wire input) => inputs.FindIndex(i => i == input);

        /// <summary>
        /// Return the index of the given output wire within the outputs list
        /// </summary>
        /// <param name="output">Output wire</param>
        /// <returns>The index of the output wire within the outputs list or -1 if not found</returns>
        public int GetOutputIndex(Wire output) => outputs.FindIndex(o => o == output);

        private void SetWireIndex(Wire wire, int index, List<Wire> wires, bool insertNull = false)
        {
            // Remove the wire
            if (!wires.Remove(wire))
                return;

            // If the list isnt big enough then add empties (this is for loading)
            if (insertNull)
                for (int i = index - wires.Count; i > 0; i--)
                    wires.Add(null);

            // Insert the wire back into the list
            wires.Insert(index, wire);
        }

        /// <summary>
        /// Set the index of the given input within the inputs list
        /// </summary>
        /// <param name="input">Input Wire</param>
        /// <param name="index">New Index</param>
        /// <param name="insertEmpty">If true insert null wires in the inputs list to ensure the index can be set</param>
        public void SetInputIndex(Wire input, int index, bool insertNull = false) =>
            SetWireIndex(input, index, inputs, insertNull);

        /// <summary>
        /// Set the index of the given output within the outputs list
        /// </summary>
        /// <param name="output">Output wire</param>
        /// <param name="index">New Index</param>
        /// <param name="insertNull">If true insert null wires in the outputs list to ensure the index can be set</param>
        public void SetOutputIndex(Wire output, int index, bool insertNull = false) =>
            SetWireIndex(output, index, outputs, insertNull);

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
            _tickFrame = true;
            foreach (var tile in _tick)
                tile.Send(_tickEvent);
            _tickFrame = false;
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
        public object GetPropertyValue(string name) => GetProperty(name).GetValue(this);

        /// <summary>
        /// Return the property value for the given property and cast it to the given type
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="name">Name of property</param>
        /// <returns>Property value</returns>
        public T GetPropertyValue<T>(string name) => (T)GetPropertyValue(name);
    }
}
