using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource audioSource;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public AudioClip[] doorSounds;
    public AudioClip[] clearItemsSounds;
    public AudioClip[] greetingCustomerSounds;
    public AudioClip[] happyCustomerSounds;
    public AudioClip[] angryCustomerSounds;
    public AudioClip[] gameStartSounds;
    public AudioClip[] gameEndSounds;
}
