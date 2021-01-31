using System.Collections;
using UnityEngine;

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
