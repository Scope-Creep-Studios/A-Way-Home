using UnityEngine;

public class Lever : MonoBehaviour
{
    private SpriteRenderer lever_color;
    private bool playerNearby = false;
    public GameObject interactPrompt;
    public GhostWall ghost_wall;
    private bool isPulled = false;

    void Start()
    {
        lever_color = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPulled)
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
        if (playerNearby && !isPulled && Input.GetKeyDown(KeyCode.E))
        {
            Turn();
        }
    }

    private void Turn()
    {
        isPulled = true;
        interactPrompt.SetActive(false);
        lever_color.color = Color.red;
        GameManager gameManager = FindFirstObjectByType<GameManager>(); //will need to change the orientation of the lever and put animation here
        gameManager.isPulled = true;
        ghost_wall.MakeGhost(); //calling from ghostwall 
    }
}