﻿using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Info", menuName = "Puzzled/Tile Info")]
    public class TileInfo : ScriptableObject
    {
        [SerializeField] private string displayName = "";
        [SerializeField] private string description;
        [SerializeField] private Tile[] _prefabs = null;
        [SerializeField] private bool _allowMultiple = true;
        [SerializeField] private bool _allowDynamic = true;
        [SerializeField] private TileLayer _layer = TileLayer.Static;

        public Tile[] prefabs => _prefabs;

        public bool allowMultiple => _allowMultiple;
        public bool allowDynamic => _allowDynamic;

        public TileLayer layer => _layer;
    }
}

