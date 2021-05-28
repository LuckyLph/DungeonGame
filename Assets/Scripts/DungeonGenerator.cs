using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private int dungeonWidth;
    [SerializeField]
    private int dungeonHeigth;
    [SerializeField]
    private RoomType[] roomsToPlace;

    [SerializeField]
    private GameObject gridObject;

    private int[,] grid;
    private List<DungeonRoom> rooms;
    private List<GameObject> roomsInstances = new List<GameObject>();
    private List<RoomType> roomsToPlaceList;

    private readonly string DefaultRoom = "DefaultRoom";
    private readonly string DefaultRoomE = "DefaultRoomE";
    private readonly string DefaultRoomEW = "DefaultRoomEW";
    private readonly string DefaultRoomN = "DefaultRoomN";
    private readonly string DefaultRoomNE = "DefaultRoomNE";
    private readonly string DefaultRoomNS = "DefaultRoomNS";
    private readonly string DefaultRoomNW = "DefaultRoomNW";
    private readonly string DefaultRoomS = "DefaultRoomS";
    private readonly string DefaultRoomSE = "DefaultRoomSE";
    private readonly string DefaultRoomSW = "DefaultRoomSW";
    private readonly string DefaultRoomW = "DefaultRoomW";
    private readonly string StartRoom = "StartRoom";
    private readonly Vector2Int DefaultRoomSize = new Vector2Int(10, 10);
    private readonly Vector2Int StartRoomSize = new Vector2Int(18, 10);

    private static System.Random rand = new System.Random();

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        gridObject.ClearChildren();
        grid = new int[dungeonHeigth, dungeonWidth];
        rooms = new List<DungeonRoom>();
        roomsToPlaceList = new List<RoomType>(roomsToPlace);
        InstantiateRoom(new DungeonRoom(new Vector2Int(41, 45), StartRoomSize, StartRoom));

        PlaceRooms();
        PrintGrid();
    }

    void PlaceRooms()
    {
        roomsToPlaceList = roomsToPlaceList.OrderBy(a => rand.Next()).ToList();

        foreach (var i in roomsToPlaceList)
        {
            if (i == RoomType.DefaultRoom)
            {
                PlaceRoom(new DungeonRoom(new Vector2Int(0, 0), DefaultRoomSize, DefaultRoom));
            }
        }
    }

    bool PlaceRoom(DungeonRoom room)
    {
        for (int i = 0; i < 50; i++)
        {
            Vector2Int randomPos = new Vector2Int(rand.Next(0, 100), rand.Next(0, 100));
            room.Position = randomPos;
            if (CheckIfRoomFits(room))
            {
                InstantiateRoom(room);
                return true;
            }
        }
        return false;
    }

    bool CheckIfRoomFits(DungeonRoom room)
    {
        if (room.Position.x + room.Size.x > dungeonWidth || room.Position.y + room.Size.y > dungeonHeigth)
        {
            return false;
        }

        for (int i = room.Position.y; i < room.Position.y + room.Size.y; i++)
        {
            for (int j = room.Position.x; j < room.Position.x + room.Size.x; j++)
            {
                if (i < dungeonHeigth && j < dungeonWidth)
                {
                    if (grid[i, j] == 1)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    void AddRoomToGrid(DungeonRoom room)
    {
        try
        {
            for (int i = room.Position.y; i < room.Position.y + room.Size.y; i++)
            {
                for (int j = room.Position.x; j < room.Position.x + room.Size.x; j++)
                {
                    if (i < dungeonHeigth && j < dungeonWidth)
                    {
                        grid[i, j] = 1;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Invalid room!");
            Debug.Log(e.StackTrace);
        }
    }

    void InstantiateRoom(DungeonRoom room)
    {
        GameObject instance = Instantiate(Resources.Load(room.Type, typeof(GameObject))) as GameObject;
        instance.transform.SetParent(gridObject.transform);
        instance.transform.position = GridToWorldSpace(room.Position);
        rooms.Add(room);
        AddRoomToGrid(room);
    }

    void PrintGrid()
    {
        int rowLength = grid.GetLength(0);
        int colLength = grid.GetLength(1);
        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                stringBuilder.Append(grid[i, j].ToString());
                stringBuilder.Append(" ");
            }
            stringBuilder.Append(System.Environment.NewLine);
        }

        PrintFile(stringBuilder.ToString());
    }

    void PrintFile(string content)
    {
        var fileName = "MyLog.txt";
        var sr = File.CreateText(fileName);
        sr.Write(content);
        sr.Close();
    }

    Vector3 GridToWorldSpace(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x, dungeonHeigth - gridPosition.y);
    }

    Vector2Int GetRandomGridLocation()
    {
        var rand = new System.Random();
        return new Vector2Int(rand.Next(0, 100), rand.Next(0, 100));
    }
}
