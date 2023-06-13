using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    List<PackItem> requestedItems = new List<PackItem>();

    public List<PackItem> items { get { return requestedItems; } }

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        
    }

    public void AssignItems(List<PackItem> items)
    {
        requestedItems = items;
    }

    public void SetMoveTarget(Transform target)
    {
        agent.destination = target.position;
    }
}
