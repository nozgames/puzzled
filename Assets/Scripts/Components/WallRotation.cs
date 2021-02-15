using System;
using UnityEngine;

namespace Puzzled
{
    public class WallRotation : TileComponent
    {
        [SerializeField] private Transform _target = null;

        private int _rotation = 0;

        [Editable]
        private int rotation {
            get => _rotation;
            set {
                _rotation = value % 4;

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
            }
        }
    }
}
