using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor 
    {
        private void EnableMoveTool ()
        {
            moveToolOptions.SetActive(true);

            canvas.onLButtonDown = OnMoveToolLButtonDown;
            canvas.onLButtonUp = OnMoveToolLButtonUp;
        }

        private void DisableMoveTool ()
        {
            moveToolOptions.SetActive(false);
        }

        private void OnMoveToolLButtonDown (Vector2 position)
        {
            Debug.Log(position);
        }

        private void OnMoveToolLButtonUp (Vector2 position)
        {
            Debug.Log(position);
        }
    }
}
