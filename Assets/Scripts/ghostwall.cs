using UnityEngine;

public class GhostWall : MonoBehaviour
{
    private SpriteRenderer ghost_wall_color;

    void Start()
    {
        ghost_wall_color = GetComponent<SpriteRenderer>();
    }

    public void MakeGhost()
    {
        gameObject.layer = LayerMask.NameToLayer("GhostWall");

        Color temp_color = Color.yellow;
        temp_color.a = 0.5f; //this is temporary arrangeemnt to show the player that something changed, will probably have to chnage it to the tile blocks we have later on
        ghost_wall_color.color = temp_color;
    }
}