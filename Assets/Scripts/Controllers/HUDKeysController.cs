using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDKeysController : MonoBehaviour
{
    private Text text;
    private SimpleCharacterController characterController;

    private void Awake()
    {
        text = GetComponentInChildren<Text>();
        characterController = GameObject.FindWithTag("Player").GetComponent<SimpleCharacterController>();
    }

    private void Update()
    {
        text.text = characterController.Keys.ToString();
    }
}
