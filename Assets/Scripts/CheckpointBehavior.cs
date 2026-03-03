using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehavior : MonoBehaviour
{
    private bool wasActivated = false;


    // Start is called before the first frame update
    void Start()
    {
        wasActivated = false;
    }

    public void Activate()
    {
        wasActivated = true;
    }

    public bool IsActivated()
    {
        return wasActivated;
    }
}
