using NoZ;

namespace Puzzled
{
    class Busy : TileComponent
    {
        protected override void OnEnable() 
        {
            base.OnEnable();
            BeginBusy();
        }

        protected override void OnDisable() 
        {
            EndBusy();
            base.OnDisable();
        }
    }
}
