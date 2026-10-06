using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
        private static AudioController _instance;
    public static AudioController Instance {get{return _instance;}}

    private AudioSource AudioSource;

    [SerializeField] private AudioClip[] footsteps;
    void awake(){
        _instance = this;
    }
    public void playRandomFootstep()
    {
       // AudioSource.playOneShot(footsteps[Random.Range(0, audioClips.Length)]);
    }
}
