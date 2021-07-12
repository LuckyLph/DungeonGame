using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonRoom
{
    private enum DoorDirections
    {
        EW,
        NE,
        NS,
        NW,
        SE,
        SW,
    }
    private string[] roomNames = new string[3] {"DefaultRoom", "StartRoom", "LargeRoom"};

    public Vector2Int Position;
    public Vector2Int Size;
    public string PrefabPath;
    public GameObject Instance;
    public RoomType Type;

    public DungeonRoom(Vector2Int position, Vector2Int size, RoomType type)
    {
        Position = position;
        Size = size;
        Type = type;
        if (type == RoomType.DefaultRoom)
        {
            RandomizeDoorsPlacement();
        }
        else
        {
            PrefabPath = DefaultPrefabPath();
        }
    }

    private void RandomizeDoorsPlacement()
    {
        int firstDoor = Random.Range(0, 3);
        int secondDoor;
        do
        {
            secondDoor = Random.Range(0, 3);
        } while (secondDoor == firstDoor);

        switch (firstDoor)
        {
            case 0:
                if (secondDoor == 1) SetPrefabPath(DoorDirections.NE);
                if (secondDoor == 2) SetPrefabPath(DoorDirections.NS);
                if (secondDoor == 3) SetPrefabPath(DoorDirections.NW);
                break;
            case 1:
                if (secondDoor == 0) SetPrefabPath(DoorDirections.NE);
                if (secondDoor == 2) SetPrefabPath(DoorDirections.SE);
                if (secondDoor == 3) SetPrefabPath(DoorDirections.EW);
                break;
            case 2:
                if (secondDoor == 0) SetPrefabPath(DoorDirections.NS);
                if (secondDoor == 1) SetPrefabPath(DoorDirections.SE);
                if (secondDoor == 3) SetPrefabPath(DoorDirections.SW);
                break;
            case 3:
                if (secondDoor == 0) SetPrefabPath(DoorDirections.NW);
                if (secondDoor == 1) SetPrefabPath(DoorDirections.EW);
                if (secondDoor == 2) SetPrefabPath(DoorDirections.SW);
                break;
        }
    }

    private void SetPrefabPath(DoorDirections direction)
    {
        PrefabPath = DefaultPrefabPath() + direction.ToString();
    }

    private string DefaultPrefabPath()
    {
        return "Rooms/" + roomNames[(int)Type];
    }
}
