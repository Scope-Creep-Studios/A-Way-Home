using System.Collections.Generic;
using UnityEngine;

public class MusicMuffler : MonoBehaviour
{
    [SerializeField] private AudioLowPassFilter targetFilter;

    public void SetMuffle(bool isMuffled)
    {
        if (targetFilter != null)
        {
            targetFilter.enabled = isMuffled;
        }
    }
}