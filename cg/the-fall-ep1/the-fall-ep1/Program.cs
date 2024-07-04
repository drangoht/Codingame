using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] fakeinputs = ["0 0 0 0 0 3","8 2 2 2 2 10","3 0 0 0 12 13","11 2 2 2 1 10","2 13 0 0 3 0","0 7 2 2 4 13","0 3 0 12 4 10","0 11 2 5 10 0"];

        IEnumerable<RoomDefinition> roomDefinitions = InitRoomDefinitions();
        Graph graph = new Graph();
        int EXITROOMTYPE = -1;
        string[] inputs;
        //inputs = Console.ReadLine().Split(' ');
        int W = 6;// int.Parse(inputs[0]); // number of columns.
        int H = 8;// int.Parse(inputs[1]); // number of rows.
        for (int i = 0; i < H; i++)
        {
            string LINE = fakeinputs[i]; // Console.ReadLine(); // represents a line in the grid and contains W integers. Each integer represents one room of a given type.

            var casesType = LINE.Split(' ');
            for (int j = 0; j < W; j++)
            {
                //Console.Error.WriteLine($"{casesType[j]}");
                graph.AllNodes.Add(new Room()
                {
                    Definition = roomDefinitions.Single(r => r.RoomType == Int32.Parse(casesType[j])),
                    Position = new Vector(j, i)
                });
            }
        }

        //int EX = int.Parse(Console.ReadLine()); // the coordinate along the X axis of the exit (not useful for this first mission, but must be read).
        int EX = 3;
        graph.AllNodes.Add(new Room()
        {
            Definition = roomDefinitions.Single(r => r.RoomType == EXITROOMTYPE),
            Position = new Vector(EX, H)
        });

        foreach (var room in graph.AllNodes)
        {
            Console.Error.WriteLine($"X:{room.Position.X} Y:{room.Position.Y} Type:{room.Definition.RoomType}");
        }


        // Root
        //inputs = Console.ReadLine().Split(' ');
        int XI = 5;// int.Parse(inputs[0]);
        int YI = 0;// int.Parse(inputs[1]);
        string POS = "TOP";// inputs[2];
        graph.Root = graph.AllNodes.Single(r => r.Position.X == XI && r.Position.Y == YI);
        var exit = graph.AllNodes.Single(r => r.Definition.RoomType == EXITROOMTYPE);
        graph.MakeRoomLinks(exit);
        List<Vector> fullPath = new();
        graph.SetFullPath(graph.Root, Direction.GetValueByName(POS), exit, fullPath);

        foreach (var v in fullPath)
        {

            Console.WriteLine($"{v.X} {v.Y}");

            inputs = Console.ReadLine().Split(' ');
            XI = int.Parse(inputs[0]);
            YI = int.Parse(inputs[1]);
            POS = inputs[2];
        }
        // game loop
        //while (true)
        //{
        //    // inputs = Console.ReadLine().Split(' ');
        //    // XI = int.Parse(inputs[0]);
        //    // YI = int.Parse(inputs[1]);
        //    // POS = inputs[2];

        //    // Write an action using Console.WriteLine()
        //    // To debug: Console.Error.WriteLine("Debug messages...");


        //    // One line containing the X Y coordinates of the room in which you believe Indy will be on the next turn.
        //    Console.WriteLine("0 0");
        //}
    }
    private static List<RoomDefinition> InitRoomDefinitions()
    {
        return new List<RoomDefinition>() {
        new RoomDefinition(
            -1,
            new List<Path>()
        ),
        new RoomDefinition(
            0,
            new List<Path>()
        ),
        new RoomDefinition(
            1,
            new List<Path>(){
                new Path(Direction.LEFT,Direction.BOTTOM),
                new Path(Direction.RIGHT,Direction.BOTTOM),
                new Path(Direction.TOP,Direction.BOTTOM),
            }
        ),
        new RoomDefinition(
            2,
            new List<Path>(){
                new Path(Direction.LEFT,Direction.RIGHT),
                new Path(Direction.RIGHT,Direction.LEFT),
            }
        ),
        new RoomDefinition(
            3,
            new List<Path>(){
                new Path(Direction.TOP,Direction.BOTTOM)
            }
        ),
        new RoomDefinition(
            4,
            new List<Path>(){
                new Path(Direction.TOP,Direction.LEFT),
                new Path(Direction.RIGHT,Direction.BOTTOM)
            }
        ),
        new RoomDefinition
        (
            5,
            new List<Path>()
            {
                new Path(Direction.TOP,Direction.RIGHT),
                new Path(Direction.LEFT,Direction.BOTTOM)
            }
        ),
        new RoomDefinition
        (
            6,
            new List<Path>()
            {
                new Path(Direction.LEFT,Direction.RIGHT),
                new Path(Direction.RIGHT,Direction.LEFT)
            }
        ),
        new RoomDefinition
        (
            7,
            new List<Path>()
            {
                new Path(Direction.TOP,Direction.BOTTOM),
                new Path(Direction.RIGHT,Direction.BOTTOM)
            }
        ),
        new RoomDefinition
        (
            8,
            new List<Path>()
            {
                new Path(Direction.LEFT,Direction.BOTTOM),
                new Path(Direction.RIGHT,Direction.BOTTOM)
            }
        ),
        new RoomDefinition
        (
            9,
            new List<Path>()
            {
                new Path(Direction.LEFT,Direction.BOTTOM),
                new Path(Direction.TOP,Direction.BOTTOM)
            }
        ),
        new RoomDefinition
        (
            10,
            new List<Path>()
            {
                new Path(Direction.TOP,Direction.LEFT)
            }
        ),
        new RoomDefinition
        (
            11,
            new List<Path>()
            {
                new Path(Direction.TOP,Direction.RIGHT)
            }
        ),
        new RoomDefinition
        (
            12,
            new List<Path>()
            {
                new Path(Direction.RIGHT,Direction.BOTTOM)
            }
        ),
        new RoomDefinition
        (
            13,
            new List<Path>()
            {
                new Path(Direction.LEFT,Direction.BOTTOM)
            }
        ),
       };
    }

}

public class Graph
{
    public Room? Root { get; set; }
    private List<Room> visitedRooms = new();
    public List<Room> AllNodes { get; set; } = new List<Room>();
    public List<Link<Room>> Links { get; set; } = new List<Link<Room>>();
    public void CreateNode(Room no)
    {
        AllNodes.Add(no);
    }
    public void CreateLink(Room parent, Room child)
    {
        if(!Links.Any(l => l.Child == child && l.Parent==parent))
            Links.Add(new Link<Room>() { Parent = parent, Child = child });
    }
    // GetNeighbours where path lead to room
    public List<Room> GetNeighbours(Room room) =>
        AllNodes
            .SelectMany(room => room.Definition.Paths, (room, path) => new { room, path })
            .Where(rp =>
                rp.room.Position.X + Helper.DirectionToVector(rp.path.Exit).X == room.Position.X &&
                rp.room.Position.Y + Helper.DirectionToVector(rp.path.Exit).Y == room.Position.Y)
            .Select(r => r.room)
            .ToList();

    public void MakeRoomLinks(Room childRoom) => MakeRoomLinks(childRoom, null);

    public void MakeRoomLinks(Room parentRoom,Room? childRoom)
    {
        if (childRoom is not null)
            CreateLink(parentRoom, childRoom);
        if (!visitedRooms.Contains(parentRoom))
        {
            var neighbours = GetNeighbours(parentRoom);
            visitedRooms.Add(parentRoom);
            foreach (var neighbour in neighbours)
            {
                MakeRoomLinks(neighbour, parentRoom);
            }
            
        }
    }
    public void SetFullPath(Room start,int startDirection,Room end, List<Vector> fullPath)
    {
        if (start == end)
            return;
        var exitDirection = start.Definition.GetExitDirection(startDirection);
        var nextRoom = Links.SingleOrDefault(r =>
            r.Parent == start &&
            r.Child!.Position.X == start.Position.X + Helper.DirectionToVector(exitDirection).X &&
            r.Child!.Position.Y == start.Position.Y + Helper.DirectionToVector(exitDirection).Y)!.Child;
        fullPath.Add(new Vector(nextRoom.Position.X, nextRoom.Position.Y));
        startDirection = Helper.GetStartDirectionFromExitDirection(exitDirection);
        SetFullPath(nextRoom, startDirection, end, fullPath);
    }
}

public class Link<T>
{

    public T? Parent { get; set; }
    public T? Child { get; set; }
}

public class RoomDefinition
{
    public int RoomType { get; }
    public List<Path> Paths { get; }
    public RoomDefinition(int roomType, List<Path> paths)
    {
        RoomType = roomType;
        Paths = paths;
    }
    public int? GetExitDirection(int startDirection) =>
        Paths.SingleOrDefault(p => p.Entrance == startDirection)!.Exit;
}
public class Path
{
    public int Entrance { get; }
    public int Exit { get; }
    public Path(int entrance, int exit)
    {
        Entrance = entrance;
        Exit = exit;
    }
}
public static class Direction
{
    public const int TOP = 0;
    public const int BOTTOM = 1;
    public const int LEFT  = 2;
    public const int RIGHT = 3;
    public static int GetValueByName(string name) => name switch
    {
        "TOP" => TOP,
        "BOTTOM" => BOTTOM,
        "LEFT" => LEFT,
        "RIGHT" => RIGHT,
        _ => throw new ArgumentOutOfRangeException(nameof(name), $"Not expected direction name: {name}"),
    };
}
public class Room : IEquatable<Room>
{
    public Vector Position { get; set; }
    public RoomDefinition Definition { get; set; }

    public bool Equals(Room? other)
    {
        return other?.Position.X == Position.X && other?.Position.Y == Position.Y;
    }
}

public record Vector(int X = 0, int Y = 0);
public static class Helper
{
    public static Vector DirectionToVector(int? direction) => direction switch
    {
        Direction.TOP => new Vector(0, -1),
        Direction.BOTTOM => new Vector(0, 1),
        Direction.LEFT => new Vector(-1, 0),
        Direction.RIGHT => new Vector(1, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Not expected direction value: {direction}"),
    };
    public static int GetStartDirectionFromExitDirection(int? direction) => direction switch
    {
        Direction.BOTTOM => Direction.TOP,
        Direction.LEFT => Direction.RIGHT,
        Direction.RIGHT => Direction.LEFT,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Not expected direction value: {direction}"),
    };
}