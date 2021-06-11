using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Puzzled.UI;

namespace Puzzled
{
    public class GameManager : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private float _tick = 0.25f;
        [SerializeField] private Puzzle _puzzlePrefab = null;
        [SerializeField] private Transform _puzzles = null;
        [SerializeField] private CameraManager _cameraManager = null;

        /// <summary>
        /// Event fired when the current puzzle changes
        /// </summary>
        public static event Action<Puzzle> onPuzzleChanged;

        /// <summary>
        /// Event fired when the gamepad state changes
        /// </summary>
        public static event Action<bool> onGamepadChanged;

        /// <summary>
        /// True if there is an active gamepad
        /// </summary>
        public static bool isUsingGamepad => _instance == null ? false : _instance._gamepad;

        public static bool isValid => _instance != null;

        /// <summary>
        /// True if the application is in the processing of quitting
        /// </summary>
        public static bool isQuitting => _instance._quitting;

        public static float tick => _instance._tick;

        public static float tickTimeRemaining => (tick - _instance.elapsed);

        private float elapsed = 0.0f;

        private bool _isPlaying = false;

        private bool _gamepad = false;

        private bool _quitting = false;

        private static GameManager _instance = null;

        /// <summary>
        /// Current puzzle
        /// </summary>
        private Puzzle _puzzle;

        /// <summary>
        /// Get/Set the current active puzzle
        /// </summary>
        public static Puzzle puzzle {
            get => _instance._puzzle;
            set {
                if (_instance._puzzle == value)
                    return;

                if (_instance._puzzle != null)
                    _instance._puzzle.gameObject.SetActive(false);

                _instance._puzzle = value;

                if (_instance._puzzle != null)
                {
                    _instance._puzzle.gameObject.SetActive(true);
                    UIManager.SetPlayerItem(_instance._puzzle.player != null ? _instance._puzzle.player.inventory : null);
                    UIManager.ShowHud(!_instance._puzzle.isEditing);
                }
                else
                    UIManager.ShowHud(false);

                onPuzzleChanged?.Invoke(_instance._puzzle);
            }
        }

        private void Awake()
        {
            if(null != _instance)
            {
                Debug.Log("Multiple instances of GameManager in scene");
                return;
            }

            _instance = this;
        }

        public static void Initialize ()
        {
            Debug.Assert(_instance != null);

            _instance._gamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
            InputSystem.onDeviceChange += _instance.OnDeviceChanged;

            // Initialize the camera manager
            _instance._cameraManager.Initialize();

            //WorldManager.UpdateWorlds();

            // TODO: move this to a better place
            var texture = new Texture2D(512, 512);
            var colors = new Color[512 * 512];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            texture.Apply();
            Shader.SetGlobalTexture("_void_texture", texture);
        }

        public static void Shutdown()
        {
            _instance._quitting = true;

            // Destroy the UI first
            UIManager._instance.gameObject.SetActive(false);
            Destroy(UIManager._instance.gameObject);

            puzzle = null;

            // Destroy all remaining puzzles
            for (int i = _instance._puzzles.childCount - 1; i >= 0; i--)
                _instance._puzzles.GetChild(i).GetComponent<Puzzle>().Destroy();

            InputSystem.onDeviceChange -= _instance.OnDeviceChanged;
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

        public static bool isPlaying {
            get => _instance._isPlaying;
            set {
                _instance._isPlaying = value;
            }
        }

        /// <summary>
        /// Load a puzzle at the given path and set it to the current puzzle
        /// </summary>
        /// <param name="path">Puzzle path</param>
        /// <returns>Loaded puzzle</returns>
        public static Puzzle LoadPuzzle(World.IPuzzleEntry puzzleEntry)
        {
            // Disable the puzzle first to ensure two puzzles are never enabled at the same time
            var oldPuzzle = Puzzle.current;
            Puzzle.current = null;

            // Load and set the new puzzle
            var puzzle = puzzleEntry.Load();
            Puzzle.current = puzzle;

            // If the new puzzle failed to load then set the old puzzle back to active
            if (Puzzle.current == null)
                Puzzle.current = oldPuzzle;

            LightmapManager.Render();

            return Puzzle.current;
        }

        /// <summary>
        /// Create a new puzzle and deactivate it
        /// </summary>
        /// <returns>New puzzle</returns>
        public static Puzzle InstantiatePuzzle ()
        {
            var puzzle = Instantiate(_instance._puzzlePrefab.gameObject, _instance._puzzles).GetComponent<Puzzle>();
            return puzzle;
        }

        /// <summary>
        /// Unload the current puzzle
        /// </summary>
        public static void UnloadPuzzle ()
        {
            if (Puzzle.current == null)
                return;

            Puzzle.current.Destroy();
        }

        public static void PuzzleComplete ()
        {
            //UIManager._instance.ShowPuzzleComplete();
        }

        private void Update()
        {
            if (!isPlaying)
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
            isPlaying = true;

            CameraManager.ShowLetterbox(true);
            CameraManager.ShowGizmos(false);
            CameraManager.ShowWires(false);
            CameraManager.ShowFog();
            CameraManager.ShowLayer(TileLayer.Dynamic, true);
            CameraManager.ShowLayer(TileLayer.Floor, true);
            CameraManager.ShowLayer(TileLayer.Static, true);
            CameraManager.ShowLayer(TileLayer.Logic, false);
            CameraManager.ShowLayer(TileLayer.Wall, true);
        }

        public static void Stop ()
        {
            if (puzzle == null)
                return;

            UIManager.ClosePopup();

            isPlaying = false;
        }

        private void OnDeviceChanged(InputDevice inputDevice, InputDeviceChange deviceChange)
        {
            _gamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
            onGamepadChanged?.Invoke(_gamepad);
        }
    }
}
