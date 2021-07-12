using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Harmony;

public class MainMenuButtonController : MonoBehaviour
{
    private MainController mainController;

    private void Awake()
    {
        mainController = GameObject.FindWithTag("MainController").GetComponent<MainController>();
    }
    public void Play()
    {
        mainController.SwitchMainMenuToGame();
    }

    public void ShowOptions()
    {

    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
