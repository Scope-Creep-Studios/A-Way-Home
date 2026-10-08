using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class door : MonoBehaviour
{
    public string doorName;
    public string gameState;
    public string nextSceneName;

    public DialogueSystem dialogueSystem;
    public NarrativeUI narrativeUI;

    private Collider2D doorCollider;

    void Start()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();

            if (gameManager.hasCollectible)
            {
                doorCollider.enabled = false;

                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                DialogueSystem.DialogueEntry[] dialogue = dialogueSystem.GetDialogue(
                    "Door",
                    doorName,
                    gameState
                );

                narrativeUI.ShowNarrative(dialogue);
            }
        }
    }
}