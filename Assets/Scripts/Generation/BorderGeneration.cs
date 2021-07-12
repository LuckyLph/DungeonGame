using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BorderGeneration : MonoBehaviour
{
    [SerializeField] int dungeonWidth = 200;
    [SerializeField] int dungeonHeight = 200;

    [SerializeField] GameObject gridObject;

    public void Generate()
    {
        PlaceBottomLeftCorner(new Vector3Int(0, 0, 0));
    }

    /// <summary>
    /// 34
    /// 12
    /// </summary>
    /// <param name="position"></param>
    private void PlaceBottomLeftCorner(Vector3Int position)
    {
        //string[] spritePaths = new string[4] { "Tiles/Dungeon@128x128_111", "Tiles/Dungeon@128x128_112", "Tiles/Dungeon@128x128_95", "Tiles/Dungeon@128x128_96" };
        string spritePath = "Tiles/border.png";

        var tileList = new List<Tile>();
        for (int i = 0; i < 4; i++)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Resources.Load<Sprite>(spritePath);
            tileList.Add(tile);
        }
        Tilemap tilemap = gridObject.GetComponentInChildren<Tilemap>();
        tilemap.SetTile(position, tileList[0]);
        tilemap.SetTile(position + new Vector3Int(1, 0, 0), tileList[1]);
        tilemap.SetTile(position + new Vector3Int(0, 1, 0), tileList[2]);
        tilemap.SetTile(position + new Vector3Int(1, 1, 0), tileList[3]);
    }
}
