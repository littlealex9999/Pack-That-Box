using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;

    void Update()
    {
        #region Playing Text
        if (GameManager.instance != null && GameManager.instance.currentGameState == GameManager.GameState.Playing) {
            if (timeText) {
                int mins = (int)(GameManager.instance.remainingTime / 60);
                int secs = (int)(GameManager.instance.remainingTime % 60);
                timeText.text = mins.ToString() + ":" + secs.ToString();
            }

            if (scoreText) {
                scoreText.text = GameManager.instance.currentScore.ToString("0");
            }
        }
        #endregion
    }
}
