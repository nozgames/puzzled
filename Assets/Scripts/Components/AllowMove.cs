using NoZ;

namespace Puzzled
{
    class AllowMove : TileComponent
    {
        [ActorEventHandler]
        private void OnQueryMove(QueryMoveEvent evt) => evt.result = true;
    }
}
