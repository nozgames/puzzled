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

        /// <summary>
        /// Cell the actor is current in
        /// </summary>
        public Vector2Int Cell => GameManager.Instance.GetActorCell(this);

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);
    }
}
