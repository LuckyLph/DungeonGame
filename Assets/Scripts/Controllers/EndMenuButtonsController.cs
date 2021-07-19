using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMenuButtonsController : MonoBehaviour
{
    private MainController mainController;

    private void Awake()
    {
        mainController = GameObject.FindWithTag("MainController").GetComponent<MainController>();
    }
    public void PlayAgain()
    {
        mainController.SwitchEndMenuToGame();
    }

    public void MainMenu()
    {
        mainController.SwitchEndMenuToMainMenu();
    }
}
