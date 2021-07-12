using UnityEngine;

namespace Puzzled
{
    public class GameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab = null;
        [SerializeField] private int _cacheSize = 32;

        private GameObject[] _cache = null;
        private int _available = 0;

        private void Awake()
        {
            _cache = new GameObject[_cacheSize];
            _available = 0;
        }

        public GameObject Get (Transform transform)
        {
            GameObject go = null;

            if (_available == 0)
            {
                go = Instantiate(_prefab, transform);

                // Backwards reference to the pool that created it
                go.AddComponent<GameObjectPoolRef>().pool = this;
            }
            else
            {
                _available--;

                go = _cache[_available];
                _cache[_available] = null;
                go.transform.SetParent(transform);
                go.SetActive(true);
            }

            return go;
        }

        public void Put (GameObject go)
        {
            if (go == null)
                return;

            if(_available >= _cacheSize)
            {
                Destroy(go);
                return;
            }

            _cache[_available++] = go;
            go.SetActive(false);

            for(int i=go.transform.childCount-1; i >= 0; i--)
            {
                var poolref = go.transform.GetChild(i).GetComponent<GameObjectPoolRef>();
                if (poolref != null)
                    poolref.gameObject.DestroyPooled();
            }

            go.transform.SetParent(transform);
        }
    }
}
