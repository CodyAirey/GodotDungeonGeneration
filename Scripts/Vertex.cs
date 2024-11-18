using Godot;

public class Vertex<T>
{
    public Vector2 Position { get; private set; }
    public T Data { get; private set; }

    public Vertex(Vector2 position, T data = default)
    {
        Position = position;
        Data = data;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vertex<T> other)
        {
            return Position == other.Position;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public static bool operator ==(Vertex<T> left, Vertex<T> right)
    {
        if (left is null || right is null) return ReferenceEquals(left, right);
        return left.Position == right.Position;
    }

    public static bool operator !=(Vertex<T> left, Vertex<T> right)
    {
        return !(left == right);
    }
}
