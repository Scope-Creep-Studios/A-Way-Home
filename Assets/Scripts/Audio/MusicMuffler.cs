using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(AudioLowPassFilter))]
public class MusicMuffler : MonoBehaviour
{
    public float openFreq = 22000f;
    public float smoothTime = 0.3f; // seconds

    AudioLowPassFilter lpf;
    float target, velocity;

    void Awake()
    {
        lpf = GetComponent<AudioLowPassFilter>();
        lpf.cutoffFrequency = target = openFreq;
    }

    public void SetFilter(bool on, float closedFreq) => target = on ? closedFreq : openFreq;

    void Update()
    {
        lpf.cutoffFrequency = Mathf.SmoothDamp(lpf.cutoffFrequency, target, ref velocity, smoothTime, Mathf.Infinity,Time.unscaledDeltaTime);
    }
}