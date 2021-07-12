using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private PlayerSensor doorSensor;

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
        gameObject.SetActive(false);
    }

    private void OnPlayerSensorEntered(GameObject playerCollider)
    {
        playerCollider.transform.parent.GetComponent<SimpleCharacterController>().OnPlayerInteract += OnPlayerInteract;
    }

    private void OnPlayerSensorExited(GameObject playerCollider)
    {
        playerCollider.transform.parent.GetComponent<SimpleCharacterController>().OnPlayerInteract -= OnPlayerInteract;
    }
}
