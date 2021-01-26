using System;
using UnityEngine;

namespace Puzzled
{
    public class EditableAttribute : Attribute
    {
        public bool hidden = false;
        public bool multiline = false;
        public int rangeMin;
        public int rangeMax;
        public int order = 0;
        public string placeholder;
        public string hiddenIfFalse;
        public string hiddenIfTrue;

        public Vector2Int range => new Vector2Int(rangeMin, rangeMax);
    }
}
