using System;
using System.Linq;
using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    [SerializeField] private int spawnAmount;
    [SerializeField] private Transform[] locations;
    [SerializeField] private Transform keysParent;

    void Start()
    {
        if (spawnAmount > locations.Length) spawnAmount = locations.Length;
        Transform[] sortedLocations = locations.OrderBy(a => Guid.NewGuid()).ToArray();

        for (int i = 0; i < spawnAmount; i++)
        {
            GameObject instance = Instantiate(Resources.Load("Key", typeof(GameObject))) as GameObject;
            instance.transform.SetParent(keysParent);
            instance.transform.position = sortedLocations[i].position;
        }
    }
}
