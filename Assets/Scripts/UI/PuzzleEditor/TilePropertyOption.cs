using System;

namespace Puzzled
{
    class TilePropertyOption
    {
        public Tile tile { get; private set; }
        public TileProperty tileProperty { get; private set; }
        public string name { get; private set; }

        public TilePropertyOption(Tile tile, TileProperty tileProperty)
        {
            this.tile = tile;
            this.tileProperty = tileProperty;
            name = tileProperty.info.Name.NicifyName();
        }

        public void SetValue(object value) => tileProperty.SetValue(tile, value);

        public object GetValue() => tileProperty.GetValue(tile);

        public T GetValue<T>() => tileProperty.GetValue<T>(tile);
    }
}
