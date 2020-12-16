using NoZ;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Transform _pan = null;
        [SerializeField] private Grid grid = null;
        [SerializeField] private Transform wires = null;
        [SerializeField] private GameObject wirePrefab = null;
        [SerializeField] private float wireHitThreshold = 0.1f;
        [SerializeField] private float _tick = 0.25f;
            
        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;
        [SerializeField] private LayerMask playLayers = 0;
        [SerializeField] private LayerMask defaultLayers = 0;

        public InputActionReference menuAction;

        /// <summary>
        /// Current player
        /// </summary>
        private Player _player;

        public static bool isUsingGamepad => _instance._gamepad;

        public static Player player {
            get => _instance._player;
            set => _instance._player = value;
        }

        public static Cell playerCell => player != null ? player.tile.cell : Cell.invalid;

        public static bool isValid => _instance != null;

        public static float tick => _instance._tick;

        private float elapsed = 0.0f;

        private bool _paused = false;

        private bool _gamepad = false;

        private static GameManager _instance = null;

        /// <summary>
        /// Returns the active puzzle
        /// </summary>
        public Puzzle puzzle { get; private set; }

        private void OnEnable()
        {
            //menuAction.action.Enable();
            //menuAction.action.performed += OnMenuAction;

            _gamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
            InputSystem.onDeviceChange += OnDeviceChanged;

            if (_instance == null)
                _instance = this;

            // Set default camera mask layers
            Camera.main.cullingMask = _instance.defaultLayers;
        }

        private void OnApplicationQuit()
        {
            // Destroy the UI first
            UIManager.instance.gameObject.SetActive(false);
            Destroy(UIManager.instance.gameObject);

            UnloadPuzzle();
        }

        private void OnDisable()
        {
            if (_instance == this)
                _instance = null;

            InputSystem.onDeviceChange -= OnDeviceChanged;

            menuAction.action.Disable();
            menuAction.action.performed -= OnMenuAction;
        }

        private void OnMenuAction(InputAction.CallbackContext obj)
        {
            UIManager.instance.ShowIngame();
        }

        private int _busy = 0;

        public static bool isBusy => _instance._busy > 0;

        public static int busy {
            get => _instance._busy;
            set {
                _instance._busy = value;
                // TODO: event?
            }
        }

        public static bool paused {
            get => _instance._paused;
            set {
                _instance._paused = value;
            }
        }

        public static void LoadPuzzle(Puzzle puzzle) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unload the current puzzle
        /// </summary>
        public static void UnloadPuzzle ()
        {
            // Destroy the player
            if (_instance._player != null)
            {
                Destroy(_instance._player.gameObject);
                _instance._player = null;
            }

            // Unlink all tiles and destory them
            TileGrid.UnlinkAll(true);
        }

        public static void PuzzleComplete ()
        {
            UIManager.instance.ShowPuzzleComplete();
        }

        public static Tile InstantiateTile(Guid guid, Cell cell) =>
            InstantiateTile(TileDatabase.GetTile(guid), cell);

        public static Tile InstantiateTile (Tile prefab, Cell cell)
        {
            if (prefab == null)
                return null;

            // Do not allow two tiles to be intstantiated into the same cell
            if(TileGrid.IsLinked(cell, prefab.info.layer))
            {
                Debug.LogError($"Cannot create tile `{prefab.info.displayName} at cell {cell}, layer is occupied by `{TileGrid.CellToTile(cell, prefab.info.layer).info.displayName}");
                return null;
            }

            var tile = Instantiate(prefab.gameObject, _instance.grid.transform).GetComponent<Tile>();
            tile.guid = prefab.guid;
            tile.cell = cell;
            tile.gameObject.SetChildLayers(TileLayerToObjectLayer(tile.info.layer));
            return tile;
        }

        private static int TileLayerToObjectLayer (TileLayer layer)
        {
            switch (layer)
            {
                case TileLayer.Floor:
                    return _instance.floorLayer;

                case TileLayer.Static:
                    return _instance.staticLayer;

                case TileLayer.Dynamic:
                    return _instance.dynamicLayer;

                case TileLayer.Logic:
                    return _instance.logicLayer;
            }

            return 0;
        }

        public static Wire InstantiateWire (Tile input, Tile output)
        {
            if (input == null || output == null)
                return null;

            if (!output.info.allowWireInputs || !input.info.allowWireOutputs)
                return null;

            if (input == output)
                return null;

            // Already connected?
            if (input.HasOutput(output))
                return null;

            var wire = Instantiate(_instance.wirePrefab, _instance.wires.transform).GetComponent<Wire>();
            wire.from.tile = input;
            wire.to.tile = output;
            input.outputs.Add(wire);
            output.inputs.Add(wire);
            wire.transform.position = TileGrid.CellToWorld(wire.from.tile.cell);
            return wire;
        }

        public static void HideWires() => ShowWires(false);

        public static void ShowWires(bool visible)
        {
            var wires = _instance.wires;
            for (int i = 0; i < wires.transform.childCount; i++)
                wires.transform.GetChild(i).GetComponent<Wire>().visible = visible;
        }

        public static void ShowWires(Tile tile)
        {
            foreach (var output in tile.outputs)
                output.visible = true;

            foreach (var input in tile.inputs)
                input.visible = true;
        }

        /// <summary>
        /// Return the wire that collides with the given world position
        /// </summary>
        /// <param name="position">World poisition to hit test</param>
        /// <returns>Wire that collides with the world position or null if none found</returns>
        public static Wire HitTestWire (Vector3 position)
        {
            var cell = TileGrid.WorldToCell(position + new Vector3(0.5f, 0.5f, 0));
            var threshold = _instance.wireHitThreshold * _instance.wireHitThreshold;

            for (int i=0; i<_instance.wires.childCount; i++)
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

        public static void Pan(Vector3 pan)
        {
            _instance._pan.position += Vector3.Scale(pan, new Vector3(1,1,0));
        }

        public static void PanCenter ()
        {
            // TODO: find center of puzzle
            _instance._pan.position = Vector3.zero;
        }

        private void Update()
        {
            if (paused)
                return;

            if (tick <= 0.01f)
                return;

            elapsed += Time.deltaTime;
            while (elapsed > tick)
            {
                Tile.Tick();
                elapsed -= tick;
            }
        }

        public static void Play ()
        {
            Camera.main.cullingMask = _instance.playLayers;
            paused = false;

            // Send a start event to all actors
            var start = new StartEvent();
            var grid = _instance.grid.transform;
            for(int i=0; i<grid.childCount; i++)
            {
                var tile = grid.GetChild(i).GetComponent<Actor>();
                if (null == tile)
                    continue;

                tile.Send(start);
            }

            CameraManager.Play();
        }

        public static void Stop ()
        {
            CameraManager.Stop();

            Camera.main.cullingMask = _instance.defaultLayers;
            paused = true;
        }

        public static void ShowLayer (TileLayer layer, bool show)
        {
            if (show)
                Camera.main.cullingMask |= (1<<TileLayerToObjectLayer(layer));
            else
                Camera.main.cullingMask &= ~(1<<TileLayerToObjectLayer(layer));
        }

        private void OnDeviceChanged(InputDevice inputDevice, InputDeviceChange deviceChange)
        {
            _gamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
        }
    }
}
