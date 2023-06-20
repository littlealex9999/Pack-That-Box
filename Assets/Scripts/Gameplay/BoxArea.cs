using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Box") GameManager.instance.AddPreparedBox(other.GetComponent<Box>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Box") GameManager.instance.RemovePreparedBox(other.GetComponent<Box>());
    }
}
