using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private CharacterSwap _swap;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Respawn girl))
        {
            girl.Die();   
        }
        else if (other.TryGetComponent(out CompanionFollow _))
        {
            _swap.RecallGhost();   
        }
    }
}