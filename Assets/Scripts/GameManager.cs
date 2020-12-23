﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
        /// True if there is an active gamepad
        /// </summary>
        public static bool isUsingGamepad => _instance._gamepad;

        public static bool isValid => _instance != null;

        /// <summary>
        /// True if the application is in the processing of quitting
        /// </summary>
        public static bool isQuitting => _instance._quitting;

        public static float tick => _instance._tick;

        private float elapsed = 0.0f;

        private bool _paused = false;

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
                    _instance._puzzle.gameObject.SetActive(true);

                onPuzzleChanged?.Invoke(_instance._puzzle);
            }
        }

        public static Puzzle GetPuzzle (string name)
        {
            for(int i=0; i<_instance._puzzles.childCount; i++)
            {
                var puzzle = _instance._puzzles.GetChild(i).GetComponent<Puzzle>();
                if (!puzzle.isEditing && puzzle.filename == name)
                    return puzzle;                
            }

            return null;
        }

        private void OnEnable()
        {
            _gamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
            InputSystem.onDeviceChange += OnDeviceChanged;

            if (_instance == null)
                _instance = this;

            // Initialize the camera manager
            _cameraManager.Initialize();
        }

        private void OnApplicationQuit()
        {
            _quitting = true;

            // Destroy the UI first
            UIManager.instance.gameObject.SetActive(false);
            Destroy(UIManager.instance.gameObject);

            puzzle = null;

            // Destroy all remaining puzzles
            for (int i = _puzzles.childCount - 1; i >= 0; i--)
                _puzzles.GetChild(i).GetComponent<Puzzle>().Destroy();

            if (_instance == this)
                _instance = null;

            InputSystem.onDeviceChange -= OnDeviceChanged;
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

        /// <summary>
        /// Load a puzzle at the given path and set it to the current puzzle
        /// </summary>
        /// <param name="path">Puzzle path</param>
        /// <returns>Loaded puzzle</returns>
        public static Puzzle LoadPuzzle(string path, bool editing=false)
        {
            // Disable the puzzle first to ensure two puzzles are never enabled at the same time
            var oldPuzzle = Puzzle.current;
            Puzzle.current = null;

            // Load and set the new puzzle
            var puzzle = Puzzle.Load(path);
            puzzle.isEditing = true;
            Puzzle.current = puzzle;

            // If the new puzzle failed to load then set the old puzzle back to active
            if (Puzzle.current == null)
                Puzzle.current = oldPuzzle;

            return Puzzle.current;
        }

        /// <summary>
        /// Create a new puzzle and deactivate it
        /// </summary>
        /// <returns>New puzzle</returns>
        public static Puzzle InstantiatePuzzle ()
        {
            var puzzle = Instantiate(_instance._puzzlePrefab.gameObject, _instance._puzzles).GetComponent<Puzzle>();
            puzzle.gameObject.SetActive(false);
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
            UIManager.instance.ShowPuzzleComplete();
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
            paused = false;

            CameraManager.Play();
        }

        public static void Stop ()
        {
            CameraManager.Stop();
            
            paused = true;
        }

        private void OnDeviceChanged(InputDevice inputDevice, InputDeviceChange deviceChange)
        {
            _gamepad = InputSystem.devices.Where(d => d.enabled && d is Gamepad).Any();
        }
    }
}
