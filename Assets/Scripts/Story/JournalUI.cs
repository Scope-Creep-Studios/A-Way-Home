
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalUI : MonoBehaviour
{
    public GameObject journalPanel;

    [Header("Description")]
    public Image itemImage;
    public TMP_Text itemNameText;
    public TMP_Text descriptionText;

    [Header("Item Images")]
    public Sprite tieSprite;
    public Sprite posterSprite;
    public Sprite ringSprite;

    [Header("Item Buttons")]
    public Button tieButton;
    public Button posterButton;
    public Button ringButton;

    [Header("Locked Item Appearance")]
    public Color lockedColor = Color.gray;

    private Color tieOriginalColor;
    private Color posterOriginalColor;
    private Color ringOriginalColor;

    [Header("Journal Animation")]
    public RectTransform journalIcon;

    [Header("Notification Animation")]
    public Image journalIconImage;
    public float pulseScale = 1.12f;
    public float pulseSpeed = 1.5f;

    private Coroutine notificationCoroutine;
    private Vector3 originalIconScale;
    private Color originalIconColor;

    private void Start()
    {
        if (journalIcon != null)
        {
            originalIconScale = journalIcon.localScale;
        }

        if (journalIconImage != null)
        {
            originalIconColor = journalIconImage.color;
        }

        // Save original button colors.
        tieOriginalColor = tieButton.image.color;
        posterOriginalColor = posterButton.image.color;
        ringOriginalColor = ringButton.image.color;

        RefreshJournal();
    }

    public void ToggleJournal()
    {
        journalPanel.SetActive(!journalPanel.activeSelf);

        if (journalPanel.activeSelf)
        {
            StopNewItemNotification();
            RefreshJournal();
        }
    }

    public void CloseJournal()
    {
        journalPanel.SetActive(false);
    }

    private void RefreshJournal()
    {
        Debug.Log("Collected Items:");

        foreach (string item in JournalManager.Instance.collectedItems)
        {
            Debug.Log(item);
        }
        UpdateItemButton(tieButton, "Tie", tieOriginalColor);
        UpdateItemButton(posterButton, "Poster", posterOriginalColor);
        UpdateItemButton(ringButton, "Ring", ringOriginalColor);
    }

    private void UpdateItemButton(
        Button button,
        string itemName,
        Color originalColor)
    {
        bool collected = IsCollected(itemName);

        button.interactable = collected;

        if (collected)
        {
            button.image.color = originalColor;
        }
        else
        {
            Color color = lockedColor;
            button.image.color = color;
        }
    }

    private bool IsCollected(string itemName)
    {
        return JournalManager.Instance != null &&
               JournalManager.Instance.collectedItems.Contains(itemName);
    }

    public void ShowTie()
    {
        ShowItem(
            tieSprite,
            "Tie",
            "A dark-colored tie, wrinkled from being soaked in the river.\n" +
            "The style looks a little outdated.\n\n" +
            "The girl doesn’t really recognize it. Her father rarely wore ties, except for formal occasions she can barely remember.\n\n" +
            "The ghost, though, stares at it for a long time.\n\n" +
            "For some reason, he can’t shake the feeling that the knot isn’t tied quite right."
        );
    }

    public void ShowPoster()
    {
        ShowItem(
            posterSprite,
            "Missing Person Poster",
            "A rain-wrinkled missing-child flyer. Layer upon layer of dried paste clings to all four corners.\n\n" +
            "Beneath the photograph, the girl's name and two contact numbers have been gone over again and again, until the ink has nearly bled through to the other side. At the bottom, one more line has been added in small handwriting—\n\n" +
            "Please call, no matter the hour."
        );
    }

    public void ShowRing()
    {
        ShowItem(
            ringSprite,
            "Ring",
            "Everything once etched into my sight\n\n" +
            "The longing that once held all of me together\n\n" +
            "The pure desire that once burned within my heart\n\n" +
            "The beauty withered long ago\n\n" +
            "The fierce echo of a life refusing to fade\n\n" +
            "Undeniable proof that I once lived"
        );
    }

    private void ShowItem(Sprite image, string title, string description)
    {
        string itemName = title == "Missing Person Poster"
            ? "Poster"
            : title;

        if (!IsCollected(itemName))
        {
            return;
        }

        itemImage.sprite = image;
        itemNameText.text = title;
        descriptionText.text = description;
    }

    // Start notification when a new item is collected.
    public void ShowNewItemNotification()
    {
        if (journalPanel.activeSelf)
        {
            return;
        }

        if (notificationCoroutine == null)
        {
            notificationCoroutine = StartCoroutine(NotificationAnimation());
        }
    }

    private IEnumerator NotificationAnimation()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;

            // Grow effect.
            float scale = Mathf.Lerp(1f, pulseScale, t);
            journalIcon.localScale = originalIconScale * scale;

            // Change icon color.
            Color yellowColor = new Color(0.96f, 0.91f, 0.79f);

            journalIconImage.color = Color.Lerp(
                originalIconColor,
                yellowColor,
                t
            );

            yield return null;
        }
    }

    // Stop notification when Journal is opened.
    public void StopNewItemNotification()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        if (journalIcon != null)
        {
            journalIcon.localScale = originalIconScale;
        }

        if (journalIconImage != null)
        {
            journalIconImage.color = originalIconColor;
        }
    }

    // Play the collectible flies into Journal.
    public void PlayJournalIconAnimation()
    {
        StartCoroutine(JournalIconBounce());
    }

    private IEnumerator JournalIconBounce()
    {
        Vector3 originalScale = journalIcon.localScale;

        journalIcon.localScale = originalScale * 1.2f;

        yield return new WaitForSeconds(0.1f);

        // Do not reset the icon if notification is still playing.
        if (notificationCoroutine == null)
        {
            journalIcon.localScale = originalIconScale;
        }
    }
}