using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Retro boot sequence controller: DOS/BIOS text scrolling → Windows 98 splash → desktop fade-out.
/// Attach to a GameObject; it auto-creates a fullscreen overlay on the GameCanvas.
/// </summary>
public class BootSequenceManager : MonoBehaviour
{
    public static BootSequenceManager Instance;

    public System.Action OnBootComplete;

    private GameObject bootOverlay;
    private bool bootFinished = false;
    private bool skipping = false;

    // Phase objects
    private Text biosText;
    private GameObject splashPanel;
    private Image progressFill;
    private CanvasGroup overlayGroup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void StartBoot(RectTransform canvasRT)
    {
        // Create fullscreen overlay
        bootOverlay = new GameObject("BootOverlay");
        bootOverlay.transform.SetParent(canvasRT, false);
        bootOverlay.transform.SetAsLastSibling();

        RectTransform overlayRT = bootOverlay.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;

        Image overlayBg = bootOverlay.AddComponent<Image>();
        overlayBg.color = Color.black;
        overlayBg.raycastTarget = true; // Block clicks to desktop

        overlayGroup = bootOverlay.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 1f;

        // === Phase 1: BIOS Text ===
        GameObject biosObj = new GameObject("BiosText");
        biosObj.transform.SetParent(bootOverlay.transform, false);
        RectTransform biosRT = biosObj.AddComponent<RectTransform>();
        biosRT.anchorMin = new Vector2(0.02f, 0.02f);
        biosRT.anchorMax = new Vector2(0.98f, 0.98f);
        biosRT.sizeDelta = Vector2.zero;

        biosText = biosObj.AddComponent<Text>();
        biosText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        biosText.fontSize = 16;
        biosText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        biosText.alignment = TextAnchor.UpperLeft;
        biosText.horizontalOverflow = HorizontalWrapMode.Wrap;
        biosText.verticalOverflow = VerticalWrapMode.Overflow;
        biosText.supportRichText = true;
        biosText.text = "";

        // === Phase 2: Windows 98 Splash (initially hidden) ===
        splashPanel = new GameObject("SplashPanel");
        splashPanel.transform.SetParent(bootOverlay.transform, false);
        RectTransform splashRT = splashPanel.AddComponent<RectTransform>();
        splashRT.anchorMin = Vector2.zero;
        splashRT.anchorMax = Vector2.one;
        splashRT.sizeDelta = Vector2.zero;

        Image splashBg = splashPanel.AddComponent<Image>();
        splashBg.color = new Color(0f, 0f, 0.5f, 1f); // Classic Windows 98 blue

        // Logo Title: "First Aid OS 98"
        GameObject logoObj = new GameObject("LogoText");
        logoObj.transform.SetParent(splashPanel.transform, false);
        RectTransform logoRT = logoObj.AddComponent<RectTransform>();
        logoRT.anchoredPosition = new Vector2(0, 60);
        logoRT.sizeDelta = new Vector2(800, 120);

        Text logoText = logoObj.AddComponent<Text>();
        logoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        logoText.fontSize = 48;
        logoText.fontStyle = FontStyle.Bold;
        logoText.color = Color.white;
        logoText.alignment = TextAnchor.MiddleCenter;
        logoText.text = "First Aid OS 98";

        Shadow logoShadow = logoObj.AddComponent<Shadow>();
        logoShadow.effectColor = new Color(0, 0, 0, 0.6f);
        logoShadow.effectDistance = new Vector2(3, -3);

        // Subtitle
        GameObject subObj = new GameObject("SubText");
        subObj.transform.SetParent(splashPanel.transform, false);
        RectTransform subRT = subObj.AddComponent<RectTransform>();
        subRT.anchoredPosition = new Vector2(0, -10);
        subRT.sizeDelta = new Vector2(600, 40);

        Text subText = subObj.AddComponent<Text>();
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.fontSize = 18;
        subText.color = new Color(0.8f, 0.8f, 1f, 0.8f);
        subText.alignment = TextAnchor.MiddleCenter;
        subText.text = "Jede Sekunde zählt.";

        // "Starting..." hint
        GameObject hintObj = new GameObject("HintText");
        hintObj.transform.SetParent(splashPanel.transform, false);
        RectTransform hintRT = hintObj.AddComponent<RectTransform>();
        hintRT.anchoredPosition = new Vector2(0, -200);
        hintRT.sizeDelta = new Vector2(400, 30);

        Text hintText = hintObj.AddComponent<Text>();
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 14;
        hintText.color = new Color(1f, 1f, 1f, 0.5f);
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.text = "Klicke oder drücke Enter zum Überspringen...";

        // Progress Bar Background
        GameObject progBgObj = new GameObject("ProgressBG");
        progBgObj.transform.SetParent(splashPanel.transform, false);
        RectTransform progBgRT = progBgObj.AddComponent<RectTransform>();
        progBgRT.anchoredPosition = new Vector2(0, -130);
        progBgRT.sizeDelta = new Vector2(400, 24);

        Image progBgImg = progBgObj.AddComponent<Image>();
        progBgImg.color = new Color(0f, 0f, 0.3f, 1f);

        // Progress bar inner border
        GameObject progInnerObj = new GameObject("ProgressInner");
        progInnerObj.transform.SetParent(progBgObj.transform, false);
        RectTransform progInnerRT = progInnerObj.AddComponent<RectTransform>();
        progInnerRT.anchorMin = Vector2.zero;
        progInnerRT.anchorMax = Vector2.one;
        progInnerRT.offsetMin = new Vector2(3, 3);
        progInnerRT.offsetMax = new Vector2(-3, -3);

        Image progInnerImg = progInnerObj.AddComponent<Image>();
        progInnerImg.color = new Color(0f, 0f, 0.2f, 1f);

        // Progress bar fill (anchored left, we'll grow anchorMax.x)
        GameObject progFillObj = new GameObject("ProgressFill");
        progFillObj.transform.SetParent(progInnerObj.transform, false);
        RectTransform progFillRT = progFillObj.AddComponent<RectTransform>();
        progFillRT.anchorMin = Vector2.zero;
        progFillRT.anchorMax = new Vector2(0f, 1f);
        progFillRT.offsetMin = Vector2.zero;
        progFillRT.offsetMax = Vector2.zero;

        progressFill = progFillObj.AddComponent<Image>();
        progressFill.color = new Color(0.3f, 0.3f, 1f, 1f); // Windows 98 blue blocks

        splashPanel.SetActive(false);

        StartCoroutine(BootSequence());
    }

