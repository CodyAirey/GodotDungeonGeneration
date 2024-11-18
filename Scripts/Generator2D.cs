using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using Graphs;

public partial class Generator2D : Node
{
	private enum CellType
	{
		None,
		Room,
		Hallway
	}

	private class Room
	{
		public Rect2I Bounds;

		public Room(Vector2I location, Vector2I size)
		{
			Bounds = new Rect2I(location, size);
		}

		public static bool Intersect(Room a, Room b)
		{
			return a.Bounds.Intersects(b.Bounds);
		}
	}



	[Export] private Vector2I Size = new Vector2I(72, 41);
	[Export(PropertyHint.Range, "1,50,1")] private int RoomCount = 10;
	[Export] private Vector2I RoomMinSize = new Vector2I(10, 10);
	[Export] private Vector2I RoomMaxSize = new Vector2I(30, 30);
	[Export] private PackedScene SquarePrefab;
	[Export] private Color RoomColor = new Color(1, 0, 0);
	[Export] private Color HallwayColor = new Color(0, 0, 1);
	[Export] private int TileSize = 16;

	[Export] private double ExtraHallwayChance = .2;

	private Random _random;
	private Godot.Collections.Dictionary<Vector2I, CellType> _grid;
	private List<Room> _rooms;
	private HashSet<Prim.Edge> selectedEdges;

	private TileMapLayer floorLayer;
	private TileMapLayer wallLayer;
	Delaunay2D<Room> delaunay;

	public override void _Ready()
	{
		convertToTilespace();
		setupNodes();
		adjustCameraScope();
		Generate();
	}

	private void setupNodes()
	{
		floorLayer = GetNode<TileMapLayer>("Floors");
		wallLayer = GetNode<TileMapLayer>("Walls");
	}

	private void convertToTilespace()
	{
		RoomMinSize = RoomMinSize * TileSize;
		RoomMaxSize = RoomMaxSize * TileSize;
		Size = Size * TileSize;
	}

	private void adjustCameraScope()
	{
		Camera2D camera = GetNode<Camera2D>("Camera2D");

		camera.Offset = new Vector2(Size.X / 2, Size.Y / 2);
		Vector2 windowSize = GetWindow().Size;
		Vector2 zoom = new Vector2(
			windowSize.X / Size.X,
			windowSize.Y / Size.Y
		);

		camera.Zoom = zoom;
	}

	private void Generate()
	{
		_random = new Random();
		_grid = new Godot.Collections.Dictionary<Vector2I, CellType>();
		_rooms = new List<Room>();

		PlaceRooms();
		Triangulate();
		CreateHallways();
		PathfindHallways();
	}

	private void PlaceRooms()
	{
		for (int i = 0; i < RoomCount; i++)
		{
			var location = new Vector2I(
				_random.Next(0, Size.X - RoomMinSize.X - 1),
				_random.Next(0, Size.Y - RoomMinSize.Y - 1)
			);

			var roomSize = new Vector2I(
				_random.Next(RoomMinSize.X, RoomMaxSize.X + 1),
				_random.Next(RoomMinSize.Y, RoomMaxSize.Y + 1)
			);

			bool add = true;
			var newRoom = new Room(location, roomSize);
			var buffer = new Room(location - Vector2I.One, roomSize + Vector2I.One * 2);

			foreach (var room in _rooms)
			{
				if (Room.Intersect(room, buffer))
				{
					add = false;
					break;
				}
			}

			if (newRoom.Bounds.Position.X < 0 || newRoom.Bounds.End.X >= Size.X ||
				newRoom.Bounds.Position.Y < 0 || newRoom.Bounds.End.Y >= Size.Y)
			{
				add = false;
			}

			if (add)
			{
				_rooms.Add(newRoom);
				PlaceRoom(newRoom.Bounds.Position, newRoom.Bounds.Size);

				for (int x = newRoom.Bounds.Position.X; x < newRoom.Bounds.Position.X + newRoom.Bounds.Size.X; x++)
				{
					for (int y = newRoom.Bounds.Position.Y; y < newRoom.Bounds.Position.Y + newRoom.Bounds.Size.Y; y++)
					{
						var pos = new Vector2I(x, y);
						_grid[pos] = CellType.Room;
					}
				}
			}
		}
	}


