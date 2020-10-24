using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Theme", menuName = "Puzzled/Theme")]
    public class Theme : ScriptableObject
    {
        public GameObject wall = null;
        public GameObject floor = null;
    }
}
