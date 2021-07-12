using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    private GameObject currentlySelected;

    void Start()
    {
        
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            currentlySelected = EventSystem.current.currentSelectedGameObject;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(currentlySelected);
        }
    }
}
