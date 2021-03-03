using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UIDecalArrayEditorItem : UIListItem
    {
        [SerializeField] private UIDecalEditor _decalEditor = null;
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;
        [SerializeField] private Button _deleteButton = null;

        public event Action<Decal> onValueChanged;
        public event Action<UIDecalArrayEditorItem> onDeleted;

        public Decal value {
            get => _decalEditor.decal;
            set => _decalEditor.decal = value;
        }

        protected override void Awake()
        {
            base.Awake();

            _deleteButton.onClick.AddListener(() => {
                onDeleted?.Invoke(this);
            });

            _decalEditor.onDecalChanged += (value) => onValueChanged?.Invoke(value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_index != null)
                _index.text = transform.GetSiblingIndex().ToString();
        }
    }
}
