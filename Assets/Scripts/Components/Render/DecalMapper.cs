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

//         private bool hasSpriteRenderer0 => (_spriteRenderer0 != null);
//         private bool hasSpriteRenderer1 => (_spriteRenderer1 != null);
//         private bool hasSpriteRenderer2 => (_spriteRenderer2 != null);
//         private bool hasSpriteRenderer3 => (_spriteRenderer3 != null);

        [Editable]
        public Decal[] decals { get; private set; }
        // FIXME: the hidden if false logic wasn't working with decals
        //         [Editable(hiddenIfFalse = "hasSpriteRenderer0")]
        //         public Decal decal0 { get; private set; }
        //         [Editable (hiddenIfFalse = "hasSpriteRenderer1")]
        //         public Decal decal1 { get; private set; }
        //         [Editable (hiddenIfFalse = "hasSpriteRenderer2")]
        //         public Decal decal2 { get; private set; }
        //         [Editable (hiddenIfFalse = "hasSpriteRenderer3")]
        //         public Decal decal3 { get; private set; }

        [ActorEventHandler]
        protected virtual void OnStart(StartEvent evt)
        {
            if (decals == null)
                return;

            // FIXME: how can we do this without having an array of decals?
            if (_spriteRenderer0 != null)
            {
                if (decals.Length > 0)
                    _spriteRenderer0.sprite = decals[0].sprite;
                else
                    _spriteRenderer0.sprite = null;
            }

            if (_spriteRenderer1 != null)
            {
                if (decals.Length > 1)
                    _spriteRenderer1.sprite = decals[1].sprite;
                else
                    _spriteRenderer1.sprite = null;
            }

            if (_spriteRenderer2 != null)
            {
                if (decals.Length > 2)
                    _spriteRenderer2.sprite = decals[2].sprite;
                else
                    _spriteRenderer2.sprite = null;
            }

            if (_spriteRenderer3 != null)
            {
                if (decals.Length > 3)
                    _spriteRenderer3.sprite = decals[3].sprite;
                else
                    _spriteRenderer3.sprite = null;
            }
        }
    }
}
