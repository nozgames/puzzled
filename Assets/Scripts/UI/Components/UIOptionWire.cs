using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UIOptionWire : UIOptionEditor
    {
        [SerializeField] private Toggle _toggle = null;
        [SerializeField] private TMPro.TextMeshProUGUI _tileName = null;

        private UIOptionWires wiresEditor = null;

        private void Awake()
        {
            _toggle.group = GetComponentInParent<ToggleGroup>();
            wiresEditor = GetComponentInParent<UIOptionWires>();            
        }

        protected override void OnTargetChanged(object target)
        {
            var wire = (Wire)target;
            var tile = wiresEditor.isInput ? wire.from.tile : wire.to.tile;
            
            _tileName.text = tile.info.displayName;

            UpdateIndex();
        }

        public void OnSelectionChanged (bool selected)
        {
            if(selected)
                wiresEditor.OnSelectionChanged(this);
        }

        public void UpdateIndex()
        {
            label = (transform.GetSiblingIndex() + 1).ToString();
        }
    }
}
