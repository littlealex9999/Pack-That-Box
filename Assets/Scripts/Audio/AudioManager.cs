using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public AudioClip mainSong;
    public AudioClip[] doorSounds;
}
