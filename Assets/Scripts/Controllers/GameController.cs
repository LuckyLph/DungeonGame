using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Harmony;

public class GameController : MonoBehaviour
{
    private SimpleCharacterController characterController;
    private MainController mainController;
    private CameraController cameraController;

    public float GameTimer { get; set; }

    private void Awake()
    {
        characterController = GameObject.FindWithTag("Player").GetComponent<SimpleCharacterController>();
        mainController = GameObject.FindWithTag("MainController").GetComponent<MainController>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void Start()
    {
        cameraController.EnableCamera(GameObject.FindWithTag("Player"));
        GameTimer = 0;
    }

    private void Update()
    {
        GameTimer = GameTimer + Time.deltaTime;
        mainController.GameTimer = GameTimer;
        mainController.PlayerLives = characterController.Lives;
        mainController.PlayerStars = characterController.Stars;
        if (characterController.Lives == 0)
        {
            cameraController.CameraEnabled = false;
            mainController.IsGameWon = false;
            mainController.SwitchGameToEndMenu();
        }
        if (characterController.Stars == 4)
        {
            cameraController.CameraEnabled = false;
            mainController.IsGameWon = true;
            mainController.SwitchGameToEndMenu();
        }
    }
}
