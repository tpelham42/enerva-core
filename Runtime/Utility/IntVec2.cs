using System.Numerics;
using System.Xml.Serialization;

public class IntVec2 {
    [XmlAttribute("x")]
    public int X { get; set; }

    [XmlAttribute("y")]
    public int Y { get; set; }

    public IntVec2() {
        X = 0;
        Y = 0;
    }

    public IntVec2(int x, int y) {
        X = x;
        Y = y;
    }

    public UnityEngine.Vector2 AsVector2 {
        get { return new UnityEngine.Vector2(X, Y); }
    }

    public bool Equals(IntVec2 other) {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj) {
        return obj is IntVec2 other && Equals(other);
    }

    public override int GetHashCode() {
        unchecked {
            // simple, fast hash appropriate for value pair
            return (X * 397) ^ Y;
        }
    }

    public static bool operator ==(IntVec2 left, IntVec2 right) {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null)) return false;
        return left.Equals(right);
    }

    public static bool operator !=(IntVec2 left, IntVec2 right) {
        return !(left == right);
    }

    public override string ToString() {
        return $"({X}, {Y})";
    }


}