using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scoreboard", order = 0)]
public class Score : ScriptableObject
{
    [SerializeField] float[] scores = new float[10];
    public int Length { get { return scores.Length; } }

    bool loadedProperly;
    public bool Loaded { get { return loadedProperly; } }

    public void AddScore(float score)
    {
        for (int i = 0; i < scores.Length; ++i) {
            if (score > scores[i]) {
                for (int j = scores.Length - 1; j > i; --j) {
                    scores[j + 1] = scores[j]; // shuffle all scores down one
                }

                scores[i] = score;
            }
        }
    }

    public float this[int i]
    {
        get {
            return scores[i];
        }
    }

    public void SaveScores(string filename)
    {

    }

    public void LoadScores(string filename)
    {

    }
}
