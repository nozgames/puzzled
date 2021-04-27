using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;

namespace Puzzled
{
    public class UIOptionDecal : UIPropertyEditor
    {
        [SerializeField] private UIDecalPreview _decalEditor = null;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            // Hide the decal option if the decal isnt the top most in the cell
            //gameObject.SetActive(DecalSurface.FromCell(target.tile.puzzle, target.tile.cell) == DecalSurface.FromTile(target.tile));

            _decalEditor.decal = target.GetValue<Decal>();
            //_decalEditor.onDecalChanged -= OnDecalChanged;
            //_decalEditor.onDecalChanged += OnDecalChanged;
        }

        private void OnDecalChanged(Decal decal)
        {
            target.SetValue(decal);
        }
    }
}
