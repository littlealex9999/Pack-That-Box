using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackItem : MonoBehaviour
{
    [SerializeField] int ID; // items with the same ID will be considered the same for packing purposes

    public int itemID { get { return ID; } }
}
