using NoZ;

namespace Puzzled
{
    class AllowMove : PuzzledActorComponent
    {
        [ActorEventHandler]
        private void OnQueryMove(QueryMoveEvent evt) => evt.result = true;
    }
}
