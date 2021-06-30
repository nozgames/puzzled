using System;

namespace Puzzled
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CoordinateSystemAttribute : Attribute
    {
        public CellCoordinateSystem system { get; private set; }

        public CoordinateSystemAttribute(CellCoordinateSystem coordinateType)
        {
            this.system = coordinateType;
        }
    }

    public enum TileLayer
    {
        Floor,

        /// <summary>
        /// Wall tile
        /// </summary>
        [CoordinateSystem(CellCoordinateSystem.SharedEdge)]
        Wall,

        /// <summary>
        /// Static on a wall
        /// </summary>
        [CoordinateSystem(CellCoordinateSystem.Edge)]
        WallStatic,

        Static,

        Dynamic,

        Logic,

        InvisibleStatic
    }    
}
