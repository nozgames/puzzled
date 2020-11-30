using UnityEngine;
using NoZ;
using System.Collections.Generic;

namespace Puzzled
{
    public class Tile : Actor
    {
        [SerializeField] private TileInfo _info = null;

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

        public int inputCount => inputs.Count;
        public int outputCount => outputs.Count;

        public bool allInputsActive => activeInputCount == inputCount;

        public bool hasActiveInput => activeInputCount > 0;

        public bool HasOutput (Tile tile)
        {
            if (null == outputs)
                return false;
            
            foreach(var output in outputs)
                if (tile == output.output)
                    return true;

            return false;
        }

        public bool HasInput (Tile tile)
        {
            if (null == inputs)
                return false;

            foreach (var input in inputs)
                if (tile == input.input)
                    return true;

            return false;
        }

        public void SetOutputsActive (bool active)
        {
            if (null == outputs)
                return;

            foreach (var output in outputs)
                output.enabled = active;
        }

        public void PulseOutputs()
        {
            SetOutputsActive(true);
            SetOutputsActive(false);
        }
    }
}
