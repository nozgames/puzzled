using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;

namespace Puzzled
{
    public class UIOptionDecal : UIPropertyEditor
    {
        [SerializeField] private UIDecalEditor _decalEditor = null;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            label = target.name;
            _decalEditor.interactable = target.tile.info.layer != TileLayer.Floor || target.tile.puzzle.grid.CellToTile(target.tile.cell, TileLayer.Static) == null;
            _decalEditor.decal = target.GetValue<Decal>();
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
