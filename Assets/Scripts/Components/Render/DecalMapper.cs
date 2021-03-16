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

        [Editable (hidden=true, serialized=false)]
        public bool hasSpriteRenderer0 => (_spriteRenderer0 != null);
        [Editable(hidden = true, serialized = false)]
        public bool hasSpriteRenderer1 => (_spriteRenderer1 != null);
        [Editable(hidden = true, serialized = false)]
        public bool hasSpriteRenderer2 => (_spriteRenderer2 != null);
        [Editable(hidden = true, serialized = false)]
        public bool hasSpriteRenderer3 => (_spriteRenderer3 != null);

        [Editable (hiddenIfFalse = "hasSpriteRenderer0")]
        public Decal decal0 { get; private set; }
        [Editable (hiddenIfFalse = "hasSpriteRenderer1")]
        public Decal decal1 { get; private set; }
        [Editable (hiddenIfFalse = "hasSpriteRenderer2")]
        public Decal decal2 { get; private set; }
        [Editable (hiddenIfFalse = "hasSpriteRenderer3")]
        public Decal decal3 { get; private set; }

        [ActorEventHandler]
        protected virtual void OnStart(StartEvent evt)
        {
            if (_spriteRenderer0 != null)
                _spriteRenderer0.sprite = decal0.sprite;
            if (_spriteRenderer1 != null)
                _spriteRenderer1.sprite = decal1.sprite;
            if (_spriteRenderer2 != null)
                _spriteRenderer2.sprite = decal2.sprite;
            if (_spriteRenderer3 != null)
                _spriteRenderer3.sprite = decal3.sprite;
        }
    }
}
