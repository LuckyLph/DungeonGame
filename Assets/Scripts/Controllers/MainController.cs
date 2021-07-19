using System.Collections;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    [Header("Scene Bundles")]
    [SerializeField] private SceneBundle game;
    [SerializeField] private SceneBundle mainMenu;
    [SerializeField] private SceneBundle endGame;

    private SceneBundleLoader loader;

    public bool IsGameWon { get; set; }
    public int PlayerLives { get; set; }
    public int PlayerStars { get; set; }
    public float GameTimer { get; set; }

    private void Awake()
    {
        loader = GetComponent<SceneBundleLoader>();
    }

    private void Start()
    {
        bool isEditor = false;
#if UNITY_EDITOR
        isEditor = true;
#else
    isEditor = false;
#endif
        if (!isEditor || SceneManager.sceneCount == 1)
        {
            LoadMainMenu();
        }
        
    }

    public Coroutine LoadGame()
    {
        return loader.Load(game);
    }

    public Coroutine UnloadGame()
    {
        return loader.Unload(game);
    }
    public Coroutine LoadMainMenu()
    {
        return loader.Load(mainMenu);
    }

    public Coroutine UnloadMainMenu()
    {
        return loader.Unload(mainMenu);
    }
    public Coroutine LoadEndGame()
    {
        return loader.Load(endGame);
    }

    public Coroutine UnloadEndGame()
    {
        return loader.Unload(endGame);
    }

    public Coroutine SwitchMainMenuToGame()
    {
        UnloadMainMenu();
        return LoadGame();
    }

    public Coroutine SwitchGameToEndMenu()
    {
        UnloadGame();
        return LoadEndGame();
    }
    public Coroutine SwitchGameToMainMenu()
    {
        UnloadGame();
        return LoadMainMenu();
    }

    public Coroutine ReloadGame()
    {
        return loader.Reload(game);
    }

    public Coroutine SwitchEndMenuToGame()
    {
        UnloadEndGame();
        return LoadGame();
    }

    public Coroutine SwitchEndMenuToMainMenu()
    {
        UnloadEndGame();
        return LoadMainMenu();
    }
}
