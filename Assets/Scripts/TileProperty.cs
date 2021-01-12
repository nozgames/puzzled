using System;
using System.Linq;
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
        Sound
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
        public string name => info.Name;

        /// <summary>
        /// Display name of the property
        /// </summary>
        public string displayName {
            get {
                var displayName = name;
                if (port != null && displayName.EndsWith("Port"))
                    displayName = name.Substring(0, name.Length - 4);

                return displayName.NicifyName();
            }
        }

        /// <summary>
        /// Type of the property
        /// </summary>
        public TilePropertyType type { get; private set; }

        /// <summary>
        /// Construct a new tile property
        /// </summary>
        /// <param name="info"></param>
        /// <param name="editable"></param>
        public TileProperty(PropertyInfo info, EditableAttribute editable, PortAttribute port)
        {
            this.info = info;
            this.editable = editable;
            this.port = port;
            type = TilePropertyType.Unknown;

            // Dont bother setting the type if the property isnt ediable
            if (editable == null)
                return;

            if (info.PropertyType == typeof(int))
                type = TilePropertyType.Int;
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
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Return the component the property is a member of
        /// </summary>
        /// <param name="tile">Component parent tile</param>
        /// <returns>Component</returns>
        private TileComponent GetComponent(Tile tile) => (TileComponent)tile.GetComponentInChildren(info.DeclaringType);

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
    }
}
