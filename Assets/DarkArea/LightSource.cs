using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LightSource : MonoBehaviour
{
    [Header("Interaction")]
    public GameObject interactionPrompt;
    public float interactionRange = 1.5f;
    public Transform ghost;

    [Header("Activation")]
    public DarknessArea darknessArea;
    public HiddenPlatform[] platformsToActivate;

    private bool isActivated = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
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

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateLight();
        }
    }

    private void ActivateLight()
    {
        isActivated = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

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