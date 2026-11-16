using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorybookManager : MonoBehaviour
{
    public TMP_Text rightPageText;
    public Image pageIllustration;
    public Sprite[] pageImages;
    public CanvasGroup pageContentGroup;

    [TextArea(3, 12)]
    public string[] storyPages;

    [Header("Typing Settings")]
    public float typingSpeed = 0.04f;

    [Header("Fade Settings")]
    public float fadeDuration = 0.6f;

    private int currentPage = 0;
    private int currentSentence = 0;

    private string[] sentences;
    private int[] sentenceEndCharacters;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isTransitioning = false;
    private bool storyFinished = false;

    void Start()
    {
        if (rightPageText == null ||
            pageContentGroup == null ||
            storyPages == null ||
            storyPages.Length == 0)
        {
            Debug.LogError("Please assign the required UI elements and story pages.");
            return;
        }

        pageContentGroup.alpha = 0;
        SetupPage();
        StartCoroutine(ShowFirstPage());
    }

    void Update()
    {
        if (storyFinished || isTransitioning ||
            !Input.GetKeyDown(KeyCode.E))
            return;

        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            rightPageText.maxVisibleCharacters =
                sentenceEndCharacters[currentSentence];

            isTyping = false;
            typingCoroutine = null;
        }
        else
        {
            currentSentence++;

            if (currentSentence < sentences.Length)
            {
                typingCoroutine = StartCoroutine(TypeCurrentSentence());
            }
            else
            {
                StartCoroutine(NextPageTransition());
            }
        }
    }

    void SetupPage()
    {
        if (currentPage >= storyPages.Length)
        {
            storyFinished = true;
            Debug.Log("Storybook finished");
            return;
        }

        sentences = storyPages[currentPage].Split(
            new char[] { '\n' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        currentSentence = 0;
        sentenceEndCharacters = new int[sentences.Length];

        rightPageText.text = string.Join("\n\n", sentences);
        rightPageText.maxVisibleCharacters = 0;

        if (pageImages != null &&
            currentPage < pageImages.Length &&
            pageIllustration != null)
        {
            pageIllustration.sprite = pageImages[currentPage];
        }

        int characterCount = 0;

        for (int i = 0; i < sentences.Length; i++)
        {
            characterCount += sentences[i].Trim().Length;
            sentenceEndCharacters[i] = characterCount;

            if (i < sentences.Length - 1)
                characterCount += 2;
        }
    }

    IEnumerator ShowFirstPage()
    {
        yield return StartCoroutine(Fade(0, 1));

        typingCoroutine = StartCoroutine(TypeCurrentSentence());
    }

    IEnumerator NextPageTransition()
    {
        isTransitioning = true;

        // Fade out the current page.
        yield return StartCoroutine(Fade(1, 0));

        currentPage++;

        if (currentPage >= storyPages.Length)
        {
            storyFinished = true;
            Debug.Log("Storybook finished");
            yield break;
        }

        // Load the next page.
        SetupPage();

        // Fade in the new page.
        yield return StartCoroutine(Fade(0, 1));

        // Start typing the new page.
        typingCoroutine = StartCoroutine(TypeCurrentSentence());

        isTransitioning = false;
    }

    IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        float elapsed = 0f;

        pageContentGroup.alpha = startAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            pageContentGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                elapsed / fadeDuration
            );

            yield return null;
        }

        pageContentGroup.alpha = targetAlpha;
    }

    IEnumerator TypeCurrentSentence()
    {
        isTyping = true;

        string sentence = sentences[currentSentence].Trim();

        int startCharacter = currentSentence == 0
            ? 0
            : sentenceEndCharacters[currentSentence - 1] + 2;

        int endCharacter = sentenceEndCharacters[currentSentence];

        for (int i = startCharacter; i < endCharacter; i++)
        {
            rightPageText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }
}