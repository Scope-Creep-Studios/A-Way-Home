using System.Collections.Generic;
using UnityEngine;

public class MusicMuffler : MonoBehaviour
{
    [SerializeField] private AudioLowPassFilter targetFilter;

    public float open = 20000f;
    public float smoothTime = 0.3f;

    targetFilter.setcut
}