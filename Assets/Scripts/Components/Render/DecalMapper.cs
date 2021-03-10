using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalMapper : TileComponent
    {
        [SerializeField] private SpriteRenderer _spriteRenderer0 = null;
        [SerializeField] private SpriteRenderer _spriteRenderer1 = null;
        [SerializeField] private SpriteRenderer _spriteRenderer2 = null;
        [SerializeField] private SpriteRenderer _spriteRenderer3 = null;

        private bool hasSpriteRenderer0 => (_spriteRenderer0 != null);
        private bool hasSpriteRenderer1 => (_spriteRenderer1 != null);
        private bool hasSpriteRenderer2 => (_spriteRenderer2 != null);
        private bool hasSpriteRenderer3 => (_spriteRenderer3 != null);

        [Editable (hiddenIfFalse = "hasSpriteRenderer0")]
        public Decal decal0 { get; private set; }
        [Editable (hiddenIfFalse = "hasSpriteRenderer1")]
        public Decal decal1 { get; private set; }
        [Editable (hiddenIfFalse = "hasSpriteRenderer2")]
        public Decal decal2 { get; private set; }
        [Editable (hiddenIfFalse = "hasSpriteRenderer3")]
        public Decal decal3 { get; private set; }
    }
}
