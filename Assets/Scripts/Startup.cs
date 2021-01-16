using System.Collections;
using UnityEngine;

namespace Puzzled
{
    public class Startup : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(Initialize());
        }

        private void OnApplicationQuit()
        {
            UIPuzzleEditor.Shutdown();
            UIManager.Shutdown();
            GameManager.Shutdown();
        }

        private IEnumerator Initialize()
        {
            // Wait for the databases to load
            while (!TileDatabase.isLoaded || !DecalDatabase.isLoaded || !BackgroundDatabase.isLoaded || !SFXDatabase.isLoaded)
                yield return null;

            GameManager.Initialize();
            UIManager.Initialize();
            UIManager.loading = false;
        }
    }
}
