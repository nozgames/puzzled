using System.Collections;
using UnityEngine;
using Puzzled.UI;
using Puzzled.Editor;

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
            DatabaseManager.Shutdown();
            GameManager.Shutdown();
        }

        private IEnumerator Initialize()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();
            UIManager.loading = true;
            DatabaseManager.Initialize();
            GameManager.Initialize();
            UIManager.Initialize();
            DatabaseManager.GeneratePreviews();
            yield return new WaitForSeconds(1.0f - (stopwatch.ElapsedMilliseconds / 1000.0f));
            UIManager.loading = false;
        }
    }
}
