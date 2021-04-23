using System;
using UnityEngine;

namespace Puzzled
{
    [Flags]
    public enum DecalFlags
    {
        None,
        Imported = 1,
        Flip = 2,
        AutoColor = 4
    }

    public struct Decal
    {
        public static readonly Decal none = new Decal(Guid.Empty, null) { scale = 1.0f, color = Color.white, flags = DecalFlags.AutoColor };

        public string name => sprite == null ? "None" : sprite.name.Substring(5);

        public Guid guid { get; private set; }

        public Sprite sprite { get; private set; }

        public DecalFlags flags { get; set; }

        public float rotation { get; set; }

        public Vector2 offset { get; set; }

        public float scale { get; set; }

        public Color color { get; set; }

        public bool isAutoColor { 
            get => (flags & DecalFlags.AutoColor) == DecalFlags.AutoColor;
            set {
                if (value)
                    flags |= DecalFlags.AutoColor;
                else
                    flags &= ~DecalFlags.AutoColor;
            }
        }

        public bool isImported {
            get => (flags & DecalFlags.Imported) == DecalFlags.Imported;
            set {
                if (value)
                    flags |= DecalFlags.Imported;
                else
                    flags &= ~DecalFlags.Imported;
            }
        }

        public bool isFlipped {
            get => (flags & DecalFlags.Flip) == DecalFlags.Flip;
            set {
                if (value)
                    flags |= DecalFlags.Flip;
                else
                    flags &= ~DecalFlags.Flip;
            }
        }

        public static bool operator ==(Decal lhs, Decal rhs) => lhs.guid == rhs.guid;

        public static bool operator !=(Decal lhs, Decal rhs) => lhs.guid != rhs.guid;

        public bool Equals(Decal other, bool deep = false)
        {
            if (guid != other.guid)
                return false;

            if (!deep)
                return true;

            return
                color == other.color &&
                rotation == other.rotation &&
                scale == other.scale &&
                offset == other.offset &&
                flags == other.flags;
        }

        public override bool Equals(object other) => other.GetType() == typeof(Decal) && Equals((Decal)other);

        public override int GetHashCode() => guid.GetHashCode();

        public override string ToString() => $"{guid.ToString()}";        

        public void SetTexture (Decal decal)
        {
            sprite = decal.sprite;
            guid = decal.guid;
        }

        public Decal(Guid guid, Sprite sprite)
        {
            this.guid = guid;
            this.sprite = sprite;
            offset = Vector2.zero;
            scale = 1.0f;
            rotation = 0.0f;
            flags = DecalFlags.AutoColor;
            color = Color.white;
        }        
    }
}
