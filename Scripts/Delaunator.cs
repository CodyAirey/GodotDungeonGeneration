using Godot;
using System;
using System.Collections.Generic;
using Graphs;

public class Delaunay2D<T>
{
    public class Triangle : IEquatable<Triangle>
    {
        public Vertex A { get; set; }
        public Vertex B { get; set; }
        public Vertex C { get; set; }
        public bool IsBad { get; set; }

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool ContainsVertex(Vector2 v)
        {
            return A.Position.DistanceTo(v) < 0.01f ||
                   B.Position.DistanceTo(v) < 0.01f ||
                   C.Position.DistanceTo(v) < 0.01f;
        }

        public bool CircumCircleContains(Vector2 v)
        {
            Vector2 a = A.Position;
            Vector2 b = B.Position;
            Vector2 c = C.Position;

            float ab = a.LengthSquared();
            float cd = b.LengthSquared();
            float ef = c.LengthSquared();

            float circumX = (ab * (c.Y - b.Y) + cd * (a.Y - c.Y) + ef * (b.Y - a.Y)) /
                            (a.X * (c.Y - b.Y) + b.X * (a.Y - c.Y) + c.X * (b.Y - a.Y));
            float circumY = (ab * (c.X - b.X) + cd * (a.X - c.X) + ef * (b.X - a.X)) /
                            (a.Y * (c.X - b.X) + b.Y * (a.X - c.X) + c.Y * (b.X - a.X));

            Vector2 circum = new Vector2(circumX / 2, circumY / 2);
            float circumRadius = a.DistanceSquaredTo(circum);
            float dist = v.DistanceSquaredTo(circum);
            return dist <= circumRadius;
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle t)
            {
                return this == t;
            }
            return false;
        }

        public bool Equals(Triangle t)
        {
            return this == t;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
        }

        public static bool operator ==(Triangle left, Triangle right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;

            return (left.A == right.A || left.A == right.B || left.A == right.C) &&
                   (left.B == right.A || left.B == right.B || left.B == right.C) &&
                   (left.C == right.A || left.C == right.B || left.C == right.C);
        }

        public static bool operator !=(Triangle left, Triangle right) => !(left == right);
    }

    public List<Vertex> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }
    public List<Triangle> Triangles { get; private set; }

    private Delaunay2D()
    {
        Edges = new List<Edge>();
        Triangles = new List<Triangle>();
    }

    public static Delaunay2D<T> Triangulate(List<Vertex> vertices)
    {
        Delaunay2D<T> delaunay = new Delaunay2D<T>
        {
            Vertices = new List<Vertex>(vertices)
        };
        delaunay.TriangulateInternal();
        return delaunay;
    }

    private void TriangulateInternal()
    {
        float minX = Vertices[0].Position.X;
        float minY = Vertices[0].Position.Y;
        float maxX = minX;
        float maxY = minY;

        foreach (var vertex in Vertices)
        {
            if (vertex.Position.X < minX) minX = vertex.Position.X;
            if (vertex.Position.X > maxX) maxX = vertex.Position.X;
            if (vertex.Position.Y < minY) minY = vertex.Position.Y;
            if (vertex.Position.Y > maxY) maxY = vertex.Position.Y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy) * 2;

        Vertex p1 = new Vertex(new Vector2(minX - 1, minY - 1));
        Vertex p2 = new Vertex(new Vector2(minX - 1, maxY + deltaMax));
        Vertex p3 = new Vertex(new Vector2(maxX + deltaMax, minY - 1));

        Triangles.Add(new Triangle(p1, p2, p3));

        foreach (var vertex in Vertices)
        {
            List<Edge> polygon = new List<Edge>();

            foreach (var t in Triangles)
            {
                if (t.CircumCircleContains(vertex.Position))
                {
                    t.IsBad = true;
                    polygon.Add(new Edge(t.A, t.B));
                    polygon.Add(new Edge(t.B, t.C));
                    polygon.Add(new Edge(t.C, t.A));
                }
            }

            Triangles.RemoveAll(t => t.IsBad);

            for (int i = 0; i < polygon.Count; i++)
            {
                for (int j = i + 1; j < polygon.Count; j++)
                {
                    if (polygon[i].Equals(polygon[j]))
                    {
                        polygon[i].IsBad = true;
                        polygon[j].IsBad = true;
                    }
                }
            }

            polygon.RemoveAll(e => e.IsBad);

            foreach (var edge in polygon)
            {
                Triangles.Add(new Triangle(edge.U, edge.V, vertex));
            }
        }

        Triangles.RemoveAll(t => t.ContainsVertex(p1.Position) || t.ContainsVertex(p2.Position) || t.ContainsVertex(p3.Position));

        HashSet<Edge> edgeSet = new HashSet<Edge>();

        foreach (var t in Triangles)
        {
            var ab = new Edge(t.A, t.B);
            var bc = new Edge(t.B, t.C);
            var ca = new Edge(t.C, t.A);

            if (edgeSet.Add(ab))
            {
                Edges.Add(ab);
            }

            if (edgeSet.Add(bc))
            {
                Edges.Add(bc);
            }

            if (edgeSet.Add(ca))
            {
                Edges.Add(ca);
            }
        }
    }
}
