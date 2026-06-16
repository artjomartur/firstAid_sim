using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// In-game First Aid Handbook - A Windows 98-style medical encyclopedia
/// Provides accurate first-aid procedures for every scenario in the game.
/// Accessible via desktop icon "Handbuch.exe"
/// </summary>
public class FirstAidHandbook : MonoBehaviour
{
    public static FirstAidHandbook Instance { get; private set; }

    private GameObject panel;
    private GameObject contentContainer;
    private Text titleText;
    private int currentPage = 0;

    // Map scenario index → localization prefix (m1, m2, etc.)
    private readonly string[] scenarioKeys = {
        "m1", "m2", "m3", "m4", "m5", "m6", "m7", "m8", "m9",
        "m10", "m11", "m12", "m13", "m14", "m15"
    };

    // Emoji icons for each scenario
    private readonly string[] scenarioIcons = {
        "🚲", "🩸", "😴", "🔥", "🫁", "☀", "⚡", "☠", "🚨",
        "🦴", "🤧", "🌊", "💉", "😰", "🧠"
    };

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        SetupHandbookUI();
    }

    public void ToggleWindow()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf)
            {
                panel.transform.SetAsLastSibling();
                ShowPage(currentPage);
            }
        }
    }

    private void SetupHandbookUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        // ── Main Window ──
        panel = new GameObject("HandbookWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(900, 720);
        panelRT.anchoredPosition = new Vector2(20, 10);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.88f, 0.88f, 0.9f);

        // Window shadow (parented to panel so it moves with it)
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "HandbookWindow_Shadow", new Vector2(4, -4), new Vector2(900, 720));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // ── Header Bar ──
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 336), new Vector2(892, 40));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        titleText = UIFactory.CreateText(header.transform, "Title", "📖 Erste-Hilfe-Handbuch", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        titleText.color = Color.white;
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 40);

        // ── Close Button ──
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(420, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // ── Accent Line ──
        GameObject accentLine = UIFactory.CreateUIElement(panelRT, "AccentLine", new Vector2(0, 314), new Vector2(892, 4));
        accentLine.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f);
        accentLine.AddComponent<AccentPulse>();

        // ── Left Sidebar: Scenario List ──
        GameObject sidebar = UIFactory.CreateUIElement(panelRT, "Sidebar", new Vector2(-320, -30), new Vector2(240, 620));
        sidebar.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

        // Scrollable list inside sidebar
        GameObject sidebarScroll = new GameObject("SidebarScroll");
        sidebarScroll.transform.SetParent(sidebar.transform, false);
        RectTransform scrollRT = sidebarScroll.AddComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.sizeDelta = new Vector2(-8, -8);
        scrollRT.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = sidebarScroll.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        Image scrollMask = sidebarScroll.AddComponent<Image>();
        scrollMask.color = Color.white;
        sidebarScroll.AddComponent<Mask>().showMaskGraphic = false;

        GameObject sidebarContent = new GameObject("Content");
        sidebarContent.transform.SetParent(sidebarScroll.transform, false);
        RectTransform contentRT = sidebarContent.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0, scenarioKeys.Length * 42f);
        contentRT.anchoredPosition = Vector2.zero;
        scrollRect.content = contentRT;

        VerticalLayoutGroup vlg = sidebarContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(4, 4, 4, 4);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var loc = LocalizationManager.Instance;
        for (int i = 0; i < scenarioKeys.Length; i++)
        {
            int pageIdx = i; // Capture for closure
            string label = scenarioIcons[i] + " " + (loc != null ? loc.Get(scenarioKeys[i] + "_title") : scenarioKeys[i]);

            GameObject entryBtn = UIFactory.CreateButton(contentRT, "Entry_" + i, label, Vector2.zero, new Vector2(220, 38), buttonSprite);
            
            // Make it fit the layout
            LayoutElement le = entryBtn.AddComponent<LayoutElement>();
            le.preferredHeight = 38;
            le.flexibleWidth = 1f;

            Text entryTxt = entryBtn.GetComponentInChildren<Text>();
            if (entryTxt != null)
            {
                entryTxt.fontSize = 14;
                entryTxt.color = Color.black;
                entryTxt.alignment = TextAnchor.MiddleLeft;
            }

            entryBtn.GetComponent<Button>().onClick.AddListener(() => ShowPage(pageIdx));
        }

        // ── Right Content Area ──
        contentContainer = UIFactory.CreateUIElement(panelRT, "ContentArea", new Vector2(130, -30), new Vector2(620, 620));
        contentContainer.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);

        // ── Navigation Buttons ──
        GameObject prevBtn = UIFactory.CreateButton(panelRT, "PrevBtn", "◀ Zurück", new Vector2(-110, -335), new Vector2(160, 40), buttonSprite);
        prevBtn.GetComponent<Button>().onClick.AddListener(() => ShowPage(Mathf.Max(0, currentPage - 1)));
        Text prevTxt = prevBtn.GetComponentInChildren<Text>();
        if (prevTxt != null) { prevTxt.fontSize = 16; prevTxt.color = Color.black; }

        GameObject nextBtn = UIFactory.CreateButton(panelRT, "NextBtn", "Weiter ▶", new Vector2(110, -335), new Vector2(160, 40), buttonSprite);
        nextBtn.GetComponent<Button>().onClick.AddListener(() => ShowPage(Mathf.Min(scenarioKeys.Length - 1, currentPage + 1)));
        Text nextTxt = nextBtn.GetComponentInChildren<Text>();
        if (nextTxt != null) { nextTxt.fontSize = 16; nextTxt.color = Color.black; }

        // Show first page
        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        currentPage = index;
        if (contentContainer == null) return;

        // Clear existing content children
        foreach (Transform child in contentContainer.transform)
            Destroy(child.gameObject);

        RectTransform contentRT = contentContainer.GetComponent<RectTransform>();
        var loc = LocalizationManager.Instance;
        string key = scenarioKeys[index];

        // ── Scrollable content container ──
        GameObject scrollObj = new GameObject("Scroll");
        scrollObj.transform.SetParent(contentRT, false);
        RectTransform scrollObjRT = scrollObj.AddComponent<RectTransform>();
        scrollObjRT.anchorMin = Vector2.zero;
        scrollObjRT.anchorMax = Vector2.one;
        scrollObjRT.sizeDelta = Vector2.zero;
        scrollObjRT.anchoredPosition = Vector2.zero;

        ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
        sr.horizontal = false;
        Image scrollMask = scrollObj.AddComponent<Image>();
        scrollMask.color = Color.white;
        scrollObj.AddComponent<Mask>().showMaskGraphic = false;

        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(scrollObj.transform, false);
        RectTransform innerRT = inner.AddComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0, 1);
        innerRT.anchorMax = new Vector2(1, 1);
        innerRT.pivot = new Vector2(0.5f, 1f);
        innerRT.sizeDelta = new Vector2(0, 680); // Will be tall enough for all content
        innerRT.anchoredPosition = Vector2.zero;
        sr.content = innerRT;

        VerticalLayoutGroup vlg = inner.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(16, 16, 12, 12);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = inner.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── Title ──
        string scenarioTitle = scenarioIcons[index] + "  " + (loc != null ? loc.Get(key + "_title") : key);
        CreateContentLabel(inner.transform, scenarioTitle, 26, FontStyle.Bold, new Color(0.1f, 0.1f, 0.15f));

        // ── Divider ──
        CreateDivider(inner.transform, new Color(0.9f, 0.2f, 0.2f));

        // ── Recognition section ──
        string whenLabel = loc != null ? loc.Get("handbook_when") : "⚠ Erkennung:";
        CreateContentLabel(inner.transform, whenLabel, 17, FontStyle.Bold, new Color(0.6f, 0.35f, 0.05f));
        string whenText = loc != null ? loc.Get(key + "_hb_when") : "";
        CreateContentLabel(inner.transform, whenText, 15, FontStyle.Normal, new Color(0.25f, 0.25f, 0.3f));

        // ── Steps section ──
        string stepsLabel = loc != null ? loc.Get("handbook_steps") : "✅ Richtige Schritte:";
        CreateContentLabel(inner.transform, stepsLabel, 17, FontStyle.Bold, new Color(0.1f, 0.5f, 0.2f));
        for (int s = 1; s <= 3; s++)
        {
            string step = loc != null ? loc.Get(key + "_hb_step" + s) : "";
            CreateStepCard(inner.transform, step, new Color(0.88f, 0.96f, 0.9f), new Color(0.1f, 0.5f, 0.2f));
        }

        // ── Common Mistakes section ──
        string dontsLabel = loc != null ? loc.Get("handbook_donts") : "❌ Häufige Fehler:";
        CreateContentLabel(inner.transform, dontsLabel, 17, FontStyle.Bold, new Color(0.7f, 0.15f, 0.15f));
        string dontText = loc != null ? loc.Get(key + "_hb_dont") : "";
        CreateStepCard(inner.transform, dontText, new Color(0.96f, 0.88f, 0.88f), new Color(0.7f, 0.15f, 0.15f));

        // ── Key Fact section ──
        string factLabel = loc != null ? loc.Get("handbook_fact") : "💡 Wichtiger Fakt:";
        CreateContentLabel(inner.transform, factLabel, 17, FontStyle.Bold, new Color(0.15f, 0.4f, 0.7f));
        string factText = loc != null ? loc.Get(key + "_hb_fact") : "";
        CreateStepCard(inner.transform, factText, new Color(0.88f, 0.92f, 0.98f), new Color(0.15f, 0.35f, 0.6f));

        // Update header title
        if (titleText != null)
        {
            string appTitle = loc != null ? loc.Get("handbook_title") : "📖 Erste-Hilfe-Handbuch";
            titleText.text = appTitle + " — " + (loc != null ? loc.Get(key + "_title") : key);
        }
    }

    private void CreateContentLabel(Transform parent, string text, int fontSize, FontStyle style, Color color)
    {
        GameObject obj = new GameObject("Label");
        obj.transform.SetParent(parent, false);
        
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(580, 0);
        
        Text txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = color;
        txt.alignment = TextAnchor.UpperLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        // Make the text auto-size
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 580;
        ContentSizeFitter csf = obj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void CreateStepCard(Transform parent, string text, Color bgColor, Color accentColor)
    {
        GameObject card = new GameObject("StepCard");
        card.transform.SetParent(parent, false);

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = bgColor;
        cardBg.raycastTarget = false;

        VerticalLayoutGroup cardVlg = card.AddComponent<VerticalLayoutGroup>();
        cardVlg.padding = new RectOffset(12, 12, 8, 8);
        cardVlg.childForceExpandWidth = true;
        cardVlg.childForceExpandHeight = false;
        cardVlg.childControlWidth = true;
        cardVlg.childControlHeight = true;

        ContentSizeFitter cardCsf = card.AddComponent<ContentSizeFitter>();
        cardCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement cardLe = card.AddComponent<LayoutElement>();
        cardLe.preferredWidth = 580;

        // Left accent stripe
        // (Skipped for simplicity inside layout group - the bg color already gives the card identity)

        // Text
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(card.transform, false);

        Text txt = txtObj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 15;
        txt.fontStyle = FontStyle.Normal;
        txt.color = new Color(0.15f, 0.15f, 0.2f);
        txt.alignment = TextAnchor.UpperLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        LayoutElement txtLe = txtObj.AddComponent<LayoutElement>();
        txtLe.preferredWidth = 556;
        ContentSizeFitter txtCsf = txtObj.AddComponent<ContentSizeFitter>();
        txtCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void CreateDivider(Transform parent, Color color)
    {
        GameObject div = new GameObject("Divider");
        div.transform.SetParent(parent, false);

        Image divImg = div.AddComponent<Image>();
        divImg.color = color;
        divImg.raycastTarget = false;

        LayoutElement le = div.AddComponent<LayoutElement>();
        le.preferredHeight = 3;
        le.flexibleWidth = 1f;
    }
}
