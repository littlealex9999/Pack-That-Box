using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WindowShopper : MonoBehaviour
{
    List<Transform> windowShopLocations;
    Transform leaveLocation;

    public int assignedIndex = -1;

    float stayTime = 4;
    float timeToLeave = 15;

    float waitingTimer;

    NavMeshAgent agent;
    Animator animator;

    bool leaving;
    bool waiting;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetFloat("Patience", 1);
    }

    private void Update()
    {
        timeToLeave -= Time.deltaTime;

        if (leaving && CheckDestinationReached()) {
            Destroy(gameObject); // we have reached the place where we should despawn
        } else if (!waiting && CheckDestinationReached()) {
            waiting = true;
            waitingTimer = stayTime;
        } else if (waiting) {
            waitingTimer -= Time.deltaTime;
            if (waitingTimer <= 0) {
                waiting = false;
                GameManager.instance.AssignWindowShopperLocation(this);
            }
        }

        if (!leaving && timeToLeave <= 0) {
            leaving = true;
            waiting = false;
            GoToLocation(leaveLocation.position);

            GameManager.instance.WindowShopperLeave(this);
        }

        if (animator != null) {
            if (CheckDestinationReached()) {
                animator.SetBool("Walking", false);
            } else {
                animator.SetBool("Walking", true);
            }
        }
    }

    bool CheckDestinationReached()
    {
        if (agent.isOnNavMesh) return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        else return false;
    }

    public void GoToLocation(Vector3 position)
    {
        agent.SetDestination(position);
    }

    public void AssignTimes(float stayDuration, float waitDuration)
    {
        timeToLeave = stayDuration;
        stayTime = waitDuration;
    }

    public void AssignLocations(List<Transform> targetLocations, Transform exitLocation)
    {
        windowShopLocations = targetLocations;
        leaveLocation = exitLocation;

        agent = GetComponent<NavMeshAgent>();
        GameManager.instance.AssignWindowShopperLocation(this);
    }
}
