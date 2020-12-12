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

            var option = (TilePropertyOption)target;
            label = option.name;
            preview.texture = TileDatabase.GetPreview(option.GetValueGuid());
        }

        public void OnSelectTile ()
        {
            UIPuzzleEditor.instance.OpenTileSelector(typeof(Item), 
                (tile) => {
                    var option = (TilePropertyOption)target;
                    option.SetValue(tile.guid);
                    preview.texture = TileDatabase.GetPreview(tile.guid);
                }
            );
        }
    }
}
