using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallwayGenerator
{
    private int[,] grid;
    private int[,] roomGrid;

    public void GeneratePath(int[,] roomGrid, List<DungeonRoom> rooms, DungeonRoom roomToConnect)
    {
        this.roomGrid = roomGrid;


    }
}
