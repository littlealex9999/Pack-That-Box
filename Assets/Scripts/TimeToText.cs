using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeToText : MonoBehaviour
{
    TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // only change this bit of text while the game is in the middle of being played
        if (GameManager.instance != null && GameManager.instance.currentGameState == GameManager.GameState.Playing) {
            int mins = (int)(GameManager.instance.remainingTime / 60);
            int secs = (int)(GameManager.instance.remainingTime % 60);
            text.text = mins.ToString() + ":" + secs.ToString();
        }
    }
}
