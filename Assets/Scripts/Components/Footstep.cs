using UnityEngine;

namespace Puzzled.Assets.Scripts.Components
{
    class Footstep : MonoBehaviour
    {
        [SerializeField] private AudioSource _source = null;
        [SerializeField] private AudioClip _defaultClip = null;
        [SerializeField] private float _pitchMin = 1.0f;
        [SerializeField] private float _pitchMax = 1.0f;
        [SerializeField] private float _volume = 1.0f;

        public void Play (AudioClip clip)
        {
            var floorTile = GameManager.puzzle.grid.CellToTile(GameManager.puzzle.player.tile.cell, TileLayer.Floor);
            var floor = floorTile != null ? floorTile.GetComponent<Floor>() : null;

            var footstepClip = _defaultClip;
            if(floor != null && floor.footstepClip != null)
            {
                footstepClip = floor.footstepClip;
            }

            _source.pitch = Random.Range(_pitchMin, _pitchMax);
            _source.clip = footstepClip;
            _source.volume = _volume;
            _source.Play();
        }
    }
}
