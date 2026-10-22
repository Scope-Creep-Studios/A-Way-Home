using UnityEngine;

public class destroywallghost : MonoBehaviour
{
    public GameObject interactPrompt;
    private bool ghostNearby = false; //checks so the ghost can only interactwith it

    private void OnTriggerEnter2D(Collider2D other)
    {
        interactPrompt.SetActive(true);
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerB"))
        {
            ghostNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        interactPrompt.SetActive(false);
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerB"))
        {
            ghostNearby = false;
        }
    }

    private void Update()
    {
        if (ghostNearby && Input.GetKeyDown(KeyCode.E))
        {
            interactPrompt.SetActive(false);
            Destroy(gameObject, 1f); //destroying the wall after a second 
        }
    }
}