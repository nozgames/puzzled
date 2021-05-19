using Puzzled.Editor;
using UnityEngine;

namespace Puzzled.UI
{
    class UIHudScreen : UIScreen
    {
        public override void HandleMenuInput()
        {
            if (!UIPuzzleEditor.isOpen)
                UIManager.TogglePauseScreen();
        }
    }
}
