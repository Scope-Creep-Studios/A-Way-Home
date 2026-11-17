using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoftlockShield : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField]
    private BoxCollider2D boxCollider;

    private bool playerNearby = false;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
    }

    private void Update()
    {
        if(gameManager.hasCollectible) {
            boxCollider.enabled = true;
        }
    }
}