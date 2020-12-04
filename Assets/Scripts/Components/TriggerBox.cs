using NoZ;
using UnityEngine;

namespace Puzzled
{
    class TriggerBox : TileComponent
    {
        public bool entered { get; private set; }

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            entered = true;
            tile.SetOutputsActive(true);
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            entered = false;
            tile.SetOutputsActive(false);
        }
    }
}
