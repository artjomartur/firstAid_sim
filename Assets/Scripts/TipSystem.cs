using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Rotating Medical Tip System – Shows "Did you know?" facts as toast notifications
/// during free-roam gameplay. Displays real medical facts to reinforce learning.
/// </summary>
public class TipSystem : MonoBehaviour
{
    public static TipSystem Instance { get; private set; }

    private const int TIP_COUNT = 20;
    private const float MIN_INTERVAL = 50f;
    private const float MAX_INTERVAL = 100f;

    private GameObject currentToast;
    private int lastTipIndex = -1;
    private bool isShowingTip = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(TipLoop());
    }

    private IEnumerator TipLoop()
    {
        // Wait for initial game setup
        yield return new WaitForSeconds(15f);

        while (true)
        {
            float waitTime = Random.Range(MIN_INTERVAL, MAX_INTERVAL);
            yield return new WaitForSeconds(waitTime);

            // Only show tips during gameplay (not in menus or missions)
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null && gm.gameStarted && !isShowingTip)
            {
                ShowRandomTip();
            }
        }
    }

    /// <summary>
    /// Can also be called manually, e.g., after a mission or from a terminal command
    /// </summary>
    public void ShowRandomTip()
    {
        if (isShowingTip) return;

        // Pick a random tip different from the last
        int tipIdx;
        do { tipIdx = Random.Range(1, TIP_COUNT + 1); } while (tipIdx == lastTipIndex);
        lastTipIndex = tipIdx;

        string tipKey = $"tip_{tipIdx:D2}";
        var loc = LocalizationManager.Instance;
        string prefix = loc != null ? loc.Get("tip_prefix") : "💡 Wusstest du?";
        string tipText = loc != null ? loc.Get(tipKey) : "";

        if (string.IsNullOrEmpty(tipText)) return;

        StartCoroutine(ShowToast(prefix, tipText));
    }

    private IEnumerator ShowToast(string prefix, string text)
    {
        isShowingTip = true;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) { isShowingTip = false; yield break; }

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        // ── Toast Container ──
        currentToast = new GameObject("TipToast");
        currentToast.transform.SetParent(canvasRT, false);

        RectTransform toastRT = currentToast.AddComponent<RectTransform>();
        toastRT.anchorMin = new Vector2(0.5f, 1f);
        toastRT.anchorMax = new Vector2(0.5f, 1f);
        toastRT.pivot = new Vector2(0.5f, 1f);
        toastRT.sizeDelta = new Vector2(900, 150);
        toastRT.anchoredPosition = new Vector2(0, 200); // Start offscreen (above)

        Image toastBg = currentToast.AddComponent<Image>();
        toastBg.color = new Color(0.08f, 0.12f, 0.22f, 0.92f);
        toastBg.raycastTarget = false;

        // ── Left accent bar ──
        GameObject accent = UIFactory.CreateUIElement(toastRT, "Accent", new Vector2(-444, 0), new Vector2(6, 140));
        accent.GetComponent<Image>().color = new Color(0.3f, 0.7f, 1f);
        accent.GetComponent<Image>().raycastTarget = false;

        // ── Prefix (Did you know?) ──
        Text prefixTxt = UIFactory.CreateText(toastRT, "Prefix", prefix, new Vector2(30, 45), 22, TextAnchor.MiddleLeft);
        prefixTxt.color = new Color(0.4f, 0.8f, 1f);
        prefixTxt.fontStyle = FontStyle.Bold;
        prefixTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(840, 30);
        prefixTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 45);

        // ── Tip Text ──
        Text tipTxt = UIFactory.CreateText(toastRT, "TipText", text, new Vector2(30, -15), 18, TextAnchor.UpperLeft);
        tipTxt.color = new Color(0.85f, 0.9f, 0.95f);
        tipTxt.fontStyle = FontStyle.Normal;
        tipTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(840, 90);
        tipTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, -15);
        Shadow shadow = tipTxt.GetComponent<Shadow>();
        if (shadow != null) shadow.effectColor = Color.clear;

        // ── Slide-down animation ──
        float slideDuration = 0.5f;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, 200);
        Vector2 endPos = new Vector2(0, -40);

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            toastRT.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        toastRT.anchoredPosition = endPos;

        // ── Stay visible ──
        yield return new WaitForSeconds(8f);

        // ── Slide-up animation ──
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            toastRT.anchoredPosition = Vector2.Lerp(endPos, startPos, t);
            // Fade out
            toastBg.color = new Color(0.08f, 0.12f, 0.22f, 0.92f * (1f - t));
            yield return null;
        }

        Destroy(currentToast);
        currentToast = null;
        isShowingTip = false;
    }

    private void OnDestroy()
    {
        if (currentToast != null)
            Destroy(currentToast);
    }
}
