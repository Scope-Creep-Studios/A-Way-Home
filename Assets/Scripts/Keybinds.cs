using UnityEngine;

[CreateAssetMenu(fileName = "Keybinds", menuName = "Platformer/Keybinds")]
public class Keybinds : ScriptableObject
{
    [Header("Movement")]
    public KeyCode MoveLeft = KeyCode.A;
    public KeyCode MoveRight = KeyCode.D;
    public KeyCode Jump = KeyCode.Space;

    [Header("Ghost")]
    public KeyCode Swap = KeyCode.Tab;
    public KeyCode BecomePlatform = KeyCode.Mouse0;
}
