using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustSortingLayer : MonoBehaviour
{
    private GameObject player;
    private SpriteRenderer sprite;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (transform.position.y < player.transform.position.y)
        {
            sprite.sortingLayerName = "Entities2";
        }
        else if (transform.position.y > player.transform.position.y)
        {
            sprite.sortingLayerName = "Entities";
        }
    }
}
