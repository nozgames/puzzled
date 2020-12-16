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
            if (evt.isPlayer)
            {
                entered = true;
                tile.SetOutputsActive(true);
            }
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            if (evt.isPlayer)
            {
                entered = false;
                tile.SetOutputsActive(false);
            }
        }
    }
}
