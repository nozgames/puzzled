using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionDecal : UIOptionEditor
    {
        [SerializeField] private Image preview = null;

        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            label = ((TilePropertyOption)target).name;
            UpdatePreview();
        }

        public void ChooseDecal()
        {
            UIPuzzleEditor.instance.ChooseDecal ((decal) => {
                ((TilePropertyOption)target).SetValue(decal);
                UpdatePreview();
            });
        }

        private void UpdatePreview()
        {
            preview.sprite = ((TilePropertyOption)target).GetValue<Decal>()?.sprite;
            preview.gameObject.SetActive(preview.sprite != null);
        }
    }
}
