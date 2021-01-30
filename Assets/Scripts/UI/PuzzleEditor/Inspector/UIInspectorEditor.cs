using System;
using UnityEngine;

namespace Puzzled.Editor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class InspectorPriorityAttribute : Attribute
    {
        private int _priority;
        public int priority => _priority;

        public InspectorPriorityAttribute(int priority) => _priority = priority;
    }

    public class UIInspectorEditor : MonoBehaviour
    {
        private Tile _tile;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;
                OnTargetChanged(tile);
            }
        }

        protected virtual void OnTargetChanged (Tile tile)
        {

        }
    }
}
