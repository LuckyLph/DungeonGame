using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public event PlayerSensorEnteredHandler OnZoneSensorEntered;
    public event PlayerSensorExitedHandler OnZoneSensorExited;

    private PlayerSensor playerSensor;

    private void Awake()
    {
        playerSensor = GetComponent<PlayerSensor>();
    }

    private void OnEnable()
    {
        playerSensor.OnPlayerSensorEntered += OnPlayerEntered;
        playerSensor.OnPlayerSensorExited += OnPlayerExited;
    }

    private void OnDisable()
    {
        playerSensor.OnPlayerSensorEntered -= OnPlayerEntered;
        playerSensor.OnPlayerSensorExited -= OnPlayerExited;
    }

    private void OnPlayerEntered(GameObject player)
    {
        OnZoneSensorEntered?.Invoke(player.transform.parent.gameObject);
    }

    private void OnPlayerExited(GameObject player)
    {
        OnZoneSensorExited?.Invoke(player.transform.parent.gameObject);
    }
}
