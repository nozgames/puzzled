namespace Puzzled
{
    /// <summary>
    /// List of all known tiles.  
    /// 
    /// Note that changing the name of a tile will result in old save files not 
    /// working unless the [TileAlias] attribute is used to reference the old name.
    /// </summary>
    public enum TileType
    {
        Player,
        Exit,
        PressurePlate,
        Switch,
        Door,
        Pushable,
        Floor
    }
}
