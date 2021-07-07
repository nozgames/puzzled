using System;
using UnityEngine;

namespace Puzzled
{
    public class WallRotation : TileComponent
    {
        [SerializeField] private Transform _target = null;
        [SerializeField] private Transform _target2 = null;

        [SerializeField] private int _rotationCount = 4;

        private int _rotation = 0;

        [Editable]
        private int rotation {
            get => _rotation;
            set {
                _rotation = value % _rotationCount;

                switch (_rotation)
                {
                    case 0:
                        _target.transform.localScale = new Vector3(1, 1, 1);
                        break;

                    case 1:
                        _target.transform.localScale = new Vector3(-1, 1, 1);
                        break;

                    case 2:
                        _target.transform.localScale = new Vector3(1, 1, -1);
                        break;

                    case 3:
                        _target.transform.localScale = new Vector3(-1, 1, -1);
                        break;
                }

                if (_target2 != null)
                    _target2.transform.localScale = _target.transform.localScale;
            }
        }
    }
}
