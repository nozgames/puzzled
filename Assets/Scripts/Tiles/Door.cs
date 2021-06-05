﻿using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Door : TileComponent
    {
        private bool _open = false;
        private int _animationOpen;
        private int _animationClosed;

        [SerializeField] private GameObject _gizmos = null;

        [SerializeField] private Tile keyItem = null;
        [SerializeField] private AudioClip _unlockSound = null;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [SerializeField] private Animator _animator = null;
        [SerializeField] private AudioClip _openSound = null;
        [SerializeField] private AudioClip _closeSound = null;

        [Editable]
        public bool isOpen {
            get => _open;
            set {
                if (_open == value)
                    return;

                _open = value;

                if(_animator != null)
                {
                    _animator.enabled = true;
                    _animator.Play(value ? _animationOpen : _animationClosed, 0, (isEditing||isLoading||isStarting) ? 1.0f : 0.0f);
                }

                if (value)
                    PlaySound(_openSound);
                else
                    PlaySound(_closeSound);
            }
        }

        private bool requiresKey => keyItem != null;

        private void Awake()
        {
            if(null != _animator)
            {
                _animationOpen = Animator.StringToHash("Open");
                _animationClosed = Animator.StringToHash("Closed");
            }
        }

        [ActorEventHandler(priority=10)]
        private void OnQueryMoveEvent(QueryMoveEvent evt)
        {
            evt.IsHandled = true;
            evt.result = _open;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            // When the door is open you should be able to use things through it 
            if (_open)
                return;

            // Always report we were used, even if the use fails
            evt.IsHandled = true;

            if (!requiresKey)
                return;

            // Check to see if the user has the item to unlock the chest
            bool shouldOpen = evt.user.Send(new QueryHasItemEvent(keyItem.guid));
            if (!shouldOpen)
                return;

            PlaySound(_unlockSound);

            isOpen = true;
        }

        [ActorEventHandler]
        private void OnWirePower (WirePowerChangedEvent evt)
        {
            // Do not let wires open/close locked doors
            if (requiresKey)
                return;

            if (powerInPort.hasPower && !isOpen)
                isOpen = true;
            else if (!powerInPort.hasPower && isOpen)
                isOpen = false;
        }

        [ActorEventHandler]
        private void ShowGizmos(ShowGizmosEvent evt)
        {
            if (null != _gizmos)
                _gizmos.gameObject.SetActive(evt.show);
        }
    }
}
