using System;
using System.Collections.Generic;
using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class TileComponent : ActorComponent
    {
        private int _processedTickFrame = 0;

        /// <summary>
        /// Tile the component is attached to
        /// </summary>
        public Tile tile => (Tile)base.actor;

        /// <summary>
        /// Puzzle the parent tile is attached to
        /// </summary>
        public Puzzle puzzle => tile == null ? null : tile.puzzle;

        /// <summary>
        /// True if the component is being edited
        /// </summary>
        public bool isEditing => puzzle.isEditing;

        /// <summary>
        /// True if the component is in the process of loading
        /// </summary>
        public bool isLoading => puzzle == null || puzzle.isLoading;

        /// <summary>
        /// True if the current frame is a tick
        /// </summary>
        public bool isTickFrame => tile == null ? false : tile.isTickFrame;

        /// <summary>
        /// True if the current tick frame has been processed
        /// </summary>
        public bool isTickFrameProcessed {
            get => _processedTickFrame == tile.tickFrame;
            set {
                if (value)
                    _processedTickFrame = tile.tickFrame;
                else
                    _processedTickFrame = 0;
            }
        }

        /// <summary>
        /// current tick frame index
        /// </summary>
        public int tickFrame => tile == null ? 0 : tile.tickFrame;


        public bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All) =>
            puzzle.grid.SendToCell(evt, cell, routing);

        protected void BeginBusy() => GameManager.busy++;

        protected void EndBusy() => GameManager.busy--;

        [ActorEventHandler(priority = int.MinValue)]
        private void OnAwake(AwakeEvent evt)
        {
            // Automatically create all of the ports for ourself
            var properties = TileDatabase.GetProperties(tile);
            var type = GetType();
            foreach (var property in properties)
            {
                if (property.info.DeclaringType != type)
                    continue;

                if (property.type != TilePropertyType.Port)
                    continue;

                if (null != property.GetValue<Port>(tile))
                    continue;

                property.SetValue(tile, new Port(tile, property));
            }
        }

        protected void PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (isLoading || isEditing || clip == null || volume <= 0.0f)
                return;

            AudioManager.Instance.Play(clip, volume, pitch);
        }
    }
}
