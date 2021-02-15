using System;

namespace Puzzled
{
    public enum TileLayer
    {
        Floor,

        /// <summary>
        /// Wall tile
        /// </summary>
        Wall,

        /// <summary>
        /// Static on a wall
        /// </summary>
        WallStatic,

        Static,
        Dynamic,
        Logic
    }
}
