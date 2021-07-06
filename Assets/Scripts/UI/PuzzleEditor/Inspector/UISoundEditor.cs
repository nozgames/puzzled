using NoZ;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UISoundEditor : UIPropertyEditor
    {
        [SerializeField] private Image _preview = null;
        [SerializeField] private Button _chooseButton = null;
        [SerializeField] private TMPro.TextMeshProUGUI _previewText = null;
        [SerializeField] private Sprite _previewNone = null;
        [SerializeField] private Sprite _previewSound = null;
        [SerializeField] private Button _playButton = null;

        private void Awake()
        {
            _playButton.onClick.AddListener(() => {
                var sound = target.GetValue<Sound>();
                if(sound.clip != null)
                {
                    AudioManager.Instance.Play(sound.clip);
                }
            });

            _chooseButton.onClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseSound(
                    (sound) => {
                        target.SetValue(sound);
                        UpdatePreview();
                    },
                    target.GetValue<Sound>());
            });
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var sound = target.GetValue<Sound>();
            _preview.sprite = sound.clip != null ? _previewSound : _previewNone;
            _previewText.text = sound.clip != null ? sound.clip.name : "None";

            _playButton.gameObject.SetActive(sound.clip != null);
        }
    }
}
