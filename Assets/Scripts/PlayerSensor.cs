using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerSensorEnteredHandler(GameObject player);
public delegate void PlayerSensorExitedHandler(GameObject player);

public class PlayerSensor : MonoBehaviour
{
    public event PlayerSensorEnteredHandler OnPlayerSensorEntered;
    public event PlayerSensorExitedHandler OnPlayerSensorExited;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnPlayerSensorEntered != null && collision != null) OnPlayerSensorEntered(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (OnPlayerSensorExited != null && collision != null) OnPlayerSensorExited(collision.gameObject);
    }

}
