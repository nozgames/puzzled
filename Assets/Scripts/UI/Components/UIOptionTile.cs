using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionTile : UIOptionEditor
    {
        [SerializeField] private RawImage preview = null;

        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            var editableProperty = (TileEditorInfo.EditableProperty)target;
            label = NicifyName(editableProperty.property.Name);
            if(System.Guid.TryParse(editableProperty.GetValue(), out var guid))
                preview.texture = TileDatabase.GetPreview(guid);
        }

        public void OnSelectTile ()
        {
            UIPuzzleEditor.instance.OpenTileSelector(typeof(Item), 
                (tile) => {
                    var editableProperty = (TileEditorInfo.EditableProperty)target;
                    editableProperty.SetValue(tile.guid.ToString());
                    preview.texture = TileDatabase.GetPreview(tile.guid);
                }
            );
        }
    }
}
