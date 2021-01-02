using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;

namespace Puzzled
{
    public class UIOptionDecal : UIOptionEditor
    {
        [SerializeField] private UIDecalEditor _decalEditor = null;

        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            var option = ((TilePropertyEditorTarget)target);
            label = option.name;
            _decalEditor.interactable = option.tile.info.layer != TileLayer.Floor || option.tile.puzzle.grid.CellToTile(option.tile.cell, TileLayer.Static) == null;
            _decalEditor.decal = option.GetValue<Decal>();
            _decalEditor.onDecalChanged -= OnDecalChanged;
            _decalEditor.onDecalChanged += OnDecalChanged;
        }

        private void OnDecalChanged(Decal decal)
        {
            var option = ((TilePropertyEditorTarget)target);
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, decal));
        }
    }
}
