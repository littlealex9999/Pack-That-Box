using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accessory : MonoBehaviour
{
    [SerializeField] Customer.AccessoryTypes type;
    public Customer.AccessoryTypes accessoryType { get { return type; } }
}
