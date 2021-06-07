using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField]
    private float speed;

    private HandleInput handleInput;

    void Start()
    {
        handleInput = GetComponent<HandleInput>();
    }

    void Update()
    {
        Vector3 movement = handleInput.GetKeyboardMovementVector();
        Camera.main.transform.position += movement * speed * Time.deltaTime;
    }
}
