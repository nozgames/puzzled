using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicXAnd : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt) => UpdateState();

        private void UpdateState()
        {
            bool bMatches = true;
            for (int i = 0; i < tile.inputCount; ++i)
            {
                if (tile.inputs[i].enabled != (tile.GetInputOption(i, 0) == 1))
                {
                    bMatches = false;
                    break;
                }
            }

            tile.SetOutputsActive(bMatches);
        }
    }
}
