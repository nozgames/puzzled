using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;

namespace Puzzled
{
    public class UIOptionDecal : UIOptionEditor
    {
        [SerializeField] private UIDecalEditor _decalEditor = null;

        private void Awake()
        {
            _decalEditor.onDecalChanged += (decal) => {
                var option = ((TilePropertyOption)target);
                UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, decal));
            };
        }

        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            var option = ((TilePropertyOption)target);
            label = option.name;
            _decalEditor.decal = option.GetValue<Decal>();
        }
    }
}
