using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float speed;
    private GameObject target;
    public bool CameraEnabled { get; set; }

    private void Update()
    {
        if (CameraEnabled)
        {
            Vector3 nextPosition = Vector3.Lerp(transform.position, target.transform.position, speed * Time.deltaTime);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, Camera.main.transform.position.z);
        }
    }

    public void EnableCamera(GameObject target)
    {
        this.target = target;
        CameraEnabled = true;
    }
}
