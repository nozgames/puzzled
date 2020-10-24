using NoZ;

namespace Puzzled
{
    class PuzzledActorComponent : ActorComponent
    {
        public new PuzzledActor actor => (PuzzledActor)base.actor;
    }
}
