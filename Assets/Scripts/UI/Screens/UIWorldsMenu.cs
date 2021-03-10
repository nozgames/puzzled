using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    class UIWorldsMenu : UIScreen
    {
        public void OnBackButton()
        {
            UIManager.ShowMainMenu();
        }
    }
}
