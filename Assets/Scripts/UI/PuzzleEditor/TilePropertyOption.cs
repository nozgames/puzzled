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
            name = tileProperty.property.Name.NicifyName();
        }

        public void SetValue(string value) => tileProperty.SetValue(tile, value);
        public void SetValue(bool value) => tileProperty.SetValue(tile, value);
        public void SetValue(int value) => tileProperty.SetValue(tile, value);
        public void SetValue(Guid value) => tileProperty.SetValue(tile, value);

        public string GetValue() => tileProperty.GetValue(tile);
        public bool GetValueBool() => tileProperty.GetValueBool(tile);
        public int GetValueInt() => tileProperty.GetValueInt(tile);
        public Guid GetValueGuid() => tileProperty.GetValueGuid(tile);
    }
}
