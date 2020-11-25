using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicAnd : PuzzledActorComponent
    {
        // NOTE: this is just a stub, we need to figure out how we want to handle this
        private int numWires = 0;
        private int liveWireCount = 0; // FIXME: this needs to keep track of each wire and count them correctly.  Or it needs to make sure a wire can't double activate/deactivate

        private void Start()
        {
            numWires = 2; // FIXME: figure out how many we need
        }

        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt)
        {
            SetLiveWireCount(liveWireCount + 1);
        }

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt)
        {
            SetLiveWireCount(liveWireCount - 1);
        }

        private void SetLiveWireCount(int count)
        {
            if (liveWireCount >= numWires)
                actor.ActivateWire();
            else
                actor.DeactivateWire();
        }
    }
}
