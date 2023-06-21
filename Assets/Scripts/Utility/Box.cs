using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script belongs on "Boxes" which check if items enter them and then can be used to find what items they hold
/// </summary>
public class Box : MonoBehaviour
{
    [HideInInspector] public List<PackItem> itemsInBox = new List<PackItem>();

    public void ValidateItems()
    {
        for (int i = 0; i < itemsInBox.Count; ++i) {
            if (itemsInBox[i] == null) {
                itemsInBox.RemoveAt(i);
                --i;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item") {
            PackItem item = other.GetComponent<PackItem>();

            itemsInBox.Add(item);
            item.transform.SetParent(transform, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Item") {
            PackItem item = other.GetComponent<PackItem>();

            itemsInBox.Remove(item);
            item.transform.SetParent(null, true);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance.preparedBoxes.Contains(this)) GameManager.instance.RemovePreparedBox(this);
    }
}
