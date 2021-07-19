using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDStarsController : MonoBehaviour
{
    [SerializeField] private Image[] starIcons;
    private SimpleCharacterController characterController;

    private void Awake()
    {
        characterController = GameObject.FindWithTag("Player").GetComponent<SimpleCharacterController>();
    }

    private void Update()
    {
        if (characterController.Stars == 4)
        {
            starIcons[0].gameObject.SetActive(true);
            starIcons[1].gameObject.SetActive(true);
            starIcons[2].gameObject.SetActive(true);
            starIcons[3].gameObject.SetActive(true);
        }
        else if (characterController.Stars == 3)
        {
            starIcons[0].gameObject.SetActive(true);
            starIcons[1].gameObject.SetActive(true);
            starIcons[2].gameObject.SetActive(true);
            starIcons[3].gameObject.SetActive(false);
        }
        else if (characterController.Stars == 2)
        {
            starIcons[0].gameObject.SetActive(true);
            starIcons[1].gameObject.SetActive(true);
            starIcons[2].gameObject.SetActive(false);
            starIcons[3].gameObject.SetActive(false);
        }
        else if (characterController.Stars == 1)
        {
            starIcons[0].gameObject.SetActive(true);
            starIcons[1].gameObject.SetActive(false);
            starIcons[2].gameObject.SetActive(false);
            starIcons[3].gameObject.SetActive(false);
        }
        else if (characterController.Stars == 0)
        {
            starIcons[0].gameObject.SetActive(false);
            starIcons[1].gameObject.SetActive(false);
            starIcons[2].gameObject.SetActive(false);
            starIcons[3].gameObject.SetActive(false);
        }
    }
}
