using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinking : MonoBehaviour
{
    public GameObject defaultEyes;
    public GameObject closedEyes;
    public GameObject happyEyes;
    public GameObject angryEyes;

    public float minEyesOpen = 2;
    public float maxEyesOpen = 6;

    public float minEyesClosed = 0.1f;
    public float maxEyesClosed = 0.2f;

    bool closed;
    float timer;

    [HideInInspector] public bool manualControl;

    public enum EyeState
    {
        Normal = default,
        Closed,
        Happy,
        Angry,
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (!manualControl && timer <= 0) {
            if (closed) {
                SetEyes(EyeState.Normal);
                closed = false;

                timer = Random.Range(minEyesOpen, maxEyesOpen);
            } else {
                SetEyes(EyeState.Closed);
                closed = true;

                timer = Random.Range(minEyesClosed, maxEyesClosed);
            }
        }
    }

    public void SetEyes(EyeState state)
    {
        defaultEyes.SetActive(false);
        closedEyes.SetActive(false);
        happyEyes.SetActive(false);
        angryEyes.SetActive(false);

        switch (state) {
            case EyeState.Normal:
                defaultEyes.SetActive(true);
                break;
            case EyeState.Closed:
                closedEyes.SetActive(true);
                break;
            case EyeState.Happy:
                happyEyes.SetActive(true);
                break;
            case EyeState.Angry:
                angryEyes.SetActive(true);
                break;
        }
    }
}
