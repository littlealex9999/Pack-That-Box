using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    List<PackItem> requestedItems = new List<PackItem>();

    public List<PackItem> items { get { return requestedItems; } }

    NavMeshAgent agent;

    [HideInInspector] public int assignedWaitIndex = -1;
    [HideInInspector] public bool leaving = false;

    GameObject itemRequestList;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (leaving && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance <= agent.stoppingDistance) {
            Destroy(gameObject);
        }   
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
