using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    List<PackItem> requestedItems = new List<PackItem>();

    public List<PackItem> items { get { return requestedItems; } }

    NavMeshAgent agent;

    [HideInInspector] public int assignedWaitIndex = -1;
    [HideInInspector] public bool leaving = false;

    Transform listSpawnLocation;
    GameObject itemRequestList;
    bool spawnedList;

    float startingPatience;
    float patience;

    Image patienceMeter;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (leaving && CheckDestinationReached()) { // we delete this gameObject on reaching the leaving point
            Destroy(gameObject);
        } else if (!spawnedList && CheckDestinationReached()) { // this will run once upon reaching the counter for the first time
            CreateItemList();
            spawnedList = true;
        } else if (spawnedList) { // we only spawn a list when we reach the counter, so this check is essentially ensuring we are at the counter
            patience -= Time.deltaTime;
            if (patience <= 0) {
                GameManager.instance.RemoveCustomer(this, true);
                spawnedList = false; // ensures this block only executes once
            }
        }
    }

    #region Items
    public void AssignItems(List<PackItem> items)
    {
        requestedItems = items;
    }

    public void SetItemRequestList(GameObject go, Transform spawnLocation)
    {
        itemRequestList = go;
        listSpawnLocation = spawnLocation;
    }

    void CreateItemList()
    {
        itemRequestList = Instantiate(itemRequestList, listSpawnLocation.position, listSpawnLocation.rotation);
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

    public void SetPatience(float time)
    {
        startingPatience = time;
        patience = time;
    }
}
