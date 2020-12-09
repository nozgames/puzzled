using UnityEngine;

namespace Puzzled
{
    public class UIOptionTile : UIOptionEditor
    {
        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            label = NicifyName(((TileEditorInfo.EditableProperty)target).property.Name);
        }
    }
}
