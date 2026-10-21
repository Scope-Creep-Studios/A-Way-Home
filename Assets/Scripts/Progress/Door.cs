using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string doorName;
    public string gameState;
    public string nextSceneName;

    public DialogueSystem dialogueSystem;
    public NarrativeUI narrativeUI;

    public GameObject interactText;

    private bool playerNearby = false;

    private void Start()
    {
        if (interactText != null)
        {
            interactText.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = true;

            if (interactText != null)
            {
                interactText.SetActive(true);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;

            if (interactText != null)
            {
                interactText.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (narrativeUI != null &&
                narrativeUI.narrativeBox.activeSelf)
            {
                return;
            }

            GameManager gameManager =
                FindFirstObjectByType<GameManager>();

            if (gameManager != null && gameManager.hasCollectible)
            {
                SceneManager.LoadScene(nextSceneName);
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