using System;
using System.Reflection;

namespace Puzzled
{
    /// <summary>
    /// Supported tile property types
    /// 
    /// NOTE: this array is serialized, do not remove or insert any values or serialization will break
    /// </summary>
    public enum TilePropertyType
    {
        Unknown,
        Int,
        Bool,
        String,
        Guid,
        StringArray,
        Decal,
        DecalArray,
        Tile,
        Background,
        IntArray,
        Port,
        Sound,
        Cell,

        /// <summary>
        /// Reference to a tile component within the puzzle
        /// </summary>
        TileComponent,
        SoundArray,
    }

    public class TileProperty
    {
        /// <summary>
        /// Property info for the property 
        /// </summary>
        public PropertyInfo info { get; private set; }

        /// <summary>
        /// Editable attribute that was attached to the property
        /// </summary>
        public EditableAttribute editable { get; private set; }

        /// <summary>
        /// Optional port attribute
        /// </summary>
        public PortAttribute port { get; private set; }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string name { get; private set; }

        public ulong componentInstanceId { get; private set; }

        /// <summary>
        /// Display name of the property
        /// </summary>
        public string displayName { get; private set; }

        /// <summary>
        /// Type of the property
        /// </summary>
        public TilePropertyType type { get; private set; }

        /// <summary>
        /// Construct a new tile property
        /// </summary>
        /// <param name="info"></param>
        /// <param name="editable"></param>
        public TileProperty(TileComponent tileComponent, PropertyInfo info, EditableAttribute editable, PortAttribute port)
        {
            this.componentInstanceId = tileComponent.instanceId;
            this.info = info;
            this.editable = editable;
            this.port = port;
            type = TilePropertyType.Unknown;

            // Dont bother setting the type if the property isnt ediable
            if (editable == null)
                return;

            // Dynamic name
            if (!string.IsNullOrEmpty(editable.dynamicName))
            {
                var propertyInfo = tileComponent.GetType().GetProperty(editable.dynamicName);
                if (null != propertyInfo && propertyInfo.PropertyType == typeof(string))
                    name = (string)propertyInfo.GetValue(tileComponent);
            }

            if (string.IsNullOrEmpty(name))
                name = info.Name;

            // Build the display name
            if (!string.IsNullOrEmpty(editable.dynamicDisplayName))
            {
                var propertyInfo = tileComponent.GetType().GetProperty(editable.dynamicDisplayName);
                if (null != propertyInfo && propertyInfo.PropertyType == typeof(string))
                    displayName = (string)propertyInfo.GetValue(tileComponent);
            }

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = name;
                if (port != null && displayName.EndsWith("Port"))
                    displayName = displayName.Substring(0, displayName.Length - 4);
                displayName = displayName.NicifyName();
            }

            if (info.PropertyType == typeof(int))
                type = TilePropertyType.Int;
            else if (info.PropertyType == typeof(Cell))
                type = TilePropertyType.Cell;
            else if (info.PropertyType == typeof(int[]))
                type = TilePropertyType.IntArray;
            else if (info.PropertyType == typeof(int[]))
                type = TilePropertyType.Sound;
            else if (info.PropertyType == typeof(bool))
                type = TilePropertyType.Bool;
            else if (info.PropertyType == typeof(string))
                type = TilePropertyType.String;
            else if (info.PropertyType == typeof(string[]))
                type = TilePropertyType.StringArray;
            else if (info.PropertyType == typeof(Guid))
                type = TilePropertyType.Guid;
            else if (info.PropertyType == typeof(Decal))
                type = TilePropertyType.Decal;
            else if (info.PropertyType == typeof(Decal[]))
                type = TilePropertyType.DecalArray;
            else if (info.PropertyType == typeof(Tile))
                type = TilePropertyType.Tile;
            else if (info.PropertyType == typeof(Background))
                type = TilePropertyType.Background;
            else if (info.PropertyType == typeof(Port))
                type = TilePropertyType.Port;
            else if (info.PropertyType == typeof(Sound))
                type = TilePropertyType.Sound;
            else if (info.PropertyType == typeof(Sound[]))
                type = TilePropertyType.SoundArray;
            else if (typeof(TileComponent).IsAssignableFrom(info.PropertyType))
                type = TilePropertyType.TileComponent;
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Return the component the property is a member of
        /// </summary>
        /// <param name="tile">Component parent tile</param>
        /// <returns>Component</returns>
        public TileComponent GetComponent(Tile tile) => tile.GetTileComponent(componentInstanceId);

        /// <summary>
        /// Set the tile property value
        /// </summary>
        /// <param name="tile">Tile to set the property on</param>
        /// <param name="value">Property value</param>
        public void SetValue (Tile tile, object value) => info.SetValue(GetComponent(tile), value);

        /// <summary>
        /// Get a tile property value
        /// </summary>
        /// <param name="tile">Tile to get property from</param>
        /// <returns>Value of property</returns>
        public object GetValue (Tile tile) => info.GetValue(GetComponent(tile));

        /// <summary>
        /// Get property value and cast it to the given type
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="tile">Tile to get property from</param>
        /// <returns>Value of property</returns>
        public T GetValue<T>(Tile tile) => (T)GetValue(tile);

        /// <summary>
        /// Return the property as a boolean value 
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public bool GetValueAsBool (Tile tile)
        {
            if (tile == null)
                return false;

            switch (type)
            {
                case TilePropertyType.Bool: return GetValue<bool>(tile);
                case TilePropertyType.Int:
                    return GetValue<int>(tile) == 0 ? false : true;
                case TilePropertyType.Decal:
                    return GetValue<Decal>(tile).texture != null;
            }

            return false;
        }
    }
}
