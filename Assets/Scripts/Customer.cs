using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    List<PackItem> requestedItems = new List<PackItem>();

    public List<PackItem> items { get { return requestedItems; } }

    void Update()
    {
        
    }

    public void AssignItems(List<PackItem> items)
    {
        requestedItems = items;
    }
}
