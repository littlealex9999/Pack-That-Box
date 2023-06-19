using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBox : MonoBehaviour
{
    [SerializeField] float selectTime = 3;
    float timer;

    MenuItem heldItem;

    private void Update()
    {
        if (heldItem != null) {
            timer -= Time.deltaTime;

            if (timer <= 0) {
                heldItem.DoFunction();
                EndSelectMenuItem();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (heldItem == null && other.tag == "Menu Item") {
            BeginSelectMenuItem(other.GetComponent<MenuItem>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (heldItem != null && other.gameObject == heldItem.gameObject) {
            EndSelectMenuItem();
        }
    }

    void BeginSelectMenuItem(MenuItem item)
    {
        timer = selectTime;
        heldItem = item;
    }

    void EndSelectMenuItem()
    {
        heldItem = null;
    }
}
