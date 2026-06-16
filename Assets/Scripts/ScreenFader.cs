using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    
    private Image fadeImage;
    private bool isFading;
    private Coroutine currentFadeRoutine;

    [Header("Cloud Transition")]
    public Sprite cloudSprite;
    private RectTransform cloudRect;
    private Image cloudImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Always on top

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start clear
        
        RectTransform rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Block raycasts only when fully faded
        fadeImage.raycastTarget = false; 

        // Setup Cloud Transition Image
        GameObject cloudObj = new GameObject("CloudImage");
        cloudObj.transform.SetParent(canvasObj.transform);
        cloudImage = cloudObj.AddComponent<Image>();
        cloudImage.color = Color.white;
        cloudImage.preserveAspect = false;
        
        cloudRect = cloudObj.GetComponent<RectTransform>();
        cloudRect.anchorMin = new Vector2(0, 0);
        cloudRect.anchorMax = new Vector2(1, 1);
        cloudRect.offsetMin = Vector2.zero;
        cloudRect.offsetMax = Vector2.zero;
        
        // We will make it 3 times as wide as the screen so it covers fully while sliding
        cloudRect.sizeDelta = new Vector2(Screen.width * 3, 0);
        cloudRect.anchoredPosition = new Vector2(-Screen.width * 3, 0);
        cloudObj.SetActive(false);
    }

    public void FadeToBlack(float duration, Action onComplete = null)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeRoutine(1f, duration, onComplete));
    }

    public void FadeToClear(float duration, Action onComplete = null)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeRoutine(0f, duration, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
    {
        isFading = true;
        fadeImage.raycastTarget = true;
        
        float startAlpha = fadeImage.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
        fadeImage.raycastTarget = (targetAlpha >= 0.99f);
        isFading = false;
        
        onComplete?.Invoke();
    }

    public void CloudTransition(float duration, Action onMidpoint)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(CloudRoutine(duration, onMidpoint));
    }

    private IEnumerator CloudRoutine(float duration, Action onMidpoint)
    {
        isFading = true;
        cloudImage.gameObject.SetActive(true);
        if (cloudSprite != null) cloudImage.sprite = cloudSprite;

        // Start position (left of screen)
        float startX = -Screen.width * 2f;
        // Mid position (covering screen)
        float midX = 0f;
        // End position (right of screen)
        float endX = Screen.width * 2f;

        cloudRect.anchoredPosition = new Vector2(startX, 0);
        
        float halfDuration = duration / 2f;
        float time = 0;

        // Slide In
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = time / halfDuration;
            // Ease out quad
            t = t * (2 - t); 
            
            float x = Mathf.Lerp(startX, midX, t);
            cloudRect.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }

        cloudRect.anchoredPosition = new Vector2(midX, 0);
        onMidpoint?.Invoke();

        time = 0;
        // Slide Out
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = time / halfDuration;
            // Ease in quad
            t = t * t;

            float x = Mathf.Lerp(midX, endX, t);
            cloudRect.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }

        cloudRect.anchoredPosition = new Vector2(endX, 0);
        cloudImage.gameObject.SetActive(false);
        isFading = false;
    }
}
