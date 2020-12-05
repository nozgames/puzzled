using UnityEngine;
using NoZ;
using System.Collections.Generic;

namespace Puzzled
{
    public class Tile : Actor
    {
        [SerializeField] private TileInfo _info = null;

        private static List<Tile> _tick = new List<Tile>();
        private static TickEvent _tickEvent = new TickEvent();

        private Vector2Int _cell;

        public List<Wire> inputs { get; private set; } = new List<Wire>();
        public List<Wire> outputs { get; private set; } = new List<Wire>();

        public TileInfo info => _info;
           
        /// <summary>
        /// Cell the actor is current in
        /// </summary>
        public Vector2Int cell {
            get => _cell;
            set {
                GameManager.Instance.SetTileCell(this, value);
                _cell = value;
            }
        }

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _tick.Remove(this);

            foreach (var input in inputs)
                Destroy(input.gameObject);

            foreach (var output in outputs)
                Destroy(output.gameObject);

            inputs.Clear();
            outputs.Clear();

            if(GameManager.Instance != null)
                GameManager.Instance.RemoveTileFromCell(this);
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
        public bool allInputsActive => activeInputCount == inputCount;

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
            foreach (var tile in _tick)
                tile.Send(_tickEvent);
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
    }
}
