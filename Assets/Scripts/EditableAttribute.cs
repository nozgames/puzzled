using System;
using UnityEngine;

namespace Puzzled
{
    public class EditableAttribute : Attribute
    {
        public bool hidden = false;
        public bool multiline = false;
        public Vector2Int range;
        public int order = 0;
    }
}
