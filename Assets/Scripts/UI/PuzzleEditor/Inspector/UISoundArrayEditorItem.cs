using System;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    class UISoundArrayEditorItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;
        [SerializeField] private TMPro.TextMeshProUGUI _name = null;
        [SerializeField] private UIDoubleClick _doubleClick = null;
        [SerializeField] private Button _deleteButton = null;
        [SerializeField] private Image _previewNone = null;
        [SerializeField] private Image _previewSound = null;

        public event Action<UISoundArrayEditorItem> onDeleted;
        public event Action<UISoundArrayEditorItem> onValueChanged;

        private Sound _soundValue;

        public Sound value
        {
            get => _soundValue;
            set
            {
                _soundValue = value;

                if (_name != null)
                    _name.text = (_soundValue.clip != null) ? _soundValue.clip.name : "None";

                _previewNone.gameObject.SetActive(_soundValue.clip == null);
                _previewSound.gameObject.SetActive(_soundValue.clip != null);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _deleteButton.onClick.AddListener(() =>
            {
                onDeleted?.Invoke(this);
            });

            _doubleClick.onDoubleClick.AddListener(() =>
            {
                UIPuzzleEditor.instance.ChooseSound(
                    (sound) =>
                    {
                        value = sound;
                        onValueChanged?.Invoke(this);
                    },
                    value);
            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_index != null)
                _index.text = transform.GetSiblingIndex().ToString();
        }
    }
}
