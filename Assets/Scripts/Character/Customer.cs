using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    List<PackItem> requestedItems = new List<PackItem>();

    public List<PackItem> items { get { return requestedItems; } }

    NavMeshAgent agent;
    Animator animator;
    AudioSource audioSource;

    [HideInInspector] public int assignedWaitIndex = -1;
    [HideInInspector] public bool leaving = false;

    WaitingLocation waitingLocation;
    GameObject itemRequestList;
    GameObject boxPrefab;
    bool spawnedList;
    public Transform leavingLocation;

    [SerializeField, HideInInspector] float startingPatience;
    [SerializeField, HideInInspector] float patience;

    [SerializeField] Image patienceMeter;

    [Space, SerializeField] Transform headLocation;
    [SerializeField] Transform earsLocation;
    [SerializeField] Transform neckLocation;

    [Space, SerializeField] float idleSpeedMultiplierMin = 1;
    [SerializeField] float idleSpeedMultiplierMax = 5;
    [SerializeField] AnimationCurve idleSpeedCurve;
    [SerializeField, Range(0, 1)] float angryThreshold;
    [SerializeField] GameObject[] activateOnAngry;

    public enum AccessoryTypes
    {
        Head,
        Ears,
        Neck,
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        foreach (GameObject go in activateOnAngry) {
            go.SetActive(false);
        }

        if (patienceMeter != null) patienceMeter.transform.parent.gameObject.SetActive(false);
    }

    void Update()
    {
        if (leaving && CheckDestinationReached()) { // we delete this gameObject on reaching the leaving point
            Destroy(gameObject);
        } else if (!spawnedList && CheckDestinationReached()) { // this will run once upon reaching the counter for the first time
            SpawnItems();
            spawnedList = true;

            patienceMeter.transform.parent.gameObject.SetActive(true);
        } else if (spawnedList && leavingLocation == null) { // we only spawn a list when we reach the counter, so this check is essentially ensuring we are at the counter
            patience -= Time.deltaTime;
            if (patienceMeter) patienceMeter.fillAmount = patience / startingPatience;

            gameObject.transform.LookAt(Camera.main.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);

            if (patience <= 0) {
                GameManager.instance.RemoveCustomer(this, true);
            }
        }

        if (!CheckDestinationReached()) { // we should be walking
            if (animator) {
                SetWalkingAnimation(true);
            }
        } else {
            float patienceEval = idleSpeedCurve.Evaluate(1 - patience / startingPatience);

            if (animator) {
                SetWalkingAnimation(false, Mathf.Lerp(idleSpeedMultiplierMin, idleSpeedMultiplierMax, patienceEval));
            }

            if (patienceEval > angryThreshold) {
                foreach (GameObject go in activateOnAngry) {
                    go.SetActive(true);
                }
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

    public void SetMoveSpeed(float speed)
    {
        agent.speed = speed;
    }

    bool CheckDestinationReached()
    {
        if (agent.isOnNavMesh) return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        else return false;
    }
    #endregion

    #region Accessories
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
    #endregion

    #region animation
    void SetWalkingAnimation(bool walking, float patience = 1)
    {
        if (walking) {
            animator.SetBool("Walking", true);
        } else {
            animator.SetBool("Walking", false);
            animator.SetFloat("Patience", patience);
        }
    }

    void SetRequestFinishAnimationAudio(bool success)
    {
        if (success) {
            if (animator != null) animator.SetTrigger("Success");
            if (audioSource != null) audioSource.PlayOneShot(ArrayHelper<AudioClip>.GetRandomElement(AudioManager.instance.happyCustomerSounds));
        } else if (animator) {
            if (animator != null) animator.SetTrigger("Fail");
            if (audioSource != null) audioSource.PlayOneShot(ArrayHelper<AudioClip>.GetRandomElement(AudioManager.instance.angryCustomerSounds));
        }
    }
    #endregion

    public void SetPatience(float time)
    {
        startingPatience = time;
        patience = time;
    }

    public void EndRequest(bool failed)
    {
        SetRequestFinishAnimationAudio(!failed);
        patienceMeter.transform.parent.gameObject.SetActive(false);
    }

    public void LeaveNow()
    {
        leaving = true;

        GameManager.instance.customers.Remove(this);
        SetMoveTarget(leavingLocation);
    }
}
