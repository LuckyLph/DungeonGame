using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private DungeonGenerator generator;

    private void Awake()
    {
        generator = GameObject.FindWithTag("Generator").GetComponent<DungeonGenerator>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
