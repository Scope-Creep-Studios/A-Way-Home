using System.Collections.Generic;
using UnityEngine;
//class that plays a sound from a list of sounds, using it's own clip list and audio source. allows me to 
public class SoundGroup : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private SoundCategory category;
    [SerializeField] private AudioClip[] clips;
    //enum for easy access 
    public SoundCategory Category => category;

    public bool TryPlayRandom()
    {
        int index = Random.Range(0,clips.Length);
        Debug.Log(clips.Length + " " + index);
        if (source.isPlaying){return false;}
        source.clip=clips[index];
        source.Play();
        return true;
    }
    public bool TryPlay(int index)
    {
        
        if (source.isPlaying){return false;}
        source.clip=clips[index];
        source.Play();
        return true;
    }
    public void FPlay(int index)
    {
        
        source.clip=clips[index];
        source.Play();
    }
      public void FPlayRandom()
    {
        source.clip=clips[Random.Range(0,clips.Length)];
        source.Play();
    }
}