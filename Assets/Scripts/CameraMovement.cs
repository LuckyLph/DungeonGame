using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField]
    private float speed;

    public const float DiagonalMovementRatio = 1.41f;

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 movement = HandleDirectionInput();
        Camera.main.transform.position += movement * speed * Time.deltaTime;
    }

    private Vector3 HandleDirectionInput()
    {
        Vector3 movementVector = Vector3.zero;
        bool keyA = Input.GetKey(KeyCode.A);
        bool keyD = Input.GetKey(KeyCode.D);
        bool keyS = Input.GetKey(KeyCode.S);
        bool keyW = Input.GetKey(KeyCode.W);

        if (keyW && keyA)
        {
            if (!keyD)
                movementVector.x -= 1 / DiagonalMovementRatio;
            if (!keyS)
                movementVector.y += 1 / DiagonalMovementRatio;
        }
        else if (keyS && keyA)
        {
            if (!keyD)
                movementVector.x -= 1 / DiagonalMovementRatio;
            if (!keyW)
                movementVector.y -= 1 / DiagonalMovementRatio;
        }
        else if (keyW && keyD)
        {
            if (!keyA)
                movementVector.x += 1 / DiagonalMovementRatio;
            if (!keyS)
                movementVector.y += 1 / DiagonalMovementRatio;
        }
        else if (keyS && keyD)
        {
            if (!keyA)
                movementVector.x += 1 / DiagonalMovementRatio;
            if (!keyW)
                movementVector.y -= 1 / DiagonalMovementRatio;
        }
        else if (keyA && !keyD)
        {
            movementVector.x -= 1;
        }
        else if (keyD && !keyA)
        {
            movementVector.x += 1;
        }
        else if (keyW && !keyS)
        {
            movementVector.y += 1;
        }
        else if (keyS && !keyW)
        {
            movementVector.y -= 1;
        }

        return movementVector;
    }

}
