using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Customer))]
public class CustomerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = false;

        EditorGUILayout.FloatField("Starting Patience", serializedObject.FindProperty("startingPatience").floatValue);
        EditorGUILayout.FloatField("Patience", serializedObject.FindProperty("patience").floatValue);

        GUI.enabled = true;
    }
}
