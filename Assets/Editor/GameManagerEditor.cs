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
            if (GameManager.instance.currentGameState == GameManager.GameState.Menu) {
                if (GUILayout.Button("Start Game")) {
                    GameManager.instance.StartGame();
                }
            } else if (GameManager.instance.currentGameState == GameManager.GameState.Playing) {
                if (GUILayout.Button("Satisfy Customer") && GameManager.instance.customers.Count > 0) {
                    GameManager.instance.RemoveCustomer(GameManager.instance.customers[0]);
                }
            }

            if (GUILayout.Button("Clear Items")) {
                GameManager.instance.ClearItems();
            }
        }
    }
}
