using System;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    class UIDecalArrayEditorItem : UIListItem
    {
        [SerializeField] private UIDecalPreview _decalPreview = null;
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;
        [SerializeField] private Button _deleteButton = null;

        public event Action<UIDecalArrayEditorItem> onDeleted;

        public Decal value {
            get => _decalPreview.decal;
            set => _decalPreview.decal = value;
        }

        protected override void Awake()
        {
            base.Awake();

            _deleteButton.onClick.AddListener(() => {
                onDeleted?.Invoke(this);
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
