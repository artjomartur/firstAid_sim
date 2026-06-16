using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retro bouncing text screensaver that activates after inactivity.
/// Text bounces around the screen changing color on each wall hit.
/// </summary>
public class ScreensaverManager : MonoBehaviour
{
    public static ScreensaverManager Instance;

    private GameObject overlay;
    private RectTransform textRT;
    private Text screensaverText;
    private Image overlayBg;

    private Vector2 velocity;
    private float speed = 120f;
    private float inactivityTimer = 0f;
    private float activateAfter = 90f; // seconds of inactivity
    private bool isActive = false;
    private Color currentColor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        overlay = new GameObject("ScreensaverOverlay");
        overlay.transform.SetParent(canvasRT, false);

        RectTransform overlayRT = overlay.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        overlayRT.anchoredPosition = Vector2.zero;

        overlayBg = overlay.AddComponent<Image>();
        overlayBg.color = Color.black;
        overlayBg.raycastTarget = true;

        // Bouncing text
        GameObject textObj = new GameObject("ScreensaverText");
        textObj.transform.SetParent(overlay.transform, false);
        textRT = textObj.AddComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(400, 60);
        textRT.anchoredPosition = new Vector2(100, 50);

        screensaverText = textObj.AddComponent<Text>();
        screensaverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        screensaverText.fontSize = 36;
        screensaverText.fontStyle = FontStyle.Bold;
        screensaverText.alignment = TextAnchor.MiddleCenter;
        screensaverText.text = "🏥 First Aid OS 98";
        screensaverText.raycastTarget = false;

        currentColor = Color.HSVToRGB(Random.value, 0.8f, 1f);
        screensaverText.color = currentColor;

        // Random initial velocity
        float angle = Random.Range(20f, 70f) * Mathf.Deg2Rad;
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

        overlay.SetActive(false);
    }

    private void Update()
    {
        // Track inactivity
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            inactivityTimer = 0f;
            if (isActive)
            {
                Deactivate();
            }
            return;
        }

        // Only activate in menu (not during gameplay)
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.gameStarted)
        {
            inactivityTimer = 0f;
            return;
        }

        inactivityTimer += Time.unscaledDeltaTime;

        if (!isActive && inactivityTimer >= activateAfter)
        {
            Activate();
        }

        if (isActive)
        {
            AnimateBounce();
        }
    }

    public void Activate()
    {
        if (overlay == null) return;
        isActive = true;
        overlay.SetActive(true);
        overlay.transform.SetAsLastSibling();
    }

    private void Deactivate()
    {
        if (overlay == null) return;
        isActive = false;
        overlay.SetActive(false);
        inactivityTimer = 0f;
    }

    private void AnimateBounce()
    {
        if (textRT == null || overlay == null) return;

        RectTransform parentRT = overlay.GetComponent<RectTransform>();
        Vector2 parentSize = parentRT.rect.size;
        Vector2 textSize = textRT.sizeDelta;

        Vector2 pos = textRT.anchoredPosition;
        pos += velocity * Time.unscaledDeltaTime;

        float halfW = textSize.x / 2f;
        float halfH = textSize.y / 2f;
        float maxX = parentSize.x / 2f - halfW;
        float maxY = parentSize.y / 2f - halfH;

        bool bounced = false;

        if (pos.x > maxX) { pos.x = maxX; velocity.x = -Mathf.Abs(velocity.x); bounced = true; }
        else if (pos.x < -maxX) { pos.x = -maxX; velocity.x = Mathf.Abs(velocity.x); bounced = true; }

        if (pos.y > maxY) { pos.y = maxY; velocity.y = -Mathf.Abs(velocity.y); bounced = true; }
        else if (pos.y < -maxY) { pos.y = -maxY; velocity.y = Mathf.Abs(velocity.y); bounced = true; }

        if (bounced)
        {
            currentColor = Color.HSVToRGB(Random.value, 0.8f, 1f);
            if (screensaverText != null) screensaverText.color = currentColor;
        }

        textRT.anchoredPosition = pos;
    }

    /// <summary>Force-start the screensaver (e.g., from context menu)</summary>
    public void ForceActivate()
    {
        inactivityTimer = activateAfter;
        Activate();
    }
}
