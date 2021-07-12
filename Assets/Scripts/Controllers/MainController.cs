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

    private SceneBundleLoader loader;

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
    isEditor = false
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

    public Coroutine SwitchMainMenuToGame()
    {
        UnloadMainMenu();
        return LoadGame();
    }
}
