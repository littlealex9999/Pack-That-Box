using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    [Header("Playtime UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;

    [Header("Main Menu UI")]
    public TextMeshProUGUI scoreboardText;

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

        #region Menu Text
        if (GameManager.instance != null && GameManager.instance.currentGameState == GameManager.GameState.Menu) {
            if (scoreboardText) {
                StringBuilder sbText = new StringBuilder();
                for (int i = 0; i < GameManager.instance.currentScoreboard.Length; ++i) {
                    sbText.AppendLine(GameManager.instance.currentScoreboard[i].ToString());
                }
                sbText.Remove(sbText.Length - 1, 1);

                scoreboardText.text = sbText.ToString();
            }
        }
        #endregion
    }
}
