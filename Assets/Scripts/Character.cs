using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharState {Controlled, Idle, Following, Platform, Despawned}

[RequireComponent(typeof(PlatformerController))]

public class Character : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject _bodyVisual;
    [SerializeField] private GameObject _platformVisual; //Only for the ghost.

    [Header("Platform form (Ghost only.)")]
    [SerializeField] private BoxCollider2D _platformCollider;
    [SerializeField] private string _platformLayerName = "Ground";

    [Header("Abilities")]
    [SerializeField] private MonoBehaviour[] _abilities;
    

    public CharState State {get; private set;}
    public BoxCollider2D PlatformCollider => _platformCollider;
    public event Action<Character, CharState> StateChanged;

    private PlatformerController _controller;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _bodyCollider;
    private int _homeLayer;
    private int _platformLayer;

    private void Awake()
    {
        _controller = GetComponent<PlatformerController>();
        _rb = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<CapsuleCollider2D>();
        _homeLayer = gameObject.layer;
        _platformLayer = LayerMask.NameToLayer(_platformLayerName);
    }

    public void Enter(CharState s)
    {
        State = s;

        bool isPlatform = s == CharState.Platform;
        bool runsMotor = s == CharState.Controlled || s == CharState.Idle;
        bool inWorld = runsMotor || isPlatform;
        bool isVisible = s != CharState.Despawned;

        // physics body
        _rb.bodyType = isPlatform ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
        if (!runsMotor && !isPlatform) _rb.velocity = Vector2.zero;
        _rb.simulated = inWorld;
        gameObject.layer = isPlatform ? _platformLayer : _homeLayer;

        // collider live
        _bodyCollider.enabled = !isPlatform;
        if (_platformCollider != null) _platformCollider.enabled = isPlatform;

        // shown visual
        if (_bodyVisual != null) _bodyVisual.SetActive(isVisible && !isPlatform);
        if (_platformVisual != null) _platformVisual.SetActive(isPlatform);

        // movement and abilities
        _controller.enabled = runsMotor;
        _controller.HasControl = s == CharState.Controlled;
        foreach (MonoBehaviour ability in _abilities) 
            if (ability != null) 
            {
                ability.enabled = s == CharState.Controlled;
            }
        
        // tell any listeners that the state has changed
        StateChanged?.Invoke(this, s);
    }
}
