using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string itemName;
    public string gameState;

    public DialogueSystem dialogueSystem;
    public NarrativeUI narrativeUI;

    private bool playerNearby = false;

    public GameObject interactPrompt;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    private void Collect()
    {
        DialogueSystem.DialogueEntry[] dialogue = dialogueSystem.GetDialogue(
            "Item",
            itemName,
            gameState
        );

        narrativeUI.ShowNarrative(dialogue);

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        gameManager.hasCollectible = true;

        Destroy(gameObject);
    }
}