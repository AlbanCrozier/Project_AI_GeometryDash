using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float offsetX = 5f;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x + offsetX,
            transform.position.y,
            transform.position.z);
    }
}
