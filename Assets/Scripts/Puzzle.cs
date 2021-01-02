﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class Puzzle : MonoBehaviour
    {
        private const int FileVersion = 3;

        [Header("General")]
        [SerializeField] private TileGrid _tiles = null;
        [SerializeField] private Transform _wires = null;
        [SerializeField] private GridMesh _grid = null;
        [SerializeField] private Transform _trash = null;
        [SerializeField] private GameObject _wirePrefab = null;

        private Player _player;
        private bool _pendingDestroy;
        private string _path;
        private bool _started;

        /// <summary>
        /// Saved camera state
        /// </summary>
        private CameraState _savedCameraState;

        /// <summary>
        /// Helper function for getting/setting the current puzzle
        /// </summary>
        public static Puzzle current {
            get => GameManager.puzzle;
            set => GameManager.puzzle = value;
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
        /// Return the puzzle filename 
        /// </summary>
        public string filename => Path.GetFileNameWithoutExtension(_path);

        /// <summary>
        /// True if the puzzle has a valid path
        /// </summary>
        public bool hasPath => !string.IsNullOrEmpty(_path);

        /// <summary>
        /// True if the puzzle has been modified since the last time it was saved
        /// </summary>
        public bool isModified { get; set; }

        private void Awake()
        {
            GameManager.onPuzzleChanged += OnPuzzleChanged;
        }

        private void OnPuzzleChanged(Puzzle puzzle)
        {
            if (puzzle != this)
                return;

            if(!_started)
            {
                // Find the player
                _player = _tiles.GetComponentInChildren<Player>();
                if (_player != null)
                {
                    // Default the camera to the player, if there is an initial camera it will be set when the start event is sent out
                    CameraManager.JumpToCell(_player.tile.cell, CameraManager.DefaultZoomLevel);
                }

                // Send a start event to all tiles
                var start = new StartEvent();
                for (int i = 0; i < _tiles.transform.childCount; i++)
                    _tiles.transform.GetChild(i).GetComponent<Tile>().Send(start);

                _started = true;
            }
        }

        private void OnEnable()
        {
            if (_savedCameraState.orthographicSize > 0)
                CameraManager.state = _savedCameraState;
        }

        private void OnDisable()
        {
            _savedCameraState = CameraManager.state;
        }

        /// <summary>
        /// Instantiate a new tile in the puzzle
        /// </summary>
        /// <param name="guid">Unique identifier of tile</param>
        /// <param name="cell">Cell to create tile in</param>
        /// <returns>Instantiated tile</returns>
        public Tile InstantiateTile(Guid guid, Cell cell) =>
            InstantiateTile(TileDatabase.GetTile(guid), cell);

        /// <summary>
        /// Instantiate a new tile in the puzzle
        /// </summary>
        /// <param name="prefab">Tile prefab</param>
        /// <param name="cell">Cell to create tile in</param>
        /// <returns>Instantiated tile</returns>
        public Tile InstantiateTile(Tile prefab, Cell cell)
        {
            if (prefab == null)
                return null;

            // Do not allow two tiles to be intstantiated into the same cell
            if (_tiles.IsLinked(cell, prefab.info.layer))
            {
                Debug.LogError($"Cannot create tile `{prefab.info.displayName} at cell {cell}, layer is occupied by `{grid.CellToTile(cell, prefab.info.layer).info.displayName}");
                return null;
            }

            var tile = Instantiate(prefab.gameObject, _tiles.transform).GetComponent<Tile>();
            tile.puzzle = this;
            tile.guid = prefab.guid;
            tile.gameObject.SetChildLayers(CameraManager.TileLayerToObjectLayer(tile.info.layer));
            tile.Send(new AwakeEvent());
            tile.cell = cell;
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
            if (from == to || from.tile == to.tile)
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

            isModified = false;

            // Save the file using the given path
            using (var file = File.Create(_path))
            using (var writer = new BinaryWriter(file))
                Save(_tiles.GetLinkedTiles(), writer);
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

            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var tile = tiles[tileIndex];

                writer.Write(tile.guid);

                // Write placeholder for tile size
                var sizePosition = writer.BaseStream.Position;
                writer.Write(0);

                writer.Write(tile.cell);

                // Write the tile properties
                if (tile.properties != null)
                    foreach (var property in tile.properties)
                    {
                        var value = property.GetValue(tile);
                        if (value == null)
                            continue;

                        writer.Write((byte)property.type);
                        writer.Write(property.name);

                        switch (property.type)
                        {
                            case TilePropertyType.Int:
                                writer.Write((int)value);
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
                                writer.Write(((Decal)value).guid);
                                writer.Write((int)((Decal)value).flags);
                                break;

                            case TilePropertyType.DecalArray:
                            {
                                var darray = (Decal[])value;
                                writer.Write(darray.Length);
                                foreach (var decal in darray)
                                {
                                    writer.Write(decal.guid);
                                    writer.Write((int)decal.flags);
                                }
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
                                {
                                    var wireIndex = wires.IndexOf(wire);
                                    writer.Write(wireIndex);
                                }
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
                try
                {
                    using (var file = File.OpenRead(path))
                    using (var reader = new BinaryReader(file))
                        puzzle.Load(reader);
                } 
                catch (Exception e)
                {
                    Debug.LogException(e);
                    puzzle.LoadJson(path);
                }

                puzzle._path = path;
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
                case 1:
                case 2:
                    LoadV1(reader, version); 
                    break;

                case 3:
                    LoadV3(reader, version);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Load the binary puzzle file using the V1 Loader
        /// </summary>
        private void LoadV1(BinaryReader reader, int version)
        {
            var tiles = new Tile[reader.ReadInt32()];

            for(int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var guid = reader.ReadGuid();
                var size = reader.ReadInt32();
                var start = reader.BaseStream.Position;

                // Find the tile prefab and if it doesnt exist skip the tile
                var prefab = TileDatabase.GetTile(guid);
                if(null == prefab)
                {
                    reader.BaseStream.Position = start + size;
                    continue;
                }

                // Create the tile and if it fails skip the tile
                var cell = reader.ReadCell();
                var tile = InstantiateTile(prefab, cell);
                if(tile == null)
                {
                    reader.BaseStream.Position = start + size;
                    continue;
                }

                tiles[tileIndex] = tile;

                // Read the tile properties
                while(true)
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
                            var background = BackgroundDatabase.GetBackground(reader.ReadGuid());
                            value = background;
                            break;
                        }

                        case TilePropertyType.Decal:
                        {
                            var decal = DecalDatabase.GetDecal(reader.ReadGuid());
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
                                var decal = DecalDatabase.GetDecal(reader.ReadGuid());
                                var flags = (DecalFlags)reader.ReadInt32();

                                decal.flags = flags;
                                decals[i] = decal;
                            }

                            value = decals;
                            break;
                        }

                        case TilePropertyType.Tile:
                            value = TileDatabase.GetTile(reader.ReadGuid());
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    tile.SetPropertyValue(name, value);
                }
            }

            var wires = new Wire[reader.ReadInt32()];
            var wireOrderFrom = new int[wires.Length];
            var wireOrderTo = new int[wires.Length];
            for (int wireIndex = 0; wireIndex < wires.Length; wireIndex++)
            {
                var fromIndex = reader.ReadInt32();
                wireOrderFrom[wireIndex] = reader.ReadInt32();
                var fromOptionCount = (int)reader.ReadByte();
                var fromOptions = fromOptionCount == 0 ? null : new int[fromOptionCount];
                if (fromOptions != null)
                    for (int i = 0; i < fromOptionCount; i++)
                        fromOptions[i] = reader.ReadInt32();

                var toIndex = reader.ReadInt32();
                wireOrderTo[wireIndex] = reader.ReadInt32();
                var toOptionCount = (int)reader.ReadByte();
                var toOptions = toOptionCount == 0 ? null : new int[toOptionCount];
                if (toOptions != null)
                    for (int i = 0; i < toOptionCount; i++)
                        toOptions[i] = reader.ReadInt32();

                var wire = InstantiateWire(
                    tiles[fromIndex].GetLegacyPort(PortFlow.Output), 
                    tiles[toIndex].GetLegacyPort(PortFlow.Input));
                if (null == wire)
                    continue;

                wires[wireIndex] = wire;
                wire.from.SetOptions(fromOptions);
                wire.to.SetOptions(toOptions);
            }

            // Apply the wire order after all wires are created
            for(int wireIndex = 0; wireIndex < wires.Length; wireIndex++)
            {
                var wire = wires[wireIndex];
                if (wire == null)
                    continue;

                wire.from.port.wires.Remove(wire);
                while (wire.from.port.wires.Count < wireOrderFrom[wireIndex])
                    wire.from.port.wires.Add(null);
                wire.from.port.wires.Insert(wireOrderFrom[wireIndex], wire);

                wire.to.port.wires.Remove(wire);
                while (wire.to.port.wires.Count < wireOrderTo[wireIndex])
                    wire.to.port.wires.Add(null);
                wire.to.port.wires.Insert(wireOrderTo[wireIndex], wire);
            }
        }

        private void LoadV3 (BinaryReader reader, int version)
        {
            // Create the tile array
            var tiles = new Tile[reader.ReadInt32()];

            // Instantiate all the wires
            var wires = new Wire[reader.ReadInt32()];
            for (int wireIndex = 0; wireIndex < wires.Length; wireIndex++)
                wires[wireIndex] = Instantiate(_wirePrefab, _wires).GetComponent<Wire>();

            // Instantiate the tiles
            for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var guid = reader.ReadGuid();
                var size = reader.ReadInt32();
                var start = reader.BaseStream.Position;

                // Find the tile prefab and if it doesnt exist skip the tile
                var prefab = TileDatabase.GetTile(guid);
                if (null == prefab)
                {
                    reader.BaseStream.Position = start + size;
                    continue;
                }

                // Create the tile and if it fails skip the tile
                var cell = reader.ReadCell();
                var tile = InstantiateTile(prefab, cell);
                if (tile == null)
                {
                    reader.BaseStream.Position = start + size;
                    continue;
                }

                tiles[tileIndex] = tile;

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
                            var background = BackgroundDatabase.GetBackground(reader.ReadGuid());
                            value = background;
                            break;
                        }

                        case TilePropertyType.Decal:
                        {
                            var decal = DecalDatabase.GetDecal(reader.ReadGuid());
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
                                var decal = DecalDatabase.GetDecal(reader.ReadGuid());
                                var flags = (DecalFlags)reader.ReadInt32();

                                decal.flags = flags;
                                decals[i] = decal;
                            }

                            value = decals;
                            break;
                        }

                        case TilePropertyType.Tile:
                            value = TileDatabase.GetTile(reader.ReadGuid());
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
                                var wire = wires[reader.ReadInt32()];
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

        [Serializable]
        public class SerializedPuzzle
        {
            public SerializedTile[] tiles;
            public SerializedWire[] wires;
        }

        [Serializable]
        public class SerializedTile
        {
            public Cell cell;
            public string prefab;
            public SerializedProperty[] properties;
        }

        [Serializable]
        public class SerializedWire
        {
            public int from;
            public int fromOrder;
            public int[] fromOptions;

            public int to;
            public int toOrder;
            public int[] toOptions;
        }

        [Serializable]
        public class SerializedProperty
        {
            public string name;
            public string value;
        }

        public void LoadJson(string path)
        {
            var serializedPuzzle = JsonUtility.FromJson<SerializedPuzzle>(File.ReadAllText(path));

            if (serializedPuzzle.tiles == null)
                return;

            var tilesObjects = new List<Tile>();
            foreach (var serializedTile in serializedPuzzle.tiles)
            {
                Guid.TryParse(serializedTile.prefab, out var guid);
                tilesObjects.Add(InstantiateTile(guid, serializedTile.cell));
            }

            if (serializedPuzzle.wires != null)
            {
                // Create the wires
                var wireObjects = new List<Wire>();
                foreach (var serializedWire in serializedPuzzle.wires)
                {
                    var wire = InstantiateWire(
                        tilesObjects[serializedWire.from].GetLegacyPort(PortFlow.Output), 
                        tilesObjects[serializedWire.to].GetLegacyPort(PortFlow.Input));
                    wireObjects.Add(wire);
                    if (null == wire)
                        continue;

                    wire.from.SetOptions(serializedWire.fromOptions);
                    wire.to.SetOptions(serializedWire.toOptions);
                }

                // Sort the wires
                for (int wireIndex = 0; wireIndex < wireObjects.Count; wireIndex++)
                {
                    var wire = wireObjects[wireIndex];
                    if (wire == null)
                        continue;

                    wire.from.port.wires.Remove(wire);
                    while (wire.from.port.wires.Count < serializedPuzzle.wires[wireIndex].fromOrder)
                        wire.from.port.wires.Add(null);
                    wire.from.port.wires.Insert(serializedPuzzle.wires[wireIndex].fromOrder, wire);

                    wire.to.port.wires.Remove(wire);
                    while (wire.to.port.wires.Count < serializedPuzzle.wires[wireIndex].toOrder)
                        wire.to.port.wires.Add(null);
                    wire.to.port.wires.Insert(serializedPuzzle.wires[wireIndex].toOrder, wire);
                }
            }

            for (int i = 0; i < serializedPuzzle.tiles.Length; i++)
            {
                var serializedTile = serializedPuzzle.tiles[i];
                if (serializedTile == null || serializedTile.properties == null)
                    continue;

                if (tilesObjects[i] == null)
                    continue;

                foreach (var serializedProperty in serializedTile.properties)
                {
                    if (serializedProperty == null)
                        continue;

                    try
                    {
                        tilesObjects[i].SetPropertyDeprecated(serializedProperty.name, serializedProperty.value);
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
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

#if false
        /// <summary>
        /// Return the wire that collides with the given world position
        /// </summary>
        /// <param name="position">World poisition to hit test</param>
        /// <returns>Wire that collides with the world position or null if none found</returns>
        public static Wire HitTestWire(Vector3 position)
        {
            var cell = TileGrid.WorldToCell(position + new Vector3(0.5f, 0.5f, 0));
            var threshold = _instance.wireHitThreshold * _instance.wireHitThreshold;

            for (int i = 0; i < _instance.wires.childCount; i++)
            {
                var wire = _instance.wires.GetChild(i).GetComponent<Wire>();
                var min = Cell.Min(wire.from.cell, wire.to.cell);
                var max = Cell.Max(wire.from.cell, wire.to.cell);
                if (cell.x < min.x || cell.y < min.y || cell.x > max.x || cell.y > max.y)
                    continue;

                var pt0 = wire.from.position;
                var pt1 = wire.to.position;
                var dir = (pt1 - pt0).normalized;
                var mag = (pt1 - pt0).magnitude;
                var delta = position - pt0;
                var dot = Mathf.Clamp(Vector2.Dot(dir, delta) / mag, 0, 1);
                var dist = (position - (pt0 + dir * dot * mag)).sqrMagnitude;
                if (dist > threshold)
                    continue;

                return wire;
            }

            return null;
        }
#endif
    }
}



#if false

    how to serialize ports

    - when reading in ports from v1 we need to connect any to wires to some port
        - find the port marked legacy and connect to that
        - if output is from a tile with no outputs but is a value tile then connect to the legacy number port instead

    - when loading a v3 file
        - load all wires into an array
        - when loading a port property look up the wires by index and send array to port



#endif