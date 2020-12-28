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

            label = ((TilePropertyOption)target).name;
            UpdatePreview();
        }

        public void OnSelectTile ()
        {
            UIPuzzleEditor.instance.ChooseTile(typeof(Item), 
                (tile) => {
                    var option = ((TilePropertyOption)target);
                    UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, tile.guid));
                    UpdatePreview();
                }
            );
        }

        private void UpdatePreview()
        {
            preview.texture = TileDatabase.GetPreview(((TilePropertyOption)target).GetValue<System.Guid>());
            preview.gameObject.SetActive(preview.texture != null);
        }
    }
}
