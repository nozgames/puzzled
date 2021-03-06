﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPlayWorldScreen : UIScreen
    {
        [SerializeField] private ScrollRect _scrollRect = null;

        [SerializeField] private UIPuzzleList _puzzleList = null;
        [SerializeField] private UIPuzzleListItem _puzzleListItemPrefab = null;
        [SerializeField] private TMPro.TextMeshProUGUI _worldNameText = null;

        override public bool showConfirmButton => true;
        override public string confirmButtonText => "Play";
        override public bool showOptionButton => isDebugging;
        override public string optionButtonText => "Cheat Puzzle";

        override public bool showCancelButton => true;

        private World _world;
        public World world
        {
            get => _world;
            set => _world = value;
        }

        public bool isDebugging;

        private void OnEnable()
        {
            if (isDebugging)
                SaveManager.BeginSandbox();

            UpdateWorld();

            _puzzleList.Select();
            _puzzleList.SelectItem(0);
        }

        private void OnDisable()
        {
        }

        private void UpdateWorld ()
        {
            if (_world == null || !isActiveAndEnabled)
                return;

            _worldNameText.text = _world.displayName;

            _puzzleList.transform.DetachAndDestroyChildren();

            foreach (var entry in _world.puzzles)
            {
                var locked = entry.isLocked;
                if (locked && entry.hideWhenLocked)
                    continue;

                var item = Instantiate(_puzzleListItemPrefab.gameObject, _puzzleList.transform).GetComponent<UIPuzzleListItem>();
                item.puzzleEntry = entry;

                if (!locked)
                    item.onDoubleClick.AddListener(() => {
                        PlayPuzzle(item);
                    });
                else
                    item.interactable = false;
            }
        }

        private void Select(World.IPuzzleEntry puzzleEntry)
        {
            for(int i=0; i<_puzzleList.itemCount; i++)
            {
                var puzzleItem = _puzzleList.GetItem(i) as UIPuzzleListItem;
                if(puzzleItem.puzzleEntry == puzzleEntry)
                {
                    _puzzleList.SelectItem(i);
                    _scrollRect.ScrollTo(puzzleItem .GetComponent<RectTransform>());
                }
            }
        }

        private void ExitScreen()
        {
            if (isDebugging)
            {
                SaveManager.EndSandbox();
                UIManager.ReturnToEditWorldScreen();
            } 
            else
                UIManager.ShowPlayScreen();
        }

        private void PlayPuzzle(UIPuzzleListItem item)
        {
            // Load the puzzle and play
            GameManager.LoadPuzzle(item.puzzleEntry);
            GameManager.Play();

            var transitionIn = item.puzzleEntry.transitionIn;
            if (transitionIn != null)
                UIManager.ShowWorldTransitionScreen(transitionIn, () => UIManager.HideMenu());
            else
                UIManager.HideMenu();

        }

        public override void HandleCancelInput()
        {
            ExitScreen();
        }

        public override void HandleConfirmInput()
        {
            UIPuzzleListItem item = _puzzleList.GetItem(_puzzleList.selected) as UIPuzzleListItem;
            PlayPuzzle(item);
        }

        public override void HandleOptionInput()
        {
            if (!isDebugging)
                return;

            int savedSelection = _puzzleList.selected;
            UIPuzzleListItem item = _puzzleList.GetItem(savedSelection) as UIPuzzleListItem;
            item.puzzleEntry.MarkCompleted();
            UpdateWorld();

            int newSelection = Mathf.Min(savedSelection + 1, _puzzleList.itemCount - 1);
            _puzzleList.SelectItem(newSelection);
        }
    }
}
