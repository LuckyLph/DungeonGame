using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuButtonsController : MonoBehaviour
{
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject restartGroup;
    [SerializeField] private GameObject quitGroup;
    [SerializeField] private TogglePause togglePause;

    private MainController mainController;
    private CameraController cameraController;

    private void Awake()
    {
        mainController = GameObject.FindWithTag("MainController").GetComponent<MainController>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void OnEnable()
    {
        menuButtons.SetActive(true);
        restartGroup.SetActive(false);
        quitGroup.SetActive(false);
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void Resume()
    {
        togglePause.ToggleMenu();
    }

    public void Restart()
    {
        menuButtons.SetActive(false);
        restartGroup.SetActive(true);
    }

    public void ConfirmRestart()
    {
        //togglePause.ToggleMenu();
        //mainController.ReloadGame();
    }

    public void CancelRestart()
    {
        restartGroup.SetActive(false);
        menuButtons.SetActive(true);
    }

    public void ConfirmMainMenu()
    {
        cameraController.CameraEnabled = false;
        togglePause.ToggleMenu();
        mainController.SwitchGameToMainMenu();
    }

    public void CancelMainMenu()
    {
        quitGroup.SetActive(false);
        menuButtons.SetActive(true);
    }

    public void MainMenu()
    {
        menuButtons.SetActive(false);
        quitGroup.SetActive(true);
    }
}
