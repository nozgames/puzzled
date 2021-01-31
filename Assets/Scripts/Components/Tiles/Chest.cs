using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Chest : TileComponent
    {
        [Header("General")]
        [SerializeField] private Tile keyItem = null;
        [SerializeField] private AudioClip _unlockSound = null;

        [Editable]
        public System.Guid prizeItem { get; private set; }

        [Header("Visuals")]
        [SerializeField] private Animator _animator = null;

        private bool _locked = false;
        private bool _used = false;
       
        private void Start()
        {
            _locked = keyItem != null;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            if (_used)
                return;

            // Always retport we were used, even if the use fails
            evt.IsHandled = true;

            if(_locked)
            {
                // Check to see if the user has the item to unlock the chest
                _locked = !evt.user.Send(new QueryHasItemEvent(keyItem.guid));
                if (_locked)
                    return;
            }

            _used = true;

            PlaySound(_unlockSound);

            _animator.SetTrigger("Open");

            GameManager.busy++;

            Tween.Wait(0.5f)
                .OnStop(() => {
                    var cell = tile.cell;

                    // Destroy ourself first or the new item cannot spawn 
                    //tile.Destroy();

                    // Spawn the prize at the same spot
                    puzzle.InstantiateTile(DatabaseManager.GetTile(prizeItem), cell);

                    GameManager.busy--;
                })
                .Start(gameObject);
        }
    }
}
