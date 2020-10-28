using System.Collections.Generic;
using UnityEngine;
using NoZ;
using UnityEditor;

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


        public void TeleportToAsync (Vector2Int cell) 
        {
        }

        public void MoveLeftAsync() => MoveToAsync(Cell + new Vector2Int(-1, 0));

        public void MoveRightAsync () => MoveToAsync(Cell + new Vector2Int(1, 0));

        public void MoveUpAsync () => MoveToAsync(Cell + new Vector2Int(0, 1));

        public void MoveDownAsync () => MoveToAsync(Cell + new Vector2Int(0, -1));       

        private void MoveToAsync (Vector2Int cell) 
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), cell);

            GameManager.Instance.SetActorCell(this, cell);

            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), cell);

            // TODO: async 
            // TODO: assumes that the movement is legal
            // TODO: Sends start move event
            // TODO: Sends leaving event
        }

        public bool QueryMoveLeft() => QueryMove(Cell + new Vector2Int(-1, 0));
        public bool QueryMoveRight() => QueryMove(Cell + new Vector2Int(1, 0));
        public bool QueryMoveUp() => QueryMove(Cell + new Vector2Int(0, 1));
        public bool QueryMoveDown() => QueryMove(Cell + new Vector2Int(0, -1));

        private bool QueryMove (Vector2Int cell) 
        {
            var query = ActorEvent.Singleton<QueryMoveEvent>().Init(cell);
            GameManager.Instance.SendToCell(query, query.Cell);
            return query.Result;
        }
    }
}
