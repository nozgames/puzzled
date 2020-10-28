using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class LevelExit : ActorComponent
    {
        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            GameManager.Instance.actor.Send(ActorEvent.Singleton<LevelExitEvent>().Init());
        }
    }
}
