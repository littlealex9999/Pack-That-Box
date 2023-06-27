using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorSwing : MonoBehaviour
{
    Animator animator;
    [SerializeField, Layer] int targetLayer = 7;

    AudioSource audioSource;

    int peopleInside = 0;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == targetLayer) {
            ++peopleInside;

            if (peopleInside > 0) {
                animator.SetBool("Open", true);
                audioSource.PlayOneShot(ArrayHelper<AudioClip>.GetRandomElement(AudioManager.instance.doorSounds));
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
