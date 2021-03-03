using NoZ;
using UnityEngine;

namespace Puzzled
{
    class NumberSpinner : UsableSpinner
    {
        [Header("Visuals")]
        [SerializeField] private Sprite[] _numberSprites = null;

        override protected Sprite[] sprites => _numberSprites;
    }
}
