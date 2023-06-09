using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public enum MenuFunction
    {
        Start,
        Stop,
        Restart,
        ClearItems,
        Quit,
    }

    public MenuFunction function;

    Vector3 startingPos;
    [SerializeField] Vector3 offset;
    [SerializeField] float maxDistance = 1;

    Rigidbody rb;

    public Vector3 startPosition { get { return startingPos; } }
    public Vector3 startingOffset { get { return offset; } }
    public float range { get { return maxDistance; } }

    private void Start()
    {
        startingPos = transform.position + startingOffset;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if ((transform.position - startingPos).sqrMagnitude > maxDistance * maxDistance) {
            ResetPosition();
        }
    }

    void ResetPosition()
    {
        rb.velocity = Vector3.zero;

        rb.isKinematic = true;
        rb.position = startingPos;
        rb.isKinematic = false;
    }    

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
            case MenuFunction.Restart:
                StopGame();
                StartGame();
                break;
            case MenuFunction.ClearItems:
                ClearItems();
                break;
            case MenuFunction.Quit:
                QuitGame();
                break;
            default:
                break;
        }

        ResetPosition();
    }

    void StartGame()
    {
        GameManager.instance.StartGame();
    }

    void StopGame()
    {
        GameManager.instance.EndGame();
    }

    void ClearItems()
    {
        GameManager.instance.ClearItems();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
