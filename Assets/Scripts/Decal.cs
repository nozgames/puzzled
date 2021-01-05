using System;
using UnityEngine;

namespace Puzzled
{
    [Flags]
    public enum DecalFlags
    {
        None,
        FlipHorizontal = 1, 
        FlipVertical = 2,
        Rotate = 4
    }

    public struct Decal
    {
        public static readonly Decal none = new Decal(Guid.Empty, null);

        public Guid guid { get; private set; }

        public Sprite sprite { get; private set; }

        public DecalFlags flags { get; set; }

        public bool flipHorizontal => (flags & DecalFlags.FlipHorizontal) == DecalFlags.FlipHorizontal;

        public bool flipVertical => (flags & DecalFlags.FlipVertical) == DecalFlags.FlipVertical;

        public bool rotate => (flags & DecalFlags.Rotate) == DecalFlags.Rotate;

        public static bool operator ==(Decal lhs, Decal rhs) => lhs.guid == rhs.guid;

        public static bool operator !=(Decal lhs, Decal rhs) => lhs.guid != rhs.guid;

        public bool Equals(Decal other) => guid == other.guid;

        public override bool Equals(object other) => other.GetType() == typeof(Decal) && Equals((Decal)other);

        public override int GetHashCode() => guid.GetHashCode();

        public override string ToString() => $"{guid.ToString()}";

        public Decal(Guid guid, Sprite sprite)
        {
            this.guid = guid;
            this.sprite = sprite;
            flags = DecalFlags.None;
        }
    }
}
