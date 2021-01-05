using System;
using NoZ;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIKeypadPopup : UIPopup
    {
        [SerializeField] private Transform _solution = null;
        [SerializeField] private GridLayoutGroup _buttons = null;
        [SerializeField] private UIKeypadPopupItem _buttonPrefab = null;
        [SerializeField] private UIKeypadPopupItem _solutionPrefab = null;
        [SerializeField] private GameObject _errorMessage = null;
        [SerializeField] private GameObject _successMessage = null;


        private int _cursor = 0;
        private Action _unlockCallback;
        private Decal[] _solutionDecals;

        public void Open(Decal[] buttons, Decal[] solution, int columnCount, Action unlockCallback)
        {
            _errorMessage.SetActive(false);
            _successMessage.SetActive(false);
            _unlockCallback = unlockCallback;
            _solutionDecals = solution;

            _buttons.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _buttons.constraintCount = Math.Max(1, columnCount);

            _buttons.transform.DetachAndDestroyChildren();
            _solution.transform.DetachAndDestroyChildren();

            for(int i=0; i<solution.Length; i++)
                Instantiate(_solutionPrefab.gameObject, _solution);

            foreach (var decal in buttons)
            {
                var item = Instantiate(_buttonPrefab.gameObject, _buttons.transform).GetComponent<UIKeypadPopupItem>();
                item.decal = decal;
                item.button.onClick.AddListener(() => {
                    AddToSolution(decal);
                });
            }
        }

        private UIKeypadPopupItem GetSolutionItem(int index) =>
            _solution.transform.GetChild(index).GetComponent<UIKeypadPopupItem>();

        private void AddToSolution(Decal decal)
        {
            if (_cursor >= _solutionDecals.Length)
                return;

            GetSolutionItem(_cursor).decal = decal;
            _cursor++;

            if(_cursor == _solutionDecals.Length)
            {
                for (int i = 0; i < _solutionDecals.Length; i++)
                {
                    var item = GetSolutionItem(i);
                    if (item.decal != _solutionDecals[i] || item.decal.flags != _solutionDecals[i].flags)
                    {
                        Error();
                        return;
                    }
                }

                _successMessage.SetActive(true);
                Tween.Wait(1.0f).OnStop(() => {
                    _unlockCallback?.Invoke();
                    Close();
                }).Start(gameObject);
            }
        }

        private void Error()
        {
            _errorMessage.SetActive(true);

            Tween.Wait(2.0f).AutoDeactivate().OnStop(Clear).Start(_errorMessage);
        }

        public void Clear()
        {
            for(int i=0; i<_solution.childCount; i++)
                GetSolutionItem(i).decal = Decal.none;

            _cursor = 0;
        }
    }
}
