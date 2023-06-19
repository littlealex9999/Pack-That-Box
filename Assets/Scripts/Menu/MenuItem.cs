using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public enum MenuFunction
    {
        Start,
        Stop,
        Quit,
    }

    public MenuFunction function;

    /// <summary>
    /// Performs the action this item is meant to do
    /// </summary>
    public void DoFunction()
    {
        switch (function) {
            case MenuFunction.Start:
                StartGame();
                break;
            case MenuFunction.Stop:
                StopGame();
                break;
            case MenuFunction.Quit:
                QuitGame();
                break;
            default:
                break;
        }
    }

    void StartGame()
    {
        GameManager.instance.StartGame();
    }

    void StopGame()
    {
        GameManager.instance.EndGame();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
