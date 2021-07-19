using System;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private int mob1SpawnAmount;
    [SerializeField] private int mob2SpawnAmount;
    [SerializeField] private MobSpawnPoint[] mobSpawnPoints;
    [SerializeField] private Zone[] mob2SpawnPoints;
    [SerializeField] private Transform mobsParent;

    void Start()
    {
        if (mob1SpawnAmount > mobSpawnPoints.Length) mob1SpawnAmount = mobSpawnPoints.Length;
        var sortedSpawnPoints = mobSpawnPoints.OrderBy(a => Guid.NewGuid()).ToArray();

        for (int i = 0; i < mob1SpawnAmount; i++)
        {
            GameObject instance = Instantiate(Resources.Load("mob", typeof(GameObject))) as GameObject;
            instance.transform.SetParent(mobsParent);
            instance.transform.position = sortedSpawnPoints[i].transform.position;
            instance.GetComponent<Patrol>().targets = sortedSpawnPoints[i].targets;
        }

        if (mob2SpawnAmount > mob2SpawnPoints.Length) mob2SpawnAmount = mob2SpawnPoints.Length;
        var sortedSpawnPoints2 = mob2SpawnPoints.OrderBy(a => Guid.NewGuid()).ToArray();

        for (int i = 0; i < mob2SpawnAmount; i++)
        {
            GameObject instance = Instantiate(Resources.Load("mob2", typeof(GameObject))) as GameObject;
            instance.transform.SetParent(mobsParent);
            instance.transform.position = sortedSpawnPoints2[i].transform.position;
            instance.GetComponent<DefendZone>().zone = sortedSpawnPoints2[i];
        }
    }
}
