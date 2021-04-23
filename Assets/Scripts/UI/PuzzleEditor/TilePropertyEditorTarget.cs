using System;
using UnityEngine;

namespace Puzzled
{
    public class TilePropertyEditorTarget : IPropertyEditorTarget
    {
        public string id => name;
        public string name => tileProperty.displayName;
        public string placeholder => tileProperty.editable.placeholder;
        public Vector2Int range => tileProperty.editable.range;

        public Tile tile { get; private set; }
        public TileProperty tileProperty { get; private set; }

        public TilePropertyEditorTarget(Tile tile, TileProperty tileProperty)
        {
            this.tile = tile;
            this.tileProperty = tileProperty;
        }

        public void SetValue(object value, bool commit = true)
        {
            if (commit)
                UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(tile, tileProperty.name, value));
            else
                tileProperty.SetValue(tile, value);
        }

        public object GetValue() => tileProperty.GetValue(tile);

        public T GetValue<T>() => tileProperty.GetValue<T>(tile);
    }
}
