using System;
using UnityEngine;

namespace Puzzled.Editor
{
    public abstract class UIPaletteItem : MonoBehaviour
    {
        public abstract object value { get; }
    }
}
