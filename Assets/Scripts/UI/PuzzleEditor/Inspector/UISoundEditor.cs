using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UISoundEditor : UIPropertyEditor
    {
        [SerializeField] private Image _preview = null;
        [SerializeField] private UIDoubleClick _doubleClick = null;
        [SerializeField] private TMPro.TextMeshProUGUI _previewText = null;

        private void Awake()
        {
            _doubleClick.onDoubleClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseSound(
                    (sound) => {
                        UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, sound));
                        UpdatePreview();
                    },
                    target.GetValue<Sound>());
            });
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            label = target.name;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var sound = target.GetValue<Sound>();
            //_preview.color = sound != null ? sound.color : Color.clear;
            //_preview.gameObject.SetActive(sound != null);

            _previewText.text = sound.clip != null ? sound.clip.name : "None";
        }
    }
}