	void Triangulate()
	{
		List<Vertex> vertices = new List<Vertex>();

		// Convert room centers into vertices
		foreach (var room in _rooms)
		{
			Vector2I roomCenter = (Vector2I)((Vector2)room.Bounds.Position + (Vector2)room.Bounds.Size / 2);
			vertices.Add(new Vertex<Room>(roomCenter, room));
		}

		delaunay = Delaunay2D<Room>.Triangulate(vertices);

		GD.Print($"Generated {delaunay.Triangles.Count} triangles and {delaunay.Edges.Count} edges.");

		// // Draw for gamers
		// Node2D linesParent = new Node2D();
		// AddChild(linesParent);
		// Color drawColor = new Color(0,0,1);

		// foreach (var triangle in delaunay.Triangles)
		// {
		// 	DrawLine(linesParent, triangle.A.Position, triangle.B.Position, drawColor);
		// 	DrawLine(linesParent, triangle.B.Position, triangle.C.Position, drawColor);
		// 	DrawLine(linesParent, triangle.C.Position, triangle.A.Position, drawColor);
		// }
	}

	private void DrawLine(Node2D parent, Vector2 start, Vector2 end, Color drawColor)
	{
		Line2D line = new Line2D
		{
			Points = new Vector2[] { start, end },
			DefaultColor = drawColor,
			Width = 3
		};
		parent.AddChild(line);
	}

private void CreateHallways()
{
    // Step 1: Create a list of edges compatible with Prim's algorithm
    List<Prim.Edge> edges = new List<Prim.Edge>();

    foreach (var edge in delaunay.Edges)
    {
        edges.Add(new Prim.Edge(edge.U, edge.V));
    }
	
    // Step 2: Generate the Minimum Spanning Tree (MST)
    List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

    // Select edges for hallways
    selectedEdges = new HashSet<Prim.Edge>(mst);

    // Create a set of remaining edges by subtracting the MST edges
    var remainingEdges = new HashSet<Prim.Edge>(edges);
    remainingEdges.ExceptWith(selectedEdges);

    // Randomly add some of the remaining edges
    foreach (var edge in remainingEdges)
    {
        if (_random.NextDouble() < ExtraHallwayChance)
        {
            selectedEdges.Add(edge);
        }
    }

	Node2D linesParent = new Node2D();
	AddChild(linesParent);
	Color drawColor = new Color(0,1,0);

	foreach(var edge in selectedEdges){
		DrawLine(linesParent, edge.U.Position, edge.V.Position, drawColor);
	}
}

	private void PathfindHallways()
	{
		// Use A* or other pathfinding for hallway generation
		// Implement the logic for traversing between selected room centers
	}

	private void PlaceRoom(Vector2I location, Vector2I size)
	{
		int width = (location.X + size.X) / TileSize;
		int height = (location.Y + size.Y) / TileSize;

		location = location / TileSize;

		Array<Vector2I> cellsToModify = new Array<Vector2I>();
		Array<Vector2I> innerCellsToModify = new Array<Vector2I>();

		for (int x = location.X; x < width; x++)
		{
			for (int y = location.Y; y < height; y++)
			{
				Vector2I position = new Vector2I(x, y);
				cellsToModify.Add(position);

				// Check if the tile is within the smaller bounds
				if (x >= location.X + 2 && x < width - 2 && y >= location.Y + 2 && y < height - 2)
				{
					innerCellsToModify.Add(position);
				}
			}
		}

		floorLayer.SetCellsTerrainConnect(cellsToModify, 0, 0, false);
		wallLayer.SetCellsTerrainConnect(cellsToModify, 1, 0, false);
		wallLayer.SetCellsTerrainConnect(innerCellsToModify, 1, -1, false);

	}

	private void PlaceHallway(Vector2I location)
	{
		PlaceCube(location, Vector2I.One, HallwayColor);
	}

	private void PlaceCube(Vector2I location, Vector2I size, Color color)
	{
		var tileInstance = SquarePrefab.Instantiate<Node2D>();
		if (tileInstance == null)
		{
			GD.PrintErr("Failed to instantiate CubePrefab.");
			return;
		}

		AddChild(tileInstance);

		var rect = tileInstance.GetNode<ColorRect>("TileRect");
		if (rect != null)
		{
			rect.Color = color;
			rect.Size = size;
			rect.Visible = true;
		}

		tileInstance.Position = new Vector2(location.X, location.Y);
	}
}
