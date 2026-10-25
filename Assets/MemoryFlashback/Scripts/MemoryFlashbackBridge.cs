using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemoryFlashbackBridge : MonoBehaviour
{
    private const string Level2SceneName = "VerticalSlice";
    private const string FinalDialogueText = "Wait... what's...";
    private const string PrefabResourcePath = "MemoryFlashback/MemoryFlashbackRoot";

    private const float FadeDuration = 0.55f;

    private NarrativeUI narrativeUI;
    private Scene levelScene;

    private bool sawFinalLine = false;
    private bool busy = false;
    private bool hasPlayed = false;

    private float previousTimeScale = 1f;

    private readonly List<MonoBehaviour> disabledLevelBehaviours =
        new List<MonoBehaviour>();

    private GameObject flashbackRoot;
    private MemoryFlashbackController flashbackController;
    private bool flashbackFinished = false;

    private GameObject fadeCanvasObject;
    private Image fadeImage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (UnityEngine.Object.FindFirstObjectByType<MemoryFlashbackBridge>() != null)
            return;

        GameObject bridgeObject = new GameObject("MemoryFlashbackBridge");

        UnityEngine.Object.DontDestroyOnLoad(bridgeObject);

        bridgeObject.AddComponent<MemoryFlashbackBridge>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        BindToScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Level2SceneName)
        {
            BindToScene(scene);
        }
        else if (mode == LoadSceneMode.Single)
        {
            ClearBinding();
        }
    }

    private void BindToScene(Scene scene)
    {
        if (scene.name != Level2SceneName)
            return;

        levelScene = scene;

        narrativeUI =
            UnityEngine.Object.FindFirstObjectByType<NarrativeUI>();

        sawFinalLine = false;
        busy = false;
        hasPlayed = false;

        if (narrativeUI == null)
        {
            Debug.LogWarning(
                "[MemoryFlashbackBridge] NarrativeUI was not found in Level2-Visuals."
            );
        }
        else
        {
            Debug.Log(
                "[MemoryFlashbackBridge] Ready in Level2-Visuals."
            );
        }
    }

    private void ClearBinding()
    {
        narrativeUI = null;
        sawFinalLine = false;
        hasPlayed = false;
        levelScene = default;
    }

    private void LateUpdate()
    {
        if (busy || hasPlayed)
            return;

        if (!levelScene.IsValid() ||
            !levelScene.isLoaded ||
            levelScene.name != Level2SceneName)
        {
            return;
        }

        if (narrativeUI == null)
        {
            narrativeUI =
                UnityEngine.Object.FindFirstObjectByType<NarrativeUI>();

            if (narrativeUI == null)
                return;
        }

        if (narrativeUI.narrativeBox == null ||
            narrativeUI.dialogueText == null)
        {
            return;
        }

        if (narrativeUI.narrativeBox.activeSelf)
        {
            string currentText =
                narrativeUI.dialogueText.text.Trim();

            bool ghostIsSpeaking =
                narrativeUI.spiritDialogueBox == null ||
                narrativeUI.spiritDialogueBox.activeSelf;

            if (currentText == FinalDialogueText &&
                ghostIsSpeaking)
            {
                sawFinalLine = true;
            }

            return;
        }

        if (!sawFinalLine)
            return;

        GameManager gameManager =
            UnityEngine.Object.FindFirstObjectByType<GameManager>();

        if (gameManager != null &&
            !gameManager.hasCollectible)
        {
            sawFinalLine = false;
            return;
        }

        GameObject prefab =
            Resources.Load<GameObject>(PrefabResourcePath);

        if (prefab == null)
        {
            Debug.LogError(
                "[MemoryFlashbackBridge] Could not find " +
                "Resources/MemoryFlashback/MemoryFlashbackRoot.prefab"
            );

            sawFinalLine = false;
            return;
        }

        if (prefab.GetComponentInChildren<MemoryFlashbackController>(true) == null)
        {
            Debug.LogError(
                "[MemoryFlashbackBridge] MemoryFlashbackRoot prefab " +
                "does not contain MemoryFlashbackController."
            );

            sawFinalLine = false;
            return;
        }

        sawFinalLine = false;
        hasPlayed = true;

        StartCoroutine(PlayFlashback(prefab));
    }

    private IEnumerator PlayFlashback(GameObject prefab)
    {
        busy = true;

        FreezeLevel2();

        EnsureFadeOverlay();

        // Level 2 -> black
        yield return FadeTo(1f, FadeDuration);

        flashbackRoot = Instantiate(prefab);

        flashbackController =
            flashbackRoot.GetComponentInChildren<MemoryFlashbackController>(true);

        if (flashbackController == null)
        {
            Debug.LogError(
                "[MemoryFlashbackBridge] Instantiated prefab has no " +
                "MemoryFlashbackController."
            );

            Destroy(flashbackRoot);
            flashbackRoot = null;

            yield return FadeTo(0f, FadeDuration);

            RestoreLevel2();

            busy = false;
            yield break;
        }

        flashbackFinished = false;

        flashbackController.SetInputEnabled(false);
        flashbackController.Finished += OnFlashbackFinished;

        // Give MemoryFlashbackController.Start() one frame to initialize.
        yield return null;

        // Black -> visual novel
        yield return FadeTo(0f, FadeDuration);

        flashbackController.SetInputEnabled(true);

        while (!flashbackFinished)
        {
            yield return null;
        }

        flashbackController.SetInputEnabled(false);

        // Visual novel -> black
        yield return FadeTo(1f, FadeDuration);

        flashbackController.Finished -= OnFlashbackFinished;

        Destroy(flashbackRoot);

        flashbackRoot = null;
        flashbackController = null;

        // Let Destroy finish while the screen is still completely black.
        yield return null;

        // Black -> original Level 2.
        //
        // Level 2 is deliberately STILL frozen during this fade,
        // so the player cannot move invisibly before the screen returns.
        yield return FadeTo(0f, FadeDuration);

        RestoreLevel2();

        if (fadeCanvasObject != null)
        {
            Destroy(fadeCanvasObject);

            fadeCanvasObject = null;
            fadeImage = null;
        }

        busy = false;

        Debug.Log(
            "[MemoryFlashbackBridge] Flashback complete. Level 2 restored."
        );
    }

    private void OnFlashbackFinished()
    {
        flashbackFinished = true;
    }

    private void FreezeLevel2()
    {
        previousTimeScale = Time.timeScale;

        // Stops physics, timers, movement and normal time-based gameplay.
        Time.timeScale = 0f;

        disabledLevelBehaviours.Clear();

        MonoBehaviour[] behaviours =
            UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (!behaviour.enabled)
                continue;

            if (behaviour.gameObject.scene != levelScene)
                continue;

            disabledLevelBehaviours.Add(behaviour);

            behaviour.enabled = false;
        }

        Debug.Log(
            "[MemoryFlashbackBridge] Level 2 frozen."
        );
    }

    private void RestoreLevel2()
    {
        foreach (MonoBehaviour behaviour in disabledLevelBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        disabledLevelBehaviours.Clear();

        Time.timeScale = previousTimeScale;

        Debug.Log(
            "[MemoryFlashbackBridge] Level 2 controls restored."
        );
    }

    private void EnsureFadeOverlay()
    {
        if (fadeCanvasObject != null)
            return;

        fadeCanvasObject = new GameObject(
            "MemoryFlashbackFadeCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler)
        );

        fadeCanvasObject.transform.SetParent(
            transform,
            false
        );

        Canvas canvas =
            fadeCanvasObject.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        CanvasScaler scaler =
            fadeCanvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        GameObject fadeObject = new GameObject(
            "BlackFade",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        fadeObject.transform.SetParent(
            fadeCanvasObject.transform,
            false
        );

        RectTransform rect =
            fadeObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeImage =
            fadeObject.GetComponent<Image>();

        fadeImage.color =
            new Color(0f, 0f, 0f, 0f);

        fadeImage.raycastTarget = false;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeImage == null)
            yield break;

        float startingAlpha =
            fadeImage.color.a;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            Color color =
                fadeImage.color;

            color.a =
                Mathf.Lerp(
                    startingAlpha,
                    targetAlpha,
                    t
                );

            fadeImage.color = color;

            yield return null;
        }

        Color finalColor =
            fadeImage.color;

        finalColor.a = targetAlpha;

        fadeImage.color = finalColor;
    }
}