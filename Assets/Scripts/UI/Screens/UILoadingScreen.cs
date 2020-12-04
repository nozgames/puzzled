using System.Collections;
using UnityEngine;

namespace Puzzled
{
    public class UILoadingScreen : UIScreen
    {
        [SerializeField] private AddressableDatabaseBase[] waitForDatabases;

        private void OnEnable()
        {
            StartCoroutine(WaitForLoad());
        }

        private IEnumerator WaitForLoad ()
        {
            var loaded = false;
            while (!loaded)
            {
                loaded = true;
                foreach (var db in waitForDatabases)
                    loaded &= db.loaded;

                yield return null;
            }

            UIManager.instance.ShowMainMenu();
        }
    }
}

