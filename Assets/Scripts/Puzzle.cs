using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class Puzzle : MonoBehaviour
    {
        private const int FileVersion = 8;

        [Header("General")]
        [SerializeField] private TileGrid _tiles = null;
        [SerializeField] private Transform _wires = null;
        [SerializeField] private GridMesh _grid = null;
        [SerializeField] private Transform _trash = null;
        [SerializeField] private GameObject _wirePrefab = null;
        [SerializeField] private PuzzleProperties _propertiesPrefab = null;

        private Player _player;
        private bool _pendingDestroy;
        private string _path;
        private string _worldName;
        private bool _started;
        private PuzzleProperties _properties;
        private Dictionary<Type, object> _sharedComponentData = new Dictionary<Type, object>();

        [Flags]
        private enum SerializedTileFlags
        {
            HasName = 1 << 0,
            HasCellEdge = 1 << 1
        }

        /// <summary>
        /// Helper function for getting/setting the current puzzle
        /// </summary>
        public static Puzzle current {
            get => GameManager.puzzle;
            set => GameManager.puzzle = value;
        }

        public PuzzleProperties properties {
            get {
                if (_properties == null)
                    _properties = InstantiateTile(_propertiesPrefab.GetComponent<Tile>(), grid.minCell).GetComponent<PuzzleProperties>();

                return _properties;
            }
        }            

        /// <summary>
        /// Active player in this puzzle
        /// </summary>
        public Player player => _player;

        /// <summary>
        /// Cell the active player is in
        /// </summary>
        public Cell playerCell => player != null ? player.tile.cell : Cell.invalid;

        /// <summary>
        /// Returns true if this puzzle is the current puzzle
        /// </summary>
        public bool isCurrent => current == this;

        /// <summary>
        /// True if the puzzle is being edited
        /// </summary>
        public bool isEditing { get; set; }

        /// <summary>
        /// True if the puzzle is currently loading
        /// </summary>
        public bool isLoading { get; private set; }

        /// <summary>
        /// True if the puzzle is s
        /// </summary>
        public bool isStarting { get; private set; }

        /// <summary>
        /// True if the puzzle has been modified
        /// </summary>
        public bool isModified { get; set; }

        /// <summary>
        /// True if the grid lines should be visible for this puzzle
        /// </summary>
        public bool showGrid {
            get => _grid.gameObject.activeSelf;
            set => _grid.gameObject.SetActive(value);
        }

        /// <summary>
        /// Tile grid 
        /// </summary>
        public TileGrid grid => _tiles;

        /// <summary>
        /// Trash can
        /// </summary>
        public Transform trash => _trash;

        /// <summary>
        /// Puzzle file path
        /// </summary>
        public string path => _path;

        /// <summary>
        /// World the puzzle belongs to
        /// </summary>
        public string worldName => _worldName;

        /// <summary>
        /// Return the puzzle filename 
        /// </summary>
        public string filename => Path.GetFileNameWithoutExtension(_path);

        /// <summary>
        /// True if the puzzle has a valid path
        /// </summary>
        public bool hasPath => !string.IsNullOrEmpty(_path);

        /// <summary>
        /// Returns the active camera in the puzzle.  Note that this may be null if there are no cameras in the level
        /// </summary>
        public GameCamera activeCamera { get; private set; }

        private void Awake()
        {
            GameManager.onPuzzleChanged += OnPuzzleChanged;
        }

        private void OnPuzzleChanged(Puzzle puzzle)
        {
            if (puzzle != this)
                return;

            // Start the puzzle if it has not yet been started
            if(!_started)
            {
                GameCamera.Initialize(puzzle);

                if (!isEditing)
                {
                    // Center on starting camera if there is one
                    if (properties.startingCamera != null)
                        properties.startingCamera.ActivateCamera(0);
                }

                isStarting = true;

                // Send a start event to all tiles
                var start = new StartEvent();
                for (int i = 0; i < _tiles.transform.childCount; i++)
                    _tiles.transform.GetChild(i).GetComponent<Tile>().Send(start);

                isStarting = false;
                _started = true;
            }
        }

        /// <summary>
        /// Instantiate a new tile in the puzzle
        /// </summary>
        /// <param name="guid">Unique identifier of tile</param>
        /// <param name="cell">Cell to create tile in</param>
        /// <returns>Instantiated tile</returns>
        public Tile InstantiateTile(Guid guid, Cell cell) =>
            InstantiateTile(DatabaseManager.GetTile(guid), cell);

        /// <summary>
        /// Instantiate a new tile in the puzzle
        /// </summary>
        /// <param name="prefab">Tile prefab</param>
        /// <param name="cell">Cell to create tile in</param>
        /// <param name="info">Optional tile info override
        /// <returns>Instantiated tile</returns>
        public Tile InstantiateTile(Tile prefab, Cell cell, TileInfo info = null)
        {
            if (prefab == null)
                return null;

            // Do not allow two tiles to be intstantiated into the same cell
            if (_tiles.IsLinked(cell, prefab.layer))
            {
                Debug.LogError($"Cannot create tile `{prefab.info.displayName} at cell {cell}, layer is occupied by `{grid.CellToTile(cell, prefab.layer).info.displayName}");
                return null;
            }

            var tile = Instantiate(prefab.gameObject, _tiles.transform).GetComponent<Tile>();
            tile.info = info != null ? info : tile.info;
            tile.puzzle = this;
            tile.guid = prefab.guid;
            tile.name = prefab.name;
            tile.gameObject.SetChildLayers(CameraManager.TileLayerToObjectLayer(tile.layer));

            // Send awake event before the tile is linked into the grid
            tile.Send(new AwakeEvent());

            // Link the tile into the grid and normalize the cell
            tile.cell = cell;

            // If the tile failed to link to the given cell then just destroy it
            if(tile.cell == Cell.invalid && cell != Cell.invalid)
            {
                tile.Destroy();
                return null;
            }

            // Keep track of the player
            var playerComponent = tile.GetComponent<Player>();
            if (null != playerComponent)
                _player = playerComponent;

            return tile;
        }

        /// <summary>
        /// Instantiate a new wire in the puzzle
        /// </summary>
        /// <param name="from">Port from</param>
        /// <param name="to">Port to</param>
        /// <returns>Instantiated wire or null if the wire could not be created</returns>
        public Wire InstantiateWire(Port from, Port to)
        {
            // Ensure the wire has a valid port on both ends
            if (from == null || to == null)
                return null;

            // Ensure we arent linking to ourself
            if (from == to) //  || from.tile == to.tile)
                return null;


            if (from.flow != PortFlow.Output || to.flow != PortFlow.Input)
                return null;

            switch (from.type)
            {
                case PortType.Number:
                    // Can only connect number to number
                    if (to.type != PortType.Number)
                        return null;
                    break;

                case PortType.Power:
                    // Power ports cannot connect to number ports
                    if (to.type == PortType.Number)
                        return null;
                    break;

                case PortType.Signal:
                    // Signal can only connect to signal
                    if (to.type != PortType.Signal)
                        return null;
                    break;
            }

            // Already connected?
            if (to.IsConnectedTo(from))
                return null;

            var wire = Instantiate(_wirePrefab, _wires).GetComponent<Wire>();
            wire.from.port = from;
            wire.to.port = to;
            from.wires.Add(wire);
            to.wires.Add(wire);
            wire.transform.position = _tiles.CellToWorld(wire.from.tile.cell);
            wire.puzzle = this;
            return wire;
        }

        /// <summary>
        /// Save the puzzle using its existing path
        /// </summary>
        public void Save() => Save(_path);

        /// <summary>
        /// Save the puzzle using a new path
        /// </summary>
        /// <param name="path">Path to save file with</param>
        public void Save(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            _path = path;
            _worldName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(_path));

            isModified = false;

            // Save the file using the given path
            using (var file = File.Create(_path))
            using (var writer = new BinaryWriter(file))
                Save(_tiles.GetTiles(), writer);
        }

        /// <summary>
        /// Save the puzzle using a new path
        /// </summary>
        /// <param name="file">file stream to save file with</param>
        /// <param name="path">Path to save file with</param>
        public void Save(Stream file, string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            _path = path;
            _worldName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(_path));

            isModified = false;

            // Save the file using the given path
            using (var writer = new BinaryWriter(file))
                Save(_tiles.GetTiles(), writer);
        }

        /// <summary>
        /// Save the given tile list
        /// </summary>
        /// <param name="tiles">Tiles to save</param>
        /// <param name="writer">Write to save to</param>
        private void Save(Tile[] tiles, BinaryWriter writer)
        {
            // Write the fourcc 
            writer.WriteFourCC('P', 'U', 'Z', 'L');

            // Write the current version
            writer.Write(FileVersion);

            // Write the tiles
            writer.Write(tiles.Length);

            // Collect all the wires to write
            var wires = new List<Wire>();
            foreach (var tile in tiles)
                foreach (var property in tile.properties)
                    if (property.type == TilePropertyType.Port && property.port.flow == PortFlow.Output)
                        wires.AddRange(property.GetValue<Port>(tile).wires);

            // Write the wire count
            writer.Write(wires.Count);

            // Write all of the tile guids and cell positions
            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var tile = tiles[tileIndex];
                writer.Write(tile.guid);
                writer.Write(tile.cell);

                var flags = (SerializedTileFlags)0;
                if (tile.name != DatabaseManager.GetTile(tile.guid).name)
                    flags |= SerializedTileFlags.HasName;

                writer.Write((byte)flags);

                // Optionally write the tile name
                if ((flags & SerializedTileFlags.HasName) == SerializedTileFlags.HasName)
                    writer.Write(tile.name);
            }

            // Write tile properties
            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var tile = tiles[tileIndex];

                if (tile.properties == null || tile.properties.Length == 0)
                    continue;

                writer.Write(tileIndex + 1);

                // Write placeholder for tile size
                var sizePosition = writer.BaseStream.Position;
                writer.Write(0);

                // Write the tile properties
                foreach (var property in tile.properties)
                {
                    if (!property.editable.serialized)
                        continue;

                    var value = property.GetValue(tile);
                    if (value == null)
                        continue;

                    writer.Write((byte)property.type);
                    writer.Write(property.name);

                    switch (property.type)
                    {
                        case TilePropertyType.TileComponent:
                        {
                            var tileComponent = (TileComponent)value;
                            if (tileComponent == null)
                                writer.Write(0);
                            else
                            {
                                var tileRef = tileComponent.tile;
                                int tileRefIndex = 0;
                                for (; tileRefIndex < tiles.Length; tileRefIndex++)
                                    if (tiles[tileRefIndex] == tileRef)
                                        break;

                                writer.Write(tileRefIndex < tiles.Length ? tileRefIndex + 1 : 0);
                            }
                            break;
                        }

                        case TilePropertyType.Cell:
                            writer.Write(((Cell)value).x);
                            writer.Write(((Cell)value).y);
                            break;

                        case TilePropertyType.Int:
                            writer.Write((int)value);
                            break;

                        case TilePropertyType.Sound:
                            writer.Write(((Sound)value).guid);
                            break;

                        case TilePropertyType.IntArray:
                        {
                            var intArray = (int[])value;
                            writer.Write(intArray.Length);
                            foreach (var intValue in intArray)
                                writer.Write(intValue);
                            break;
                        }

                        case TilePropertyType.Bool:
                            writer.Write((bool)value);
                            break;

                        case TilePropertyType.String:
                            writer.Write((string)value);
                            break;

                        case TilePropertyType.Guid:
                            writer.Write((Guid)value);
                            break;

                        case TilePropertyType.Background:
                            writer.Write(((Background)value)?.guid ?? Guid.Empty);
                            break;

                        case TilePropertyType.Decal:
                            writer.Write((Decal)value);
                            break;

                        case TilePropertyType.DecalArray:
                        {
                            var darray = (Decal[])value;
                            writer.Write(darray.Length);
                            foreach (var decal in darray)
                                writer.Write(decal);
                            break;
                        }

                        case TilePropertyType.StringArray:
                        {
                            var sarray = (string[])value;
                            writer.Write(sarray.Length);
                            foreach (var s in sarray)
                                writer.Write(s);
                            break;
                        }

                        case TilePropertyType.Tile:
                            writer.Write(((Tile)value).guid);
                            break;

                        case TilePropertyType.Port:
                        {
                            var portWires = ((Port)value).wires;
                            writer.Write(portWires.Count);
                            foreach(var wire in portWires)
                                writer.Write(wires.IndexOf(wire));
                            break;
                        }

                        default:
                            throw new NotImplementedException();
                    }
                }

                // Unknown type means end of properties
                writer.Write((byte)TilePropertyType.Unknown);

                // Write the tile size
                var returnPosition = writer.BaseStream.Position;
                writer.BaseStream.Seek(sizePosition, SeekOrigin.Begin);
                writer.Write((int)(returnPosition - sizePosition - sizeof(int)));
                writer.BaseStream.Seek(returnPosition, SeekOrigin.Begin);
            }

            writer.Write(0);

            // Write all the wire options
            foreach(var wire in wires)
            {
                writer.Write((byte)(wire.from.options?.Length ?? 0));
                if (wire.from.hasOptions)
                    foreach (var option in wire.from.options)
                        writer.Write(option);

                writer.Write((byte)(wire.to.options?.Length ?? 0));
                if (wire.to.hasOptions)
                    foreach (var option in wire.to.options)
                        writer.Write(option);
            }
        }

        /// <summary>
        /// Load a puzzle from the given path
        /// </summary>
        /// <param name="path">Puzzle path</param>
        /// <returns>Loaded puzzle or null if the puzzle failed to load</returns>
        public static Puzzle Load(string path)
        {
            var puzzle = GameManager.InstantiatePuzzle();
            puzzle.isLoading = true;
            try
            {
                using (var file = File.OpenRead(path))
                using (var reader = new BinaryReader(file))
                    puzzle.Load(reader);

                puzzle._path = path;
                puzzle._worldName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(puzzle._path));
                puzzle.isLoading = false;

                Debug.Log($"Puzzled Loaded: [{puzzle._tiles.transform.childCount} tiles, {puzzle._wires.transform.childCount} wires]");
            } 
            catch (Exception e)
            {
                Debug.LogException(e);
                puzzle.Destroy();
                return null;
            }

            return puzzle;
        }

        /// <summary>
        /// Load a puzzle from the file stream
        /// </summary>
        /// <param name="file">Puzzle file stream</param>
        /// <param name="path">Puzzle path</param>
        /// <returns>Loaded puzzle or null if the puzzle failed to load</returns>
        public static Puzzle Load(Stream file, string path)
        {
            var puzzle = GameManager.InstantiatePuzzle();
            puzzle.isLoading = true;
            try
            {
                using (var reader = new BinaryReader(file))
                    puzzle.Load(reader);

                puzzle._path = path;
                puzzle._worldName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(puzzle._path));
                puzzle.isLoading = false;

                Debug.Log($"Puzzled Loaded: [{puzzle._tiles.transform.childCount} tiles, {puzzle._wires.transform.childCount} wires]");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                puzzle.Destroy();
                return null;
            }

            return puzzle;
        }

        /// <summary>
        /// Load binary puzzle file
        /// </summary>
        /// <param name="reader">Reader to read from</param>
        private void Load(BinaryReader reader)
        {
            // Write the fourcc 
            if (!reader.ReadFourCC('P', 'U', 'Z', 'L'))
                // TODO: read from json
                throw new InvalidDataException();

            // Write the current version
            var version = reader.ReadInt32();

            switch (version)
            {
                case 3:
                case 4:
                    LoadV3(reader, version);
                    break;

                case 5:
                case 6:
                case 7:
                case 8:
                    LoadV5(reader, version);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void LoadV3 (BinaryReader reader, int version)
        {
            // Create the tile array
            var tiles = new Tile[reader.ReadInt32()];

            // Instantiate all the wires
            var wires = new Wire[reader.ReadInt32()];
            for (int wireIndex = 0; wireIndex < wires.Length; wireIndex++)
            {
                wires[wireIndex] = Instantiate(_wirePrefab, _wires).GetComponent<Wire>();
                wires[wireIndex].puzzle = this;
            }

            // Instantiate the tiles
            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var guid = reader.ReadGuid();
                var size = reader.ReadInt32();
                var start = reader.BaseStream.Position;

                // Find the tile prefab and if it doesnt exist skip the tile
                var prefab = DatabaseManager.GetTile(guid);
                if (null == prefab)
                {
                    reader.BaseStream.Position = start + size;
                    continue;
                }

                // Create the tile and if it fails skip the tile
                var cell = reader.ReadCell(version);
                var tile = InstantiateTile(prefab, cell);
                if (tile == null)
                {
                    reader.BaseStream.Position = start + size;
                    continue;
                }

                if (version == 4 && reader.ReadBoolean())
                    tile.name = reader.ReadString();

                tiles[tileIndex] = tile;

                // Fish for the puzzle properties
                var puzzleProperties = tile.GetComponent<PuzzleProperties>();
                if (null != puzzleProperties)
                    _properties = puzzleProperties;

                // Read the tile properties
                while (true)
                {
                    // Last property should be an Unknown
                    var type = (TilePropertyType)reader.ReadByte();
                    if (type == TilePropertyType.Unknown)
                        break;

                    // Read the property value
                    var name = reader.ReadString();
                    var value = (object)null;

                    switch (type)
                    {
                        case TilePropertyType.Int:
                            value = reader.ReadInt32();
                            break;

                        case TilePropertyType.Cell:
                            value = new Cell(reader.ReadInt32(), reader.ReadInt32());
                            break;

                        case TilePropertyType.Sound:
                        {
                            var sound = DatabaseManager.GetSound(reader.ReadGuid());
                            value = sound;
                            break;
                        }

                        case TilePropertyType.IntArray:
                        {
                            var intArray = new int[reader.ReadInt32()];
                            for (int i = 0; i < intArray.Length; i++)
                                intArray[i] = reader.ReadInt32();
                            value = intArray;
                            break;
                        }

                        case TilePropertyType.Bool:
                            value = reader.ReadBoolean();
                            break;

                        case TilePropertyType.String:
                            value = reader.ReadString();
                            break;

                        case TilePropertyType.StringArray:
                        {
                            var sarray = new string[reader.ReadInt32()];
                            for (int i = 0; i < sarray.Length; i++)
                                sarray[i] = reader.ReadString();
                            value = sarray;
                            break;
                        }

                        case TilePropertyType.Guid:
                            value = reader.ReadGuid();
                            break;

                        case TilePropertyType.Background:
                        {
                            var background = DatabaseManager.GetBackground(reader.ReadGuid());
                            value = background;
                            break;
                        }

                        case TilePropertyType.Decal:
                        {
                            var decal = DatabaseManager.GetDecal(reader.ReadGuid());
                            if (version > 1)
                                decal.flags = (DecalFlags)reader.ReadInt32();

                            value = decal;
                            break;
                        }

                        case TilePropertyType.DecalArray:
                        {
                            var decals = new Decal[reader.ReadInt32()];
                            for (int i = 0; i < decals.Length; i++)
                            {
                                var decal = DatabaseManager.GetDecal(reader.ReadGuid());
                                var flags = (DecalFlags)reader.ReadInt32();

                                decal.flags = flags;
                                decals[i] = decal;
                            }

                            value = decals;
                            break;
                        }

                        case TilePropertyType.Tile:
                            value = DatabaseManager.GetTile(reader.ReadGuid());
                            break;

                        case TilePropertyType.Port:
                        {
                            var port = tile.GetPropertyValue<Port>(name);
                            var portWireCount = reader.ReadInt32();
                            if (null == port)
                            {
                                for (int i = 0; i < portWireCount; i++)
                                    reader.ReadInt32();
                                continue;
                            }

                            var portWires = new List<Wire>(portWireCount);
                            for (int i = 0; i < portWireCount; i++)
                            {
                                var wireIndex = reader.ReadInt32();
                                var wire = wires[wireIndex];
                                if (port.flow == PortFlow.Input)
                                    wire.to.port = port;
                                else
                                    wire.from.port = port;

                                portWires.Add(wire);
                            }
                            port.wires.AddRange(portWires);
                            continue;
                        }

                        default:
                            throw new NotImplementedException();
                    }

                    tile.SetPropertyValue(name, value);
                }
            }

            foreach(var wire in wires)
            {
                var fromOptionCount = (int)reader.ReadByte();
                var fromOptions = fromOptionCount == 0 ? null : new int[fromOptionCount];
                if (fromOptions != null)
                    for (int i = 0; i < fromOptionCount; i++)
                        fromOptions[i] = reader.ReadInt32();

                var toOptionCount = (int)reader.ReadByte();
                var toOptions = toOptionCount == 0 ? null : new int[toOptionCount];
                if (toOptions != null)
                    for (int i = 0; i < toOptionCount; i++)
                        toOptions[i] = reader.ReadInt32();

                // If the wire isnt valid then just remove it
                if (wire.from.port == null || wire.to.port == null)
                {
                    wire.Destroy();
                    continue;
                }

                wire.from.SetOptions(fromOptions);
                wire.to.SetOptions(toOptions);
                wire.UpdatePositions();
            }
        }

        private void LoadV5(BinaryReader reader, int version)
        {
            // Create the tile array
            var tiles = new Tile[reader.ReadInt32()];

            // Instantiate all the wires
            var wires = new Wire[reader.ReadInt32()];
            for (int wireIndex = 0; wireIndex < wires.Length; wireIndex++)
            {
                wires[wireIndex] = Instantiate(_wirePrefab, _wires).GetComponent<Wire>();
                wires[wireIndex].puzzle = this;
            }

            // Read all tiles and instantiate them
            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var guid = reader.ReadGuid();
                var cell = reader.ReadCell(version);
                var name = (string)null;

                if(version == 5)
                {
                    name = reader.ReadBoolean() ? reader.ReadString() : null;
                }
                else
                {
                    var flags = (SerializedTileFlags)reader.ReadByte();
                    if ((flags & SerializedTileFlags.HasName) == SerializedTileFlags.HasName)
                        name = reader.ReadString();

                    if (version < 7 && (flags & SerializedTileFlags.HasCellEdge) == SerializedTileFlags.HasCellEdge)
                        cell = new Cell(CellCoordinateSystem.SharedEdge, cell.x, cell.y, (CellEdge)reader.ReadByte());
                }

                var prefab = DatabaseManager.GetTile(guid);
                if (null == prefab)
                    continue;

                tiles[tileIndex] = InstantiateTile(prefab, cell);

                if (null == tiles[tileIndex])
                    continue;

                if (name != null)
                    tiles[tileIndex].name = name;

                // Fish for the puzzle properties
                var puzzleProperties = tiles[tileIndex].GetComponent<PuzzleProperties>();
                if (null != puzzleProperties)
                    _properties = puzzleProperties;
            }

            // Instantiate the tiles
            while (true)
            {
                var tileIndex = reader.ReadInt32();
                if (tileIndex == 0)
                    break;

                tileIndex--;

                var tile = tiles[tileIndex];

                // If the tile failed to load then just skip its properties
                var size = reader.ReadInt32();
                if (tiles[tileIndex] == null)
                {
                    reader.BaseStream.Position += size;
                    continue;
                }

                // Read the tile properties
                while (true)
                {
                    // Last property should be an Unknown
                    var type = (TilePropertyType)reader.ReadByte();
                    if (type == TilePropertyType.Unknown)
                        break;

                    // Read the property value
                    var name = reader.ReadString();
                    var value = (object)null;

                    switch (type)
                    {
                        case TilePropertyType.Int:
                            value = reader.ReadInt32();
                            break;

                        case TilePropertyType.Cell:
                            value = new Cell(reader.ReadInt32(), reader.ReadInt32());
                            break;

                        case TilePropertyType.TileComponent:
                        {
                            var tileRefIndex = reader.ReadInt32();
                            if (tileRefIndex > 0)
                            { 
                                var tileRef = tiles[tileRefIndex - 1];
                                if (tileRef != null)
                                    value = tileRef.GetComponent(tile.GetProperty(name).info.PropertyType);
                            }
                            break;
                        }

                        case TilePropertyType.Sound:
                        {
                            var sound = DatabaseManager.GetSound(reader.ReadGuid());
                            value = sound;
                            break;
                        }

                        case TilePropertyType.IntArray:
                        {
                            var intArray = new int[reader.ReadInt32()];
                            for (int i = 0; i < intArray.Length; i++)
                                intArray[i] = reader.ReadInt32();
                            value = intArray;
                            break;
                        }

                        case TilePropertyType.Bool:
                            value = reader.ReadBoolean();
                            break;

                        case TilePropertyType.String:
                            value = reader.ReadString();
                            break;

                        case TilePropertyType.StringArray:
                        {
                            var sarray = new string[reader.ReadInt32()];
                            for (int i = 0; i < sarray.Length; i++)
                                sarray[i] = reader.ReadString();
                            value = sarray;
                            break;
                        }

                        case TilePropertyType.Guid:
                            value = reader.ReadGuid();
                            break;

                        case TilePropertyType.Background:
                        {
                            var background = DatabaseManager.GetBackground(reader.ReadGuid());
                            value = background;
                            break;
                        }

                        case TilePropertyType.Decal:
                            value = reader.ReadDecal(version);
                            break;

                        case TilePropertyType.DecalArray:
                        {
                            var decals = new Decal[reader.ReadInt32()];
                            for (int i = 0; i < decals.Length; i++)
                                decals[i] = reader.ReadDecal(version);

                            value = decals;
                            break;
                        }

                        case TilePropertyType.Tile:
                            value = DatabaseManager.GetTile(reader.ReadGuid());
                            break;

                        case TilePropertyType.Port:
                        {
                            var port = tile.GetPropertyValue<Port>(name);
                            var portWireCount = reader.ReadInt32();
                            if (null == port)
                            {
                                for (int i = 0; i < portWireCount; i++)
                                    reader.ReadInt32();
                                continue;
                            }

                            var portWires = new List<Wire>(portWireCount);
                            for (int i = 0; i < portWireCount; i++)
                            {
                                var wireIndex = reader.ReadInt32();
                                var wire = wires[wireIndex];
                                if (port.flow == PortFlow.Input)
                                    wire.to.port = port;
                                else
                                    wire.from.port = port;

                                portWires.Add(wire);
                            }
                            port.wires.AddRange(portWires);
                            continue;
                        }

                        default:
                            throw new NotImplementedException();
                    }

                    tile.SetPropertyValue(name, value);
                }
            }

            foreach (var wire in wires)
            {
                var fromOptionCount = (int)reader.ReadByte();
                var fromOptions = fromOptionCount == 0 ? null : new int[fromOptionCount];
                if (fromOptions != null)
                    for (int i = 0; i < fromOptionCount; i++)
                        fromOptions[i] = reader.ReadInt32();

                var toOptionCount = (int)reader.ReadByte();
                var toOptions = toOptionCount == 0 ? null : new int[toOptionCount];
                if (toOptions != null)
                    for (int i = 0; i < toOptionCount; i++)
                        toOptions[i] = reader.ReadInt32();

                // If the wire isnt valid then just remove it
                if (wire.from.port == null || wire.to.port == null)
                {
                    wire.Destroy();
                    continue;
                }

                wire.from.SetOptions(fromOptions);
                wire.to.SetOptions(toOptions);
                wire.UpdatePositions();
            }
        }

        /// <summary>
        /// Hide all wires
        /// </summary>
        public void HideWires() => ShowWires(false);

        /// <summary>
        /// Show or hide all wires
        /// </summary>
        /// <param name="show">True to show wires and flase to hide wires</param>
        public void ShowWires(bool show=true)
        {
            var wires = _wires;
            for (int i = 0; i < wires.transform.childCount; i++)
                wires.transform.GetChild(i).GetComponent<Wire>().visible = show;
        }

        /// <summary>
        /// Show all wires for a given tile
        /// </summary>
        /// <param name="tile">Tile to show wires for</param>
        /// <param name="show">True to show wires and flase to hide wires</param>
        public void ShowWires(Tile tile, bool show = true)
        {
            foreach (var property in tile.properties)
                if (property.type == TilePropertyType.Port)
                    foreach (var wire in property.GetValue<Port>(tile).wires)
                        wire.visible = show;
        }

        /// <summary>
        /// Destroy the puzzle 
        /// </summary>
        public void Destroy()
        {
            // Destroy all tiles
            for (int i = _tiles.transform.childCount - 1; i >= 0; i--)
                _tiles.transform.GetChild(i).GetComponent<Tile>().Destroy();

            // Destroy all tiles in the trash 
            for (int i = _trash.transform.childCount - 1; i >= 0; i--)
            {
                var tile = _trash.transform.GetChild(i).GetComponent<Tile>();
                if (null != tile)
                    tile.Destroy();
            }

            _pendingDestroy = true;

            // Clear the current puzzle if it is us
            if (current == this)
                current = null;

            // Destroy ourself
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!_pendingDestroy)
                Debug.LogWarning("Puzzle was not destroyed using the Puzzle.Destroy method");

            GameManager.onPuzzleChanged -= OnPuzzleChanged;
        }

        public void SetSharedComponentData (TileComponent component, object data)
        {
            if (component == null)
                return;

            _sharedComponentData[component.GetType()] = data;
        }

        public void SetSharedComponentData(Type type, object data)
        {
            if (type == null)
                return;

            _sharedComponentData[type] = data;
        }

        public T GetSharedComponentData<T>(Type componentType) where T : class
        {
            if (componentType == null)
                return null;
            return _sharedComponentData.TryGetValue(componentType, out var value) ? (value as T) : null;
        }

        public T GetSharedComponentData<T> (TileComponent component) where T : class 
        {
            if (component == null)
                return null;
            return GetSharedComponentData<T>(component.GetType());
        }

        /// <summary>
        /// Ray cast through tiles in a cardinal direction and stop at the first tile hit
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public RayCastEvent RayCast (Cell from, Vector2Int direction, int distance)
        {
            var cell = from;
            var raycast = new RayCastEvent(direction);
            var edge = Cell.DirectionToEdge(direction);

            // TODO: diagonal

            for(int i=0; i<distance; i++)
            {
                // Test the edge between the current cell and the next cell first
                grid.SendToCell(raycast, new Cell(cell, edge), CellEventRouting.All);
                if (raycast.hit != null)
                    break;

                // Now test the next cell in the direction
                cell += direction;
                grid.SendToCell(raycast, cell, CellEventRouting.All);
                if (raycast.hit != null)
                    break;
            }

            return raycast;
        }        
    }
}
