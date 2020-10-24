using NoZ;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Grid grid = null;
        [SerializeField] private Canvas ui = null;
        [SerializeField] private int minimumPuzzleSize = 9;

        private Dictionary<Vector2Int, List<PuzzledActor>> cells;

        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Returns the active puzzle
        /// </summary>
        public Puzzle Puzzle { get; private set; }

        /// <summary>
        /// Returns the current active puzzle theme
        /// </summary>
        public Theme Theme => Puzzle.Theme;

        public Puzzle TestPuzzle = null;

        private void OnEnable()
        {
            if(Instance == null)
                Instance = this;

            if(TestPuzzle != null)
            {
                LoadPuzzle(TestPuzzle);
            }

            LinkActors();
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        public void LoadPuzzle (Puzzle puzzle)
        {
            //Screen.currentResolution
            //canvasScaler.referencePixelsPerUnit
            //canvasScaler.referenceResolution
            Camera.main.orthographicSize = /*ui.transform.localScale.x * */(Mathf.Max(minimumPuzzleSize, puzzle.Height) + 1) * 0.5f;

            for(int y=0; y<puzzle.Height; y++)
                Instantiate(puzzle.Theme.floor, grid.GetCellCenterWorld(new Vector3Int(0,-puzzle.Height/2 + y, 0)), Quaternion.identity, grid.transform);
        }

        /// <summary>
        /// Link all actors that are children of the grid to a cell and fix their world position
        /// </summary>
        private void LinkActors()
        {
            cells = new Dictionary<Vector2Int, List<PuzzledActor>>();

            var actors = grid.GetComponentsInChildren<PuzzledActor>();
            foreach (var actor in actors)
            {
                SetActorCell(actor, actor.Cell);

                actor.transform.position = grid.CellToWorld(actor.Cell.ToVector3Int());
            }
        }

        /// <summary>
        /// Return the cell for a given actor
        /// </summary>
        public Vector2Int GetActorCell(PuzzledActor actor) => grid.WorldToCell(actor.transform.position).ToVector2Int();

        /// <summary>
        /// Set the actors current cell
        /// </summary>
        public void SetActorCell(PuzzledActor actor, Vector2Int cell)
        {
            // Remove the actor from the previous cell
            GetCellActors(actor.Cell)?.Remove(actor);

            // Add the actor to the give ncell
            var actors = GetCellActors(cell);
            if (null == actors)
            {
                actors = new List<PuzzledActor>(4) { actor };
                cells[cell] = actors;
            }
            else
                actors.Add(actor);

            actor.transform.position = grid.CellToWorld(cell.ToVector3Int());
        }

        /// <summary>
        /// Returns the list of actors at the given cell
        /// </summary>
        public List<PuzzledActor> GetCellActors(Vector2Int cell) =>
            cells.TryGetValue(cell, out var actors) ? actors : null;

        /// <summary>
        /// Send an event to a given cell
        /// </summary>
        public void SendToCell<TEvent>(TEvent evt, Vector2Int cell) where TEvent : ActorEvent
        {
            var actors = GetCellActors(cell);
            if (null == actors)
                return;

            foreach (var actor in actors)
                actor.Send(evt);
        }
    }
}
