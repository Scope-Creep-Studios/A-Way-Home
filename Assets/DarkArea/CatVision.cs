using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatVision : MonoBehaviour
{
    [Header("Cat Settings")]
    public Transform cat;
    public GameObject visionCircle;

    [Header("Hidden Platforms")]
    public float revealRadius = 3f;
    private HiddenPlatform[] platforms;

    [Header("Darkness Overlay")]
    public SpriteRenderer darknessRenderer;
    public float holeRadius = 2f;
    public float edgeSoftness = 0.2f;

    [Header("Vision Center Offset")]
    public Vector2 centerOffset = Vector2.zero;

    private MaterialPropertyBlock propertyBlock;
    private bool isInsideZone = false;

    private static readonly int HoleCenterID =
        Shader.PropertyToID("_HoleCenter");

    private static readonly int RadiusID =
        Shader.PropertyToID("_Radius");

    private static readonly int SoftnessID =
        Shader.PropertyToID("_Softness");

    private static readonly int HoleActiveID =
        Shader.PropertyToID("_HoleActive");

    void Start()
    {
        platforms = FindObjectsByType<HiddenPlatform>(
            FindObjectsSortMode.None
        );

        propertyBlock = new MaterialPropertyBlock();

        if (visionCircle != null)
        {
            visionCircle.SetActive(false);
        }

        UpdateDarknessOverlay(
            false,
            cat != null ? (Vector2)cat.position + centerOffset : Vector2.zero
        );

        HideAllPlatforms();
    }

    public void SetZoneActive(bool active)
    {
        isInsideZone = active;

        if (visionCircle != null)
        {
            visionCircle.SetActive(active);
        }
    }

    void Update()
    {
        if (cat == null)
            return;

        // Keep the vision circle centered on the ghost
        Vector2 center = (Vector2)cat.position + centerOffset;

        if (visionCircle != null)
        {
            visionCircle.transform.position = new Vector3(
                center.x,
                center.y,
                visionCircle.transform.position.z
            );
        }

        UpdateDarknessOverlay(isInsideZone, center);

        // reveal hidden platforms
        UpdateHiddenPlatforms(center);
    }

    void UpdateDarknessOverlay(bool active, Vector2 center)
    {
        if (darknessRenderer == null)
            return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        darknessRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetVector(
            HoleCenterID,
            new Vector4(center.x, center.y, 0f, 0f)
        );

        propertyBlock.SetFloat(RadiusID, holeRadius);
        propertyBlock.SetFloat(SoftnessID, edgeSoftness);
        propertyBlock.SetFloat(HoleActiveID, active ? 1f : 0f);

        darknessRenderer.SetPropertyBlock(propertyBlock);
    }

    void UpdateHiddenPlatforms(Vector2 center)
    {
        if (platforms == null)
            return;

        foreach (HiddenPlatform platform in platforms)
        {
            if (platform == null)
                continue;

            float distance = Vector2.Distance(
                center,
                platform.transform.position
            );

            bool visible = isInsideZone &&
                           distance <= revealRadius;

            platform.SetVisible(visible);
        }
    }

    void HideAllPlatforms()
    {
        if (platforms == null)
            return;

        foreach (HiddenPlatform platform in platforms)
        {
            if (platform != null)
            {
                platform.SetVisible(false);
            }
        }
    }

    void OnDisable()
    {
        UpdateDarknessOverlay(
            false,
            cat != null ? (Vector2)cat.position + centerOffset : Vector2.zero
        );

        HideAllPlatforms();
    }
}