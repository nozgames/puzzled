using System;

namespace Puzzled
{
    public class TilePropertyEditorTarget
    {
        public Tile tile { get; private set; }
        public TileProperty tileProperty { get; private set; }
        public string name { get; private set; }

        public TilePropertyEditorTarget(Tile tile, TileProperty tileProperty)
        {
            this.tile = tile;
            this.tileProperty = tileProperty;
            name = tileProperty.info.Name;
            if (name.EndsWith("Port"))
                name = name.Substring(0, name.Length - 4);
            
            name = name.NicifyName();

        }

        public void SetValue(object value) => tileProperty.SetValue(tile, value);

        public object GetValue() => tileProperty.GetValue(tile);

        public T GetValue<T>() => tileProperty.GetValue<T>(tile);
    }
}
