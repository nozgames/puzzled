using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UIItemEditor : UIPropertyEditor
    {
        [SerializeField] private RawImage preview = null;
        [SerializeField] private UIDoubleClick _doubleClick = null;
        [SerializeField] private TMPro.TextMeshProUGUI _previewText = null;

        private void Awake()
        {
            _doubleClick.onDoubleClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseTile(
                    typeof(Item),
                    DatabaseManager.GetTile(target.GetValue<System.Guid>()),
                    (tile) => {
                        target.SetValue(tile.guid);
                        UpdatePreview();
                    });
            });
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var tile = DatabaseManager.GetTile(target.GetValue<System.Guid>());
            preview.texture = DatabaseManager.GetPreview(tile);
            preview.gameObject.SetActive(preview.texture != null);
            _previewText.name = tile == null ? "None" : tile.name;
        }
    }
}