    private IEnumerator BootSequence()
    {
        // ────── Phase 1: DOS/BIOS Boot Text ──────
        string[] biosLines = {
            "<color=#00ff00>FirstAid BIOS v1.98</color>",
            "Copyright (C) 2026 FirstAid Systems, Inc.",
            "",
            "CPU: Pentium(R) III 450MHz",
            "Memory Test: 64 MB OK",
            "",
            "Detecting IDE drives...",
            "  Primary Master: WD Caviar 6.4GB ... <color=#00ff00>OK</color>",
            "  Primary Slave:  CD-ROM Drive    ... <color=#00ff00>OK</color>",
            "",
            "PnP Devices Found: 3",
            "  Sound Blaster 16 ... <color=#00ff00>OK</color>",
            "  Serial Mouse     ... <color=#00ff00>OK</color>",
            "  VGA Adapter       ... <color=#00ff00>OK</color>",
            "",
            "Loading First Aid OS 98...",
            "",
            "<color=#ffff00>Press any key to skip...</color>"
        };

        float lineDelay = 0.08f;
        for (int i = 0; i < biosLines.Length; i++)
        {
            if (skipping) break;
            biosText.text += biosLines[i] + "\n";
            yield return new WaitForSecondsRealtime(lineDelay);
        }

        // Blinking cursor for a moment
        if (!skipping)
        {
            for (int blink = 0; blink < 6; blink++)
            {
                if (skipping) break;
                biosText.text = biosText.text.TrimEnd('\n', '_') + (blink % 2 == 0 ? "_" : "") + "\n";
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        // ────── Phase 2: Windows 98 Splash ──────
        if (biosText != null) biosText.gameObject.SetActive(false);
        splashPanel.SetActive(true);

        // Play boot chime
        if (AudioManager.Instance != null && AudioManager.Instance.bootChimeSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bootChimeSound, 0.7f);
        }

        // Animate progress bar
        float splashDuration = 2.5f;
        float elapsed = 0f;
        RectTransform fillRT = progressFill.GetComponent<RectTransform>();

        while (elapsed < splashDuration && !skipping)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / splashDuration);
            fillRT.anchorMax = new Vector2(progress, 1f);
            yield return null;
        }

        // Finish fill instantly if skipped
        fillRT.anchorMax = new Vector2(1f, 1f);
        yield return new WaitForSecondsRealtime(skipping ? 0.1f : 0.4f);

        // ────── Phase 3: Fade Out ──────
        float fadeDuration = 0.5f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            overlayGroup.alpha = 1f - Mathf.Clamp01(fadeElapsed / fadeDuration);
            yield return null;
        }

        overlayGroup.alpha = 0f;
        bootFinished = true;

        if (bootOverlay != null) Destroy(bootOverlay);

        OnBootComplete?.Invoke();
    }

    private void Update()
    {
        if (bootFinished) return;

        // Skip check: click, Enter, or Space
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            skipping = true;
        }
    }
}
