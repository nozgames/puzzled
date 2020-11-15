using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LevelExit : PuzzledActorComponent
    {
        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            SendToCell(new LevelExitEvent(), actor.Cell);
            actor.gameObject.SetActive(false);
        }
    }
}
