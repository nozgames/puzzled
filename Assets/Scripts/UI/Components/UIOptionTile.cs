using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionTile : UIPropertyEditor
    {
        [SerializeField] private RawImage preview = null;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            label = target.name;
            UpdatePreview();
        }

        public void OnSelectTile ()
        {
            UIPuzzleEditor.instance.ChooseTile(typeof(Item), 
                (tile) => {
                    var option = ((TilePropertyEditorTarget)target);
                    UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, tile.guid));
                    UpdatePreview();
                }
            );
        }

        private void UpdatePreview()
        {
            preview.texture = TileDatabase.GetPreview(((TilePropertyEditorTarget)target).GetValue<System.Guid>());
            preview.gameObject.SetActive(preview.texture != null);
        }
    }
}
