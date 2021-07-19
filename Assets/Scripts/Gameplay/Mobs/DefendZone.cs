using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class DefendZone : MonoBehaviour
{
    public Zone zone;

    private IAstarAI agent;
    private GameObject player;
    private Vector3 startPoint;

    private void Awake()
    {
        agent = GetComponent<IAstarAI>();
    }

    private void OnEnable()
    {
        zone.OnZoneSensorEntered += OnZoneSensorEntered;
        zone.OnZoneSensorExited += OnZoneSensorExited;
    }

    private void OnDisable()
    {
        zone.OnZoneSensorEntered -= OnZoneSensorEntered;
        zone.OnZoneSensorExited -= OnZoneSensorExited;
    }

    private void Update()
    {
        if (player != null)
        {
            agent.destination = player.transform.position;
        }
        else
        {
            agent.destination = startPoint;
        }
    }

    private void LateUpdate()
    {
        agent.SearchPath();
    }

    private void OnZoneSensorEntered(GameObject player)
    {
        this.player = player;
    }

    private void OnZoneSensorExited(GameObject player)
    {
        this.player = null;
    }

    public void SetStartPoint(Vector3 position)
    {
        startPoint = position;
    }
}
