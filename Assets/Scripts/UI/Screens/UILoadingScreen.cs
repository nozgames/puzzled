using System.Collections;
using UnityEngine;

namespace Puzzled
{
    public class UILoadingScreen : UIScreen
    {
        private void OnEnable()
        {
            StartCoroutine(WaitForLoad());
        }

        private IEnumerator WaitForLoad ()
        {
            while (!TileDatabase.isLoaded || !DecalDatabase.isLoaded || !BackgroundDatabase.isLoaded || !SFXDatabase.isLoaded)
                yield return null;

            UIManager.instance.ShowMainMenu();
        }
    }
}

