using NoZ;

namespace Puzzled
{
    class Busy : PuzzledActorComponent
    {
        protected override void OnEnable() 
        {
            BeginBusy();
        }

        protected override void OnDisable() 
        {
            EndBusy();
        }
    }
}
