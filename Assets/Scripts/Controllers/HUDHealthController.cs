using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDHealthController : MonoBehaviour
{
    [SerializeField] private Image[] healthIcons;
    private SimpleCharacterController characterController;

    private void Awake()
    {
        characterController = GameObject.FindWithTag("Player").GetComponent<SimpleCharacterController>();
    }

    private void Update()
    {
        if (characterController.Lives == 4)
        {
            healthIcons[0].gameObject.SetActive(true);
            healthIcons[1].gameObject.SetActive(true);
            healthIcons[2].gameObject.SetActive(true);
            healthIcons[3].gameObject.SetActive(true);
        }
        else if (characterController.Lives == 3)
        {
            healthIcons[0].gameObject.SetActive(true);
            healthIcons[1].gameObject.SetActive(true);
            healthIcons[2].gameObject.SetActive(true);
            healthIcons[3].gameObject.SetActive(false);
        }
        else if (characterController.Lives == 2)
        {
            healthIcons[0].gameObject.SetActive(true);
            healthIcons[1].gameObject.SetActive(true);
            healthIcons[2].gameObject.SetActive(false);
            healthIcons[3].gameObject.SetActive(false);
        }
        else if (characterController.Lives == 1)
        {
            healthIcons[0].gameObject.SetActive(true);
            healthIcons[1].gameObject.SetActive(false);
            healthIcons[2].gameObject.SetActive(false);
            healthIcons[3].gameObject.SetActive(false);
        }
        else if (characterController.Lives == 0)
        {
            healthIcons[0].gameObject.SetActive(false);
            healthIcons[1].gameObject.SetActive(false);
            healthIcons[2].gameObject.SetActive(false);
            healthIcons[3].gameObject.SetActive(false);
        }
    }
}
