using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private PlayerSensor doorSensor;
    private SimpleCharacterController controller;

    private void Awake()
    {
        doorSensor = GetComponentInChildren<PlayerSensor>();
    }

    private void OnEnable()
    {
        doorSensor.OnPlayerSensorEntered += OnPlayerSensorEntered;
        doorSensor.OnPlayerSensorExited += OnPlayerSensorExited;
    }

    private void OnDisable()
    {
        doorSensor.OnPlayerSensorEntered -= OnPlayerSensorEntered;
        doorSensor.OnPlayerSensorExited -= OnPlayerSensorExited;
    }

    private void OnPlayerInteract()
    {
        if (controller.Keys > 0)
        {
            gameObject.SetActive(false);
            controller.Keys--;
        }
    }

    private void OnPlayerSensorEntered(GameObject playerCollider)
    {
        controller = playerCollider.transform.parent.GetComponent<SimpleCharacterController>();
        controller.OnPlayerInteract += OnPlayerInteract;
    }

    private void OnPlayerSensorExited(GameObject playerCollider)
    {
        controller.OnPlayerInteract -= OnPlayerInteract;
    }
}
