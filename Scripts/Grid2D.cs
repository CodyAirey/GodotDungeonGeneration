using System.Collections;
using System.Collections.Generic;
using Godot;
public class Grid2D<T>
{
    T[] data;

    public Vector2I Size { get; private set; }
    public Vector2I Offset { get; set; }

    public Grid2D(Vector2I size, Vector2I offset)
    {
        Size = size;
        Offset = offset;

        data = new T[size.X * size.Y];
    }

    public int GetIndex(Vector2I pos)
    {
        return pos.X + (Size.X * pos.Y);
    }

    public bool InBounds(Vector2I pos)
    {
        Rect2I rect = new Rect2I(Offset, Size);
        return rect.HasPoint(pos);
    }

    public T this[int x, int y]
    {
        get
        {
            return this[new Vector2I(x, y)];
        }
        set
        {
            this[new Vector2I(x, y)] = value;
        }
    }

    public T this[Vector2I pos]
    {
        get
        {
            pos += Offset;
            return data[GetIndex(pos)];
        }
        set
        {
            pos += Offset;
            data[GetIndex(pos)] = value;
        }
    }
}