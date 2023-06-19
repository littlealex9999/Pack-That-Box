using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GameManager.instance != null) {
            if (GUILayout.Button("Satisfy Customer") && GameManager.instance.customers.Count > 0) {
                GameManager.instance.RemoveCustomer(GameManager.instance.customers[0]);
            }
        }
    }
}
