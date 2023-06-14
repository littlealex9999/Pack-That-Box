using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    bool spawnedList;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (leaving && CheckDestinationReached()) {
            Destroy(gameObject);
        } else if (!spawnedList && CheckDestinationReached()) {
            CreateItemList();
            spawnedList = true;
        }
    }

    #region Items
    public void AssignItems(List<PackItem> items)
    {
        requestedItems = items;
    }

    public void SetItemRequestList(GameObject go)
    {
        itemRequestList = go;
    }

    void CreateItemList()
    {
        itemRequestList = Instantiate(itemRequestList, transform.position, Quaternion.identity);
        TextMeshProUGUI text = itemRequestList.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null && requestedItems.Count > 0) {
            text.text = requestedItems[0].itemName;

            for (int i = 1; i < requestedItems.Count; ++i) {
                text = Instantiate(text.gameObject, text.gameObject.transform.parent).GetComponent<TextMeshProUGUI>();
                text.text = requestedItems[i].itemName;
            }
        }
    }
    #endregion

    #region Navigation
    public void SetMoveTarget(Transform target)
    {
        agent.destination = target.position;
    }

    bool CheckDestinationReached()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }
    #endregion
}
