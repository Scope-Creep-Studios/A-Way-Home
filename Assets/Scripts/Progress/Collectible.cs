using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour
{
    public string itemName;
    public string gameState;

    // Name used by the Journal
    public string journalItemName;

    public DialogueSystem dialogueSystem;
    public NarrativeUI narrativeUI;
    public JournalUI journalUI;

    private bool playerNearby = false;
    private bool isCollecting = false;

    public GameObject interactPrompt;

    [Header("Fly Animation")]
    public float flyDuration = 0.6f;
    public float arcHeight = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollecting)
        {
            playerNearby = true;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerNearby && !isCollecting && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollecting = true;
        playerNearby = false;
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        DialogueSystem.DialogueEntry[] dialogue = dialogueSystem.GetDialogue(
            "Item",
            itemName,
            gameState
        );

        narrativeUI.ShowNarrative(dialogue);

        // Add the collected item to the Journal
        GameManager gameManager = GameManager.Instance;
        if (itemName == "Collectible1" || itemName == "Collectible3")
        {
            gameManager.hasCollectible = true;
        }
        
        JournalManager.Instance.AddCollectedItem(journalItemName);
        if (journalUI != null)
        {
            journalUI.ShowNewItemNotification();
        }

        // Disable the collider during animation
        Collider2D itemCollider = GetComponent<Collider2D>();

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        // Start flying animation
        if (journalUI != null)
        {
            StartCoroutine(FlyToJournal());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FlyToJournal()
    {
        Vector3 startPosition = transform.position;

        // Get Journal Icon's screen position
        Vector3 targetScreenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                journalUI.journalIcon.position
            );

        // Convert screen position to world position
        float cameraDepth =
            Camera.main.WorldToScreenPoint(startPosition).z;

        Vector3 targetPosition =
            Camera.main.ScreenToWorldPoint(
                new Vector3(
                    targetScreenPosition.x,
                    targetScreenPosition.y,
                    cameraDepth
                )
            );

        Vector3 originalScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / flyDuration);

            // Move towards Journal Icon
            Vector3 currentPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            // Add a small upward arc
            currentPosition +=
                Vector3.up * arcHeight * Mathf.Sin(t * Mathf.PI);

            transform.position = currentPosition;

            // Gradually shrink the collectible
            transform.localScale = Vector3.Lerp(
                originalScale,
                Vector3.zero,
                t
            );

            yield return null;
        }

        // Play Journal Icon animation
        journalUI.PlayJournalIconAnimation();

        // Destroy the collectible after reaching the icon
        Destroy(gameObject);
    }
}