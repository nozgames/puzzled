using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Background", menuName = "Puzzled/Background")]
    public class Background : ScriptableObject
    {
        /// <summary>
        /// Unique guid of the background
        /// </summary>
        public System.Guid guid { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// Background color
        /// </summary>
        public Color gridColor = Color.black;
    }
}
