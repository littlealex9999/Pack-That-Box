using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script belongs on "Boxes" which check if items enter them and then can be used to find what items they hold
/// </summary>
public class Box : MonoBehaviour
{
    [HideInInspector] public List<PackItem> itemsInBox = new List<PackItem>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item") {
            itemsInBox.Add(other.GetComponent<PackItem>());
        }
    }
}
