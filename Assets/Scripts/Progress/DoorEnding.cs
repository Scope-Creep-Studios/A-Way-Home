using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorEnding : MonoBehaviour
{
    public string doorName;
    public string gameState;

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
        if(gameManager.hasCollectible) {
            boxCollider.enabled = false;
        }
        if (playerNearby)
        {
            if (narrativeUI != null &&
                narrativeUI.narrativeBox.activeSelf)
            {
                return;
            }


            if (gameManager != null && gameManager.hasCollectible)
            {
                
            }
            else
            {
                DialogueSystem.DialogueEntry[] dialogue =
                    dialogueSystem.GetDialogue(
                        "Door", doorName, gameState);

                narrativeUI.ShowNarrative(dialogue);
            }
        }
    }
}