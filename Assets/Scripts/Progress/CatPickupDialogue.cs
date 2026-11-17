using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatPickupDialogue : MonoBehaviour
{

    public DialogueSystem dialogueSystem;
    public NarrativeUI narrativeUI;

    private GameManager gameManager;

    [SerializeField]
    private BoxCollider2D boxCollider;

    private bool playerNearby = false;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            DialogueSystem.DialogueEntry[] dialogue = dialogueSystem.GetDialogue("Item", "CollectibleCat", "Level1");
            narrativeUI.ShowNarrative(dialogue);
            boxCollider.enabled = false;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}