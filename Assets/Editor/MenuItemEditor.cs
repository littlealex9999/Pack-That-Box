using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuItem))]
public class MenuItemEditor : Editor
{
    private void OnSceneGUI()
    {
        MenuItem item = (MenuItem)target;

        Vector3 center;
        if (Application.isPlaying) {
            center = item.startPosition;
        } else {
            center = item.transform.position + item.startingOffset;
        }

        Handles.DrawWireDisc(center, Vector3.up, item.range);
        Handles.DrawWireDisc(center, Vector3.right, item.range);
        Handles.DrawWireDisc(center, Vector3.forward, item.range);
    }
}
