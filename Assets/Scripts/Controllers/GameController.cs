using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private GameObject generatorObject;

    private void Awake()
    {
        generatorObject = GameObject.FindWithTag("Generator");
        dungeonGenerator = generatorObject.GetComponent<DungeonGenerator>();
    }

    private void Start()
    {
        Camera.main.GetComponent<CameraController>().EnableCamera(GameObject.FindWithTag("Player"));
    }

    private void Update()
    {
        
    }
}
