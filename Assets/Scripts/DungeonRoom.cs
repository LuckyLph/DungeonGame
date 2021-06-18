using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonRoom
{
    public Vector2Int Position;
    public Vector2Int Size;
    public bool[] Doors = new bool[4];
    public string Type;
    public GameObject Instance;

    public DungeonRoom()
    {

    }

    public DungeonRoom(Vector2Int position, Vector2Int size, string type)
    {
        Position = position;
        Size = size;
        Type = type;
    }
}
