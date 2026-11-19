using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCat : MonoBehaviour
{

    private GameManager gameManager;

    [SerializeField]
    private CharacterSwap characterSwap;

    [SerializeField]
    private BoxCollider2D boxCollider;

    private bool playerNearby = false;

    [SerializeField]
    private GameObject self;
    [SerializeField]
    private GameObject ring;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        boxCollider = GetComponent<BoxCollider2D>();
        ring.SetActive(false);
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ring.SetActive(true);
            playerNearby = true;
            characterSwap.RemoveGhost();
            Destroy(self);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    private void Update()
    {
    
    }
}
