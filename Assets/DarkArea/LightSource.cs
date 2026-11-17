using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LightSource : MonoBehaviour
{
    [Header("Interaction")]
    public GameObject interactionPrompt;
    public GameObject interactionBorder;
    
    public float interactionRange = 1.5f;
    public Transform ghost;

    [Header("Activation")]
    public DarknessArea darknessArea;
    public HiddenPlatform[] platformsToActivate;

    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Sprite _usedSprite;

    private bool isActivated = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        if (interactionBorder != null)
            interactionBorder.SetActive(false);
    }

    void Update()
    {
        if (isActivated || ghost == null)
            return;

        float distance = Vector2.Distance(
            ghost.position,
            transform.position
        );

        bool inRange = distance <= interactionRange;

        if (interactionPrompt != null) 
            interactionPrompt.SetActive(inRange);
        if (interactionBorder != null)
            interactionBorder.SetActive(inRange);

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateLight();
        }
    }

    private void ActivateLight()
    {
        isActivated = true;

        if (_sprite != null && _usedSprite != null) _sprite.sprite = _usedSprite;
        AudioController.Instance.Play(SoundCategory.Interactable,1);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        if (interactionBorder != null)
            interactionBorder.SetActive(false);

        if (darknessArea != null)
            darknessArea.LightUp();

        if (platformsToActivate != null)
        {
            foreach (HiddenPlatform platform in platformsToActivate)
            {
                if (platform != null)
                    platform.MakePermanent();
            }
        }
    }
}