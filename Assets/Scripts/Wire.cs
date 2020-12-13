﻿using UnityEngine;

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
            public Tile tile;

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

        [SerializeField] private WireMesh _visuals = null;

        public Connection from { get; private set; } = new Connection();
        public Connection to { get; private set; } = new Connection();

        private int _value = 0;

        public bool selected {
            get => _visuals.selected;
            set => _visuals.selected = value;
        }

        /// <summary>
        /// Current value of the wire
        /// </summary>
        public int value {
            get => _value;
            set {
                if (_value == value)
                    return;

                _value = value;

                if(enabled)
                    to.tile.Send(new WireValueChangedEvent(this));
            }
        }
        
        public bool visible {
            get => _visuals.gameObject.activeSelf;
            set {
                _visuals.target = to.cell;
                _visuals.gameObject.SetActive(value);
            }
        }

        private void OnEnable()
        {
            // Send a wire activation event
            to.tile.Send(new WireActivatedEvent(this));

            // Also send a wire value changed event for convienence
            to.tile.Send(new WireValueChangedEvent(this));

            _visuals.target = to.tile.cell;
        }

        private void OnDisable()
        {
            to.tile.Send(new WireDeactivatedEvent(this));
        }

        private void OnDestroy()
        {
            if(from != null)
                from.tile.outputs.Remove(this);

            if(to != null)
                to.tile.inputs.Remove(this);
        }
    }
}
