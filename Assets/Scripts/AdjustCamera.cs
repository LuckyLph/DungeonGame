using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustCamera : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;

    private void Awake()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    private void Start()
    {
        Vector3 nextPos = new Vector3(dungeonGenerator.DungeonWidth / 2, dungeonGenerator.DungeonHeight / 2, -10);
        Camera.main.transform.position = nextPos;
        
    }
}
