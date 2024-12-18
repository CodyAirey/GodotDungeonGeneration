using System;
using Godot;

namespace Graphs {
    public class Vertex : IEquatable<Vertex> {
        public Vector2 Position { get; private set; }

        public Vertex() {

        }

        public Vertex(Vector2 position) {
            Position = position;
        }

        public override bool Equals(object obj) {
            if (obj is Vertex v) {
                return Position == v.Position;
            }

            return false;
        }

        public bool Equals(Vertex other) {
            return Position == other.Position;
        }

        public override int GetHashCode() {
            return Position.GetHashCode();
        }
    }

    public class Vertex<T> : Vertex {
        public T Item { get; private set; }

        public Vertex(T item) {
            Item = item;
        }

        public Vertex(Vector2I position, T item) : base(position) {
            Item = item;
        }
    }

    public class Edge : IEquatable<Edge> {
        public Vertex U { get; set; }
        public Vertex V { get; set; }

        public bool IsBad { get; set; }

        public Edge() {

        }

        public Edge(Vertex u, Vertex v) {
            U = u;
            V = v;
        }

        public static bool operator ==(Edge left, Edge right) {
            return (left.U == right.U || left.U == right.V)
                && (left.V == right.U || left.V == right.V);
        }

        public static bool operator !=(Edge left, Edge right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj is Edge e) {
                return this == e;
            }

            return false;
        }

        public bool Equals(Edge e) {
            return this == e;
        }

        public override int GetHashCode() {
            return U.GetHashCode() ^ V.GetHashCode();
        }
    }
}