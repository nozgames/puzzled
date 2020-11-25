using System;

namespace Puzzled
{
    public enum TileLayer
    {
        Static = 10,
        Dynamic = 11,
        Logic = 12
    }

    [AttributeUsage(AttributeTargets.Field)]
    class TileLayerAttribute : Attribute
    {
        public TileLayer layer = TileLayer.Static;

        public TileLayerAttribute(TileLayer layer)
        {
            this.layer = layer;
        }
    }
}
