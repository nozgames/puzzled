using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionTile : UIPropertyEditor
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
                        UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, tile.guid));
                        UpdatePreview();
                    });
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
            var tile = DatabaseManager.GetTile(target.GetValue<System.Guid>());
            preview.texture = DatabaseManager.GetPreview(tile);
            preview.gameObject.SetActive(preview.texture != null);
            _previewText.name = tile == null ? "None" : tile.name;
        }
    }
}
