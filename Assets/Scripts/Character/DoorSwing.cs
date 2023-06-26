using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorSwing : MonoBehaviour
{
    Animator animator;
    [SerializeField, Layer] int targetLayer = 7;

    int peopleInside = 0;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == targetLayer) {
            ++peopleInside;

            if (peopleInside > 0) {
                animator.SetBool("Open", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == targetLayer) {
            --peopleInside;

            if (peopleInside <= 0) {
                animator.SetBool("Open", false);
            }
        }
    }
}
