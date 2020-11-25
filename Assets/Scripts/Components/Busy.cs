using NoZ;

namespace Puzzled
{
    class Busy : TileComponent
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
