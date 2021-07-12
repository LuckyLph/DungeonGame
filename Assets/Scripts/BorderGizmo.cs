using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderGizmo : MonoBehaviour
{
    [SerializeField] private int borderWidth;
    [SerializeField] private int borderHeigth;

    private Rect borderRect;

    private void Awake()
    {
        borderRect = new Rect(new Vector2(0, 0), new Vector2(borderWidth, borderHeigth));
    }

    void OnDrawGizmos()
    {
        // Green
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        DrawRect();
    }
    void OnDrawGizmosSelected()
    {
        // Orange
        Gizmos.color = new Color(1.0f, 0.5f, 0.0f);
        DrawRect();
    }

    public void DrawRect()
    {
        Gizmos.DrawWireCube(new Vector3(borderRect.center.x, borderRect.center.y, 0.01f), new Vector3(borderRect.size.x, borderRect.size.y, 0.01f));
    }
}
