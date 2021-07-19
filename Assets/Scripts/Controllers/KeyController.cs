using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    private PlayerSensor doorSensor;

    private void Awake()
    {
        doorSensor = GetComponentInChildren<PlayerSensor>();
    }

    private void OnEnable()
    {
        doorSensor.OnPlayerSensorEntered += OnPlayerSensorEntered;
    }

    private void OnDisable()
    {
        doorSensor.OnPlayerSensorEntered -= OnPlayerSensorEntered;
    }

    private void OnPlayerSensorEntered(GameObject player)
    {
        player.transform.root.GetComponent<SimpleCharacterController>().Keys++;
        gameObject.SetActive(false);
    }
}
