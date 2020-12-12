using System;
using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Info", menuName = "Puzzled/Tile Info")]
    public class TileInfo : ScriptableObject
    {
        [SerializeField] private string _displayName = "";
        [SerializeField] private string description;
        [SerializeField] private bool _allowMultiple = true;
        [SerializeField] private bool _allowDynamic = true;
        [SerializeField] private bool _allowWireInputs = false;
        [SerializeField] private bool _allowWireOuputs = false;
        [SerializeField] private TileLayer _layer = TileLayer.Object;

        [Serializable]
        public struct CustomOptionEditor
        {
            public GameObject prefab;
            public int order;
        }

        [SerializeField] private CustomOptionEditor[] _optionEditors = null;

        public bool allowMultiple => _allowMultiple;
        public bool allowDynamic => _allowDynamic;

        public bool allowWireInputs => _allowWireInputs;
        public bool allowWireOutputs => _allowWireOuputs;

        public string displayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public CustomOptionEditor[] optionEditors => _optionEditors;

        public TileLayer layer => _layer;        
    }
}

