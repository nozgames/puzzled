using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class PuzzledActor : Actor
    {
        private static Dictionary<Vector2Int, List<PuzzledActor>> cells;

        private Vector2Int cell;

        /// <summary>
        /// Cell the actor is current in
        /// </summary>
        public Vector2Int Cell {
            get => cell;
            set {
                GameManager.Instance.SetActorCell(this, value);
                cell = value;
            }
        }

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);
    }
}
