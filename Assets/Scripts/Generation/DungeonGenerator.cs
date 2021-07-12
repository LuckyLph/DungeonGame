using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

/// <summary>
/// 0=vide
/// 1=salle
/// 2=margin
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    private static System.Random rand = new System.Random();

    public int DungeonWidth { get { return dungeonWidth; } }
    public int DungeonHeight { get { return dungeonHeigth; } }

    [SerializeField] private int dungeonWidth;
    [SerializeField] private int dungeonHeigth;
    [SerializeField] private int marginWidth;
    [SerializeField] private int hallwayWidth;
    [SerializeField] private RoomType[] upperLeftRooms;
    [SerializeField] private RoomType[] upperRightRooms;
    [SerializeField] private RoomType[] lowerLeftRooms;
    [SerializeField] private RoomType[] lowerRightRooms;

    [SerializeField] private GameObject gridObject;

    private int[,] grid;
    private List<DungeonRoom> rooms = new List<DungeonRoom>();
    private List<DungeonRoom> roomsToClear = new List<DungeonRoom>();
    private List<RoomType> roomsToPlaceList = new List<RoomType>();
    private Tilemap outerWall;

    private readonly Vector2Int DefaultRoomSize = new Vector2Int(10, 10);
    private readonly Vector2Int StartRoomSize = new Vector2Int(18, 10);

    public void Generate()
    {
        Init();
        InstantiateRoom(new DungeonRoom(new Vector2Int(dungeonWidth / 2 - StartRoomSize.x / 2, dungeonHeigth / 2 - StartRoomSize.y / 2),
                        StartRoomSize, RoomType.StartRoom));
        
        //PlaceRooms();

        //PrintGrid();
    }

    void PlaceRooms()
    {
        if (upperLeftRooms.Length > 0)
        {
            roomsToPlaceList = upperLeftRooms.ToList();
            roomsToPlaceList = roomsToPlaceList.OrderBy(a => rand.Next()).ToList();

            foreach (var i in roomsToPlaceList)
            {
                if (i == RoomType.DefaultRoom)
                {
                    PlaceRoom(new DungeonRoom(new Vector2Int(0, 0), DefaultRoomSize, i));
                }
            }
            roomsToClear.Clear();
        } 
    }

    bool PlaceRoom(DungeonRoom room)
    {
        for (int i = 0; i < 50; i++)
        {
            Vector2Int randomPos = new Vector2Int(Random.Range(0, dungeonWidth), Random.Range(0, dungeonHeigth));
            room.Position = randomPos;
            if (CheckIfRoomFits(room))
            {
                InstantiateRoom(room);
                return true;
            }
        }
        roomsToClear.Add(room);
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
                    if (grid[i, j] != 0)
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
            AddMargin(room);

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
            Debug.Log(e.Message + e.StackTrace);
        }
    }

    private void Init()
    {
        gridObject.ClearChildren();
        grid = new int[dungeonHeigth, dungeonWidth];
    }
    void InstantiateRoom(DungeonRoom room)
    {
        GameObject instance = Instantiate(Resources.Load(room.PrefabPath, typeof(GameObject))) as GameObject;
        instance.transform.SetParent(gridObject.transform);
        instance.transform.position = GridToWorldSpace(room.Position);
        room.Instance = instance;
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

    #region CANCER
    void AddMargin(DungeonRoom room)
    {
        for (int i = room.Position.y; i < room.Position.y + room.Size.y; i++)
        {
            for (int j = room.Position.x; j >= room.Position.x - marginWidth; j--)
            {
                if (j < 0)
                {
                    break;
                }

                if (i < dungeonHeigth && j < dungeonWidth)
                {
                    grid[i, j] = 2;
                }
            }
        }
        for (int i = room.Position.y; i < room.Position.y + room.Size.y; i++)
        {
            for (int j = room.Position.x + room.Size.x; j < room.Position.x + room.Size.x + marginWidth; j++)
            {
                if (j >= dungeonWidth)
                {
                    break;
                }

                if (i < dungeonHeigth && j < dungeonWidth)
                {
                    grid[i, j] = 2;
                }
            }
        }
        for (int i = room.Position.y; i >= room.Position.y - marginWidth; i--)
        {
            for (int j = room.Position.x; j < room.Position.x + room.Size.x; j++)
            {
                if (i < 0)
                {
                    break;
                }

                if (i < dungeonHeigth && j < dungeonWidth)
                {
                    grid[i, j] = 2;
                }
            }
        }
        for (int i = room.Position.y + room.Size.y; i < room.Position.y + room.Size.y + marginWidth; i++)
        {
            for (int j = room.Position.x; j < room.Position.x + room.Size.x; j++)
            {
                if (i >= dungeonHeigth)
                {
                    break;
                }

                if (i < dungeonHeigth && j < dungeonWidth)
                {
                    grid[i, j] = 2;
                }
            }
        }


        if (room.Position.x >= marginWidth && room.Position.y >= marginWidth)
        {
            for (int i = room.Position.y - marginWidth; i < room.Position.y; i++)
            {
                for (int j = room.Position.x - marginWidth; j < room.Position.x; j++)
                {
                    if (i < dungeonHeigth && j < dungeonWidth)
                    {
                        grid[i, j] = 2;
                    }
                }
            }
        }
        if (room.Position.x < dungeonWidth - marginWidth && room.Position.y >= marginWidth)
        {
            for (int i = room.Position.y - marginWidth; i < room.Position.y; i++)
            {
                for (int j = room.Position.x + room.Size.x; j < room.Position.x + room.Size.x + marginWidth; j++)
                {
                    if (i < dungeonHeigth && j < dungeonWidth)
                    {
                        grid[i, j] = 2;
                    }
                }
            }
        }
        if (room.Position.x < dungeonWidth - marginWidth && room.Position.y < dungeonHeigth - marginWidth)
        {
            for (int i = room.Position.y + room.Size.y; i < room.Position.y + room.Size.y + marginWidth; i++)
            {
                for (int j = room.Position.x + room.Size.x; j < room.Position.x + room.Size.x + marginWidth; j++)
                {
                    if (i < dungeonHeigth && j < dungeonWidth)
                    {
                        grid[i, j] = 2;
                    }
                }
            }
        }
        if (room.Position.x >= marginWidth && room.Position.y < dungeonHeigth - marginWidth)
        {
            for (int i = room.Position.y + room.Size.y; i < room.Position.y + room.Size.y + marginWidth; i++)
            {
                for (int j = room.Position.x - marginWidth; j < room.Position.x; j++)
                {
                    if (i < dungeonHeigth && j < dungeonWidth)
                    {
                        grid[i, j] = 2;
                    }
                }
            }
        }
    }
    #endregion
}
