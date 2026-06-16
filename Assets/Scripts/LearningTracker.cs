using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Learning Progress Tracker – Shows per-scenario mastery in a Windows 98-style dashboard.
/// Tracks which first-aid skills the player has practiced and how well they performed.
/// Accessible via desktop icon "Lernfortschritt.exe"
/// </summary>
public class LearningTracker : MonoBehaviour
{
    public static LearningTracker Instance { get; private set; }

    private GameObject panel;

    // Map scenario index to internal key and localization key
    private readonly string[] scenarioKeys = {
        "m1", "m2", "m3", "m4", "m5", "m6", "m7", "m8", "m9",
        "m10", "m11", "m12", "m13", "m14", "m15"
    };

    private readonly string[] scenarioIcons = {
        "🚲", "🩸", "😴", "🔥", "🫁", "☀", "⚡", "☠", "🚨",
        "🦴", "🤧", "🌊", "💉", "😰", "🧠"
    };

    // Map scenario index → GameManager helped flag key
    private readonly string[] helpedKeys = {
        "helped_bikeAccident", "helped_bleedingWound", "helped_unconscious",
        "helped_burnInjury", "helped_choking", "helped_heatstroke",
        "helped_electricShock", "helped_poisoning", "helped_triage",
        "helped_boneFracture", "helped_allergicShock", "helped_drowning",
        "helped_diabeticShock", "helped_panicAttack", "helped_stroke"
    };

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        SetupTrackerUI();
    }

    public void ToggleWindow()
    {
        if (panel != null)
        {
            bool wasActive = panel.activeSelf;
            panel.SetActive(!wasActive);
            if (!wasActive)
            {
                panel.transform.SetAsLastSibling();
                RefreshDisplay();
            }
        }
    }

    /// <summary>
    /// Records that a scenario was completed with a given accuracy (0-100).
    /// Stores the best accuracy per scenario in PlayerPrefs.
    /// </summary>
    public void RecordScenarioCompletion(string missionName, float accuracy)
    {
        string key = MapMissionNameToKey(missionName);
        if (string.IsNullOrEmpty(key)) return;

        string prefKey = "learning_best_" + key;
        float prevBest = PlayerPrefs.GetFloat(prefKey, -1f);
        if (accuracy > prevBest)
        {
            PlayerPrefs.SetFloat(prefKey, accuracy);
            PlayerPrefs.Save();
        }
    }

    private string MapMissionNameToKey(string missionName)
    {
        // Map various mission name strings back to scenario keys
        string lower = missionName.ToLower();
        if (lower.Contains("fahrrad") || lower.Contains("bike") || lower.Contains("reanima")) return "m1";
        if (lower.Contains("blut") || lower.Contains("schnitt") || lower.Contains("bleed") || lower.Contains("wound")) return "m2";
        if (lower.Contains("bewusstlos") || lower.Contains("unconscious")) return "m3";
        if (lower.Contains("verbrenn") || lower.Contains("burn")) return "m4";
        if (lower.Contains("verschluck") || lower.Contains("chok")) return "m5";
        if (lower.Contains("hitz") || lower.Contains("heat")) return "m6";
        if (lower.Contains("strom") || lower.Contains("electric")) return "m7";
        if (lower.Contains("vergift") || lower.Contains("poison")) return "m8";
        if (lower.Contains("triage") || lower.Contains("massen")) return "m9";
        if (lower.Contains("knochen") || lower.Contains("bone") || lower.Contains("fracture")) return "m10";
        if (lower.Contains("allergi") || lower.Contains("anaphyla")) return "m11";
        if (lower.Contains("ertrink") || lower.Contains("drown")) return "m12";
        if (lower.Contains("diabet") || lower.Contains("zucker")) return "m13";
        if (lower.Contains("panik") || lower.Contains("panic")) return "m14";
        if (lower.Contains("schlaganfall") || lower.Contains("stroke") || lower.Contains("fast")) return "m15";
        return null;
    }

    private void SetupTrackerUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        // ── Main Window ──
        panel = new GameObject("LearningTrackerWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(800, 640);
        panelRT.anchoredPosition = new Vector2(-30, -20);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.88f, 0.88f, 0.9f);

        // Window shadow (parented to panel so it moves with it)
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "LearningTrackerWindow_Shadow", new Vector2(4, -4), new Vector2(800, 640));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // ── Header Bar ──
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 296), new Vector2(792, 40));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        var loc = LocalizationManager.Instance;
        string appTitle = loc != null ? loc.Get("tracker_title") : "📈 Lernfortschritt";
        Text headerTxt = UIFactory.CreateText(header.transform, "Title", appTitle, new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerTxt.color = Color.white;
        headerTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 40);

        // ── Close Button ──
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(370, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // ── Accent Line ──
        GameObject accentLine = UIFactory.CreateUIElement(panelRT, "AccentLine", new Vector2(0, 274), new Vector2(792, 4));
        accentLine.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.4f);
        accentLine.AddComponent<AccentPulse>();

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (panel == null) return;
        RectTransform panelRT = panel.GetComponent<RectTransform>();

        // Remove old content (keep header, accent, close)
        for (int i = panelRT.childCount - 1; i >= 0; i--)
        {
            Transform child = panelRT.GetChild(i);
            if (child.name != "Header" && child.name != "AccentLine")
                Destroy(child.gameObject);
        }

        var loc = LocalizationManager.Instance;
        GameManager gm = FindFirstObjectByType<GameManager>();

        int completedCount = 0;
        float totalBestAccuracy = 0f;

        // ── Progress Overview Section ──
        // Create scrollable content
        GameObject scrollObj = new GameObject("ScrollContent");
        scrollObj.transform.SetParent(panelRT, false);
        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(10, 50);
        scrollRT.offsetMax = new Vector2(-10, -54);

        ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
        sr.horizontal = false;
        Image scrollMask = scrollObj.AddComponent<Image>();
        scrollMask.color = Color.white;
        scrollObj.AddComponent<Mask>().showMaskGraphic = false;

        GameObject innerContent = new GameObject("Inner");
        innerContent.transform.SetParent(scrollObj.transform, false);
        RectTransform innerRT = innerContent.AddComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0, 1);
        innerRT.anchorMax = new Vector2(1, 1);
        innerRT.pivot = new Vector2(0.5f, 1f);
        innerRT.sizeDelta = new Vector2(0, 800);
        innerRT.anchoredPosition = Vector2.zero;
        sr.content = innerRT;

        VerticalLayoutGroup vlg = innerContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        ContentSizeFitter csf = innerContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── Scenario Grid ──
        for (int i = 0; i < scenarioKeys.Length; i++)
        {
            bool helped = IsScenarioCompleted(i, gm);
            float bestAcc = PlayerPrefs.GetFloat("learning_best_" + scenarioKeys[i], -1f);
            if (helped) completedCount++;
            if (bestAcc >= 0) totalBestAccuracy += bestAcc;

            string title = scenarioIcons[i] + " " + (loc != null ? loc.Get(scenarioKeys[i] + "_title") : scenarioKeys[i]);

            // Build card
            GameObject card = new GameObject("Card_" + i);
            card.transform.SetParent(innerContent.transform, false);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = helped ? new Color(0.9f, 0.97f, 0.91f) : new Color(0.94f, 0.94f, 0.96f);
            cardBg.raycastTarget = false;

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 48;
            le.flexibleWidth = 1f;

            HorizontalLayoutGroup hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 6, 6);
            hlg.spacing = 8f;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Status icon
            string statusIcon = helped ? "✅" : "⬜";
            CreateCardLabel(card.transform, statusIcon, 20, FontStyle.Normal, Color.black, 32);

            // Title
            CreateCardLabel(card.transform, title, 15, FontStyle.Bold, new Color(0.15f, 0.15f, 0.2f), 360);

            // Medal / accuracy
            string medal = "—";
            Color medalColor = new Color(0.5f, 0.5f, 0.55f);
            if (bestAcc >= 95) { medal = "⭐⭐⭐"; medalColor = new Color(0.8f, 0.6f, 0.05f); }
            else if (bestAcc >= 75) { medal = "⭐⭐"; medalColor = new Color(0.55f, 0.55f, 0.6f); }
            else if (bestAcc >= 0) { medal = "⭐"; medalColor = new Color(0.65f, 0.45f, 0.2f); }
            CreateCardLabel(card.transform, medal, 18, FontStyle.Normal, medalColor, 100);

            // Accuracy text
            string accText = bestAcc >= 0 ? $"{bestAcc:F0}%" : "—";
            Color accColor = bestAcc >= 80 ? new Color(0.1f, 0.55f, 0.2f) :
                             bestAcc >= 50 ? new Color(0.75f, 0.55f, 0.1f) :
                             bestAcc >= 0 ? new Color(0.7f, 0.2f, 0.2f) :
                             new Color(0.5f, 0.5f, 0.55f);
            CreateCardLabel(card.transform, accText, 16, FontStyle.Bold, accColor, 60);
        }

        // ── Bottom Summary Bar ──
        float masteryPct = (completedCount / (float)scenarioKeys.Length) * 100f;

        GameObject summaryBar = UIFactory.CreateUIElement(panelRT, "SummaryBar", new Vector2(0, -295), new Vector2(780, 44));
        summaryBar.GetComponent<Image>().color = masteryPct >= 80f ? new Color(0.85f, 0.95f, 0.87f) : new Color(0.92f, 0.92f, 0.95f);

        string masteryLabel = loc != null ? loc.Get("tracker_mastery") : "Gesamtfortschritt:";
        string readyMsg = masteryPct >= 80f 
            ? (loc != null ? loc.Get("tracker_ready") : "✅ Bereit für die Zertifizierung!")
            : (loc != null ? loc.Get("tracker_keep_going") : "Weiter üben für die Zertifizierung!");

        Text summaryTxt = UIFactory.CreateText(summaryBar.transform, "SummaryText",
            $"{masteryLabel} {completedCount}/{scenarioKeys.Length} ({masteryPct:F0}%) — {readyMsg}",
            Vector2.zero, 17, TextAnchor.MiddleCenter);
        summaryTxt.color = masteryPct >= 80f ? new Color(0.1f, 0.5f, 0.2f) : new Color(0.3f, 0.3f, 0.4f);
        summaryTxt.fontStyle = FontStyle.Bold;
        summaryTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 44);
    }

    private bool IsScenarioCompleted(int index, GameManager gm)
    {
        if (gm == null) return PlayerPrefs.GetInt(helpedKeys[index], 0) == 1;

        switch (index)
        {
            case 0: return gm.bikeAccidentHelped;
            case 1: return gm.bleedingWoundHelped;
            case 2: return gm.unconsciousHelped;
            case 3: return gm.burnInjuryHelped;
            case 4: return gm.chokingHelped;
            case 5: return gm.heatstrokeHelped;
            case 6: return gm.electricShockHelped;
            case 7: return gm.poisoningHelped;
            case 8: return gm.triageHelped;
            case 9: return gm.boneFractureHelped;
            case 10: return gm.allergicShockHelped;
            case 11: return gm.drowningHelped;
            case 12: return gm.diabeticShockHelped;
            case 13: return gm.panicAttackHelped;
            case 14: return gm.strokeHelped;
            default: return false;
        }
    }

    private void CreateCardLabel(Transform parent, string text, int fontSize, FontStyle style, Color color, float width)
    {
        GameObject obj = new GameObject("Lbl");
        obj.transform.SetParent(parent, false);

        Text txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = width;
    }
}
