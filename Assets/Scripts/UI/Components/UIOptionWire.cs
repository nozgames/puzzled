
namespace Puzzled
{
    class UIOptionWire : UIOptionEditor
    {
        protected override void OnTargetChanged(object target)
        {
            var wire = (Wire)target;
        }
    }
}
