using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TogglePause : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private InputAction pauseAction;

    private void OnEnable()
    {
        pauseAction.performed += PauseActionPerformed;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.performed -= PauseActionPerformed;
        pauseAction.Disable();
    }

    private void PauseActionPerformed(InputAction.CallbackContext obj)
    {
        ToggleMenu();
    }

    public bool IsPaused { get; set; }

    private void Start()
    {
        IsPaused = false;
    }

    public void ToggleMenu()
    {
        if (!menu.activeInHierarchy && !IsPaused)
        {
            menu.SetActive(true);
        }
        else
        {
            menu.SetActive(false);
        }
    }
}
