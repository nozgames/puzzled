using System;
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

        private static Dictionary<Vector2Int, List<PuzzledActor>> cells;

        private Vector2Int cell;

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

        public void TriggerActivateWire()
        {
            foreach (PuzzledActor connectedActor in connectedActors)
                connectedActor.Send(new ActivateWireEvent());
        }

        public void TriggerDeactivateWire()
        {
            foreach (PuzzledActor connectedActor in connectedActors)
                connectedActor.Send(new DeactivateWireEvent());
        }

        public void TriggerPulseWire()
        {
            foreach (PuzzledActor connectedActor in connectedActors)
            {
                connectedActor.Send(new ActivateWireEvent());
                connectedActor.Send(new DeactivateWireEvent());
            }
        }
    }
}
