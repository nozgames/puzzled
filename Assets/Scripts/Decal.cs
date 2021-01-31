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

        public string name => sprite == null ? "None" : sprite.name.Substring(5);

        public Guid guid { get; private set; }

        public Sprite sprite { get; private set; }

        public DecalFlags flags { get; set; }

        public bool flipHorizontal {
            get => (flags & DecalFlags.FlipHorizontal) == DecalFlags.FlipHorizontal;
            set {
                if (value)
                    flags |= DecalFlags.FlipHorizontal;
                else
                    flags &= ~DecalFlags.FlipHorizontal;
            }
        }

        public bool flipVertical {
            get => (flags & DecalFlags.FlipVertical) == DecalFlags.FlipVertical;
            set {
                if (value)
                    flags |= DecalFlags.FlipVertical;
                else
                    flags &= ~DecalFlags.FlipVertical;
            }
        }

        public bool rotate {
            get => (flags & DecalFlags.Rotate) == DecalFlags.Rotate;
            set {
                if (value)
                    flags |= DecalFlags.Rotate;
                else
                    flags &= ~DecalFlags.Rotate;
            }
        }

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
