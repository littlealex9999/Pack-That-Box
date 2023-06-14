using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteTrigger : MonoBehaviour
{
    [SerializeField] string[] deleteTags = new string[] { "Item", "Deletable" };

    private void OnTriggerEnter(Collider other)
    {
        foreach (string s in deleteTags) {
            if (other.tag == s) {
                Destroy(other.gameObject);
            }
        }
    }
}
