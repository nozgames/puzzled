using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Tile : Actor
    {
        [SerializeField] private Tile[] _connectedTiles;
        [SerializeField] private TileInfo _info = null;

        private Vector2Int _cell;

        public Tile[] connections {
            get => _connectedTiles;
            set => _connectedTiles = value;
        }

        public TileInfo info => _info;

        /// <summary>
        /// Cell the actor is current in
        /// </summary>
        public Vector2Int cell {
            get => _cell;
            set {
                GameManager.Instance.SetTileCell(this, value);
                _cell = value;
            }
        }

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(GameManager.Instance != null)
                GameManager.Instance.RemoveTileFromCell(this);
        }

        public void ActivateWire()
        {
            foreach (Tile connectedActor in _connectedTiles)
                connectedActor.Send(new ActivateWireEvent());
        }

        public void DeactivateWire()
        {
            foreach (Tile connectedActor in _connectedTiles)
                connectedActor.Send(new DeactivateWireEvent());
        }

        public void PulseWire()
        {
            foreach (Tile connectedActor in _connectedTiles)
            {
                connectedActor.Send(new ActivateWireEvent());
                connectedActor.Send(new DeactivateWireEvent());
            }
        }
    }
}
