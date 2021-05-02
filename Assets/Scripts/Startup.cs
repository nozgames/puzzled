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
            //StartCoroutine(Initialize());
            DatabaseManager.Initialize();
            GameManager.Initialize();
            UIManager.Initialize();
            UIManager.loading = false;
        }

        private void OnApplicationQuit()
        {
            UIPuzzleEditor.Shutdown();
            UIManager.Shutdown();
            DatabaseManager.Shutdown();
            GameManager.Shutdown();
        }

#if false
        private IEnumerator Initialize()
        {
            DatabaseManager.Initialize();
            GameManager.Initialize();
            UIManager.Initialize();
            UIManager.loading = false;
        }
#endif
    }
}
