using System;
using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Info", menuName = "Puzzled/Tile Info")]
    public class TileInfo : ScriptableObject
    {
        [SerializeField] private string _displayName = "";
        [SerializeField] private string description;

        [Tooltip("True if this tile can be added to a puzzled more than once")]
        [SerializeField] private bool _allowMultiple = true;

        [Tooltip("True if a dynamic tile can be placed on top on top of this tile")]
        [SerializeField] private bool _allowDynamic = false;

        [Tooltip("True if a tile allows wire inputs")]
        [SerializeField] private bool _allowWireInputs = false;

        [Tooltip("True if a tile allows wire outputs")]
        [SerializeField] private bool _allowWireOuputs = false;
        
        [Tooltip("Layer the tile is linked to")]
        [SerializeField] private TileLayer _layer = TileLayer.Static;

        [Serializable]
        public struct CustomOptionEditor
        {
            public GameObject prefab;
            public int order;
        }

        [SerializeField] private CustomOptionEditor[] _optionEditors = null;

        public bool allowMultiple => _allowMultiple;

        /// <summary>
        /// Returns true if a static tile allows a dynamic tile to be placed on top of it
        /// </summary>
        public bool allowDynamic => _allowDynamic;

        public bool allowWireInputs => _allowWireInputs;
        public bool allowWireOutputs => _allowWireOuputs;

        public string displayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public CustomOptionEditor[] optionEditors => _optionEditors;

        public TileLayer layer => _layer;        
    }
}

