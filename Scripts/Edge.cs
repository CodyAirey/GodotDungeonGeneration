
using Godot;
using System;
using System.Collections.Generic;

public class Edge<T>
{
    public Vertex<T> U { get; set; }
    public Vertex<T> V { get; set; }
    public bool IsBad { get; set; }

    public Edge(Vertex<T> u, Vertex<T> v)
    {
        U = u;
        V = v;
    }

    public override bool Equals(object obj)
    {
        return obj is Edge<T> e && Equals(e);
    }

    public bool Equals(Edge<T> e)
    {
        if (e is null) return false;

        return (U.Equals(e.U) && V.Equals(e.V)) ||
               (U.Equals(e.V) && V.Equals(e.U));
    }

    public override int GetHashCode()
    {
        int hash1 = U.GetHashCode();
        int hash2 = V.GetHashCode();

        return hash1 < hash2
            ? HashCode.Combine(hash1, hash2)
            : HashCode.Combine(hash2, hash1);
    }

    public static bool operator ==(Edge<T> left, Edge<T> right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;

        return (left.U == right.U || left.U == right.V) &&
               (left.V == right.U || left.V == right.V);
    }

    public static bool operator !=(Edge<T> left, Edge<T> right) => !(left == right);
}