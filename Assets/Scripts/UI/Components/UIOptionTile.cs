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
                    ((TilePropertyOption)target).SetValue(tile.guid);
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
