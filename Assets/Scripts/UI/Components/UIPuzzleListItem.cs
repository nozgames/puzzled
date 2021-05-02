﻿using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPuzzleListItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private RawImage _previewImage = null;

        private World.IPuzzleEntry _puzzleEntry;

        public World.IPuzzleEntry puzzleEntry {
            get => _puzzleEntry;
            set {
                _puzzleEntry = value;
                UpdatePuzzleEntry();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdatePuzzleEntry();
        }

        private void UpdatePuzzleEntry()
        {
            if (!isActiveAndEnabled || _puzzleEntry == null)
                return;

            if (_nameText != null)
                _nameText.text = _puzzleEntry.name;

//            _previewImage.texture = WorldManager.CreatePreview(_puzzleEntry);
        }
    }
}
