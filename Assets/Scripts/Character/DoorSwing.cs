using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorSwing : MonoBehaviour
{
    Animator animator;
    [SerializeField, Layer] int targetLayer = 7;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == targetLayer) {
            animator.SetBool("Open", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == targetLayer) {
            animator.SetBool("Open", false);
        }
    }
}
