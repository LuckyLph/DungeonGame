using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    [SerializeField] private int spawnAmount;
    [SerializeField] private Transform[] locations;
    [SerializeField] private Transform starsParent;

    void Start()
    {
        if (spawnAmount > locations.Length) spawnAmount = locations.Length;
        Transform[] sortedLocations = locations.OrderBy(a => Guid.NewGuid()).ToArray();

        for (int i = 0; i < spawnAmount; i++)
        {
            GameObject instance = Instantiate(Resources.Load("Star", typeof(GameObject))) as GameObject;
            instance.transform.SetParent(starsParent);
            instance.transform.position = sortedLocations[i].position;
        }
    }
}
