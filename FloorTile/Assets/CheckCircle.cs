using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCircle : MonoBehaviour
{
    public bool isCollision = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Sphere")
        {
            isCollision = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Sphere")
        {
            isCollision = false;
        }
    }
}
