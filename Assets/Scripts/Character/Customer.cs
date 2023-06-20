using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    WaitingLocation waitingLocation;
    GameObject itemRequestList;
    GameObject boxPrefab;
    bool spawnedList;

    float startingPatience;
    float patience;

    Image patienceMeter;

    [SerializeField] Transform headLocation;
    [SerializeField] Transform earsLocation;
    [SerializeField] Transform neckLocation;

    public enum AccessoryTypes
    {
        Head,
        Ears,
        Neck,

        Count
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patienceMeter = GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (leaving && CheckDestinationReached()) { // we delete this gameObject on reaching the leaving point
            Destroy(gameObject);
        } else if (!spawnedList && CheckDestinationReached()) { // this will run once upon reaching the counter for the first time
            SpawnItems();
            spawnedList = true;
        } else if (spawnedList) { // we only spawn a list when we reach the counter, so this check is essentially ensuring we are at the counter
            patience -= Time.deltaTime;
            if (patienceMeter) patienceMeter.fillAmount = patience / startingPatience;

            gameObject.transform.LookAt(Camera.main.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);

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

    public void SetSpawnItems(GameObject list, GameObject box, WaitingLocation spawnLocation)
    {
        itemRequestList = list;
        boxPrefab = box;
        waitingLocation = spawnLocation;
    }

    void SpawnItems()
    {
        // spawn item list and set the relevant text
        itemRequestList = Instantiate(itemRequestList, waitingLocation.listDropLocation.position, waitingLocation.listDropLocation.rotation);
        TextMeshProUGUI text = itemRequestList.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null && requestedItems.Count > 0) {
            text.text = requestedItems[0].itemName;

            for (int i = 1; i < requestedItems.Count; ++i) {
                text = Instantiate(text.gameObject, text.gameObject.transform.parent).GetComponent<TextMeshProUGUI>();
                text.text = requestedItems[i].itemName;
            }
        }

        GameManager.instance.AddObjectToClearList(itemRequestList);

        // spawn the box
        GameManager.instance.AddObjectToClearList(Instantiate(boxPrefab, waitingLocation.boxDropLocation.position, waitingLocation.boxDropLocation.rotation));
    }
    #endregion

    #region Navigation
    public void SetMoveTarget(Transform target)
    {
        agent.SetDestination(target.position);
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

    public void AttachAccessories(GameObject[] accessories)
    {
        foreach (GameObject accessory in accessories) {
            if (accessory == null) continue;

            Transform target = GetAccessoryTransform(accessory.GetComponent<Accessory>().accessoryType); // get the target transform from the accessory type
            if (target.childCount > 0) continue; // do not equip multiple accessories
            Instantiate(accessory, target);
        }
    }

    Transform GetAccessoryTransform(AccessoryTypes type)
    {
        switch (type) {
            case AccessoryTypes.Head:
                return headLocation;
            case AccessoryTypes.Ears:
                return earsLocation;
            case AccessoryTypes.Neck:
                return neckLocation;
            default:
                return null;
        }
    }
}
