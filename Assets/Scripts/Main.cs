using System.Collections;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

public class Main : MonoBehaviour
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
        LoadMainMenu();
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
        return loader.Load(game);
    }

    public Coroutine UnloadMainMenu()
    {
        return loader.Unload(game);
    }
}
