﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NoZ;

namespace Puzzled
{
    public class PuzzledActor : Actor
    {
        [SerializeField] private PuzzledActor[] connectedActors;
        [SerializeField] private TileId _id;

        private static Dictionary<Vector2Int, List<PuzzledActor>> cells;

        private Vector2Int cell;

        public PuzzledActor[] connections {
            get => connectedActors;
            set => connectedActors = value;
        }

        public TileId id => _id;

        /// <summary>
        /// Cell the actor is current in
        /// </summary>
        public Vector2Int Cell {
            get => cell;
            set {
                GameManager.Instance.SetActorCell(this, value);
                cell = value;
            }
        }

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(GameManager.Instance != null)
                GameManager.Instance.RemoveActorFromCell(this);
        }

        public void ActivateWire()
        {
            foreach (PuzzledActor connectedActor in connectedActors)
                connectedActor.Send(new ActivateWireEvent());
        }

        public void DeactivateWire()
        {
            foreach (PuzzledActor connectedActor in connectedActors)
                connectedActor.Send(new DeactivateWireEvent());
        }

        public void PulseWire()
        {
            foreach (PuzzledActor connectedActor in connectedActors)
            {
                connectedActor.Send(new ActivateWireEvent());
                connectedActor.Send(new DeactivateWireEvent());
            }
        }
    }
}
