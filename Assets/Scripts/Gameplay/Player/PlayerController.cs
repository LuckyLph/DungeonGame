using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    private Rigidbody2D rigidbody;
    private Vector2 movementVector;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        
    }

    void Update()
    {
        if (movementVector != Vector2.zero)
        {
            rigidbody.MovePosition(rigidbody.position + movementVector * moveSpeed * Time.deltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }
}
