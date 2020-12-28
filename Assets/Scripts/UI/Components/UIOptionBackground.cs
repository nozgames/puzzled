using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIOptionBackground : UIOptionEditor
    {
        [SerializeField] private Image _preview = null;
        [SerializeField] private UIDoubleClick _doubleClick= null;
        [SerializeField] private TMPro.TextMeshProUGUI _previewText = null;

        private void Awake()
        {
            _doubleClick.onDoubleClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseBackground(
                    (background) => {
                        var option = ((TilePropertyOption)target);
                        UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, background));
                        UpdatePreview();
                    }, 
                    ((TilePropertyOption)target).GetValue<Background>());
            });
        }

        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            label = ((TilePropertyOption)target).name;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var background = ((TilePropertyOption)target).GetValue<Background>();
            _preview.color = background != null ? background.color : Color.clear;
            _preview.gameObject.SetActive(background != null);

            _previewText.text = background != null ? background.name : "None";
        }
    }
}
