using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySoundGenerator : MonoBehaviour
{
    public List<AudioClip> entityFootsteps = new List<AudioClip>();
    public AudioSource audioSource;
    public AudioSource globalAudioSource;
    public AudioClip growlClip;

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.C))
        {
            PlayGrowl();
        }*/
    }

    public void PlayGrowl()
    {
        globalAudioSource.clip = growlClip;
        globalAudioSource.volume = 0.1f;
        globalAudioSource.Play();
    }    
}
