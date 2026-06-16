using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StrokeManager : MonoBehaviour
{
    public static StrokeManager Instance { get; private set; }

    [Header("UI Views")]
    public GameObject panel;
    public Text scenarioDescText;
    public Text hintText;
    public Text bubbleText;
    
    public Button faceButton;
    public Button armsButton;
    public Button speechButton;
    public Button timeButton;
    
    [Header("Graphics Procedural Overlay")]
    public Image drawingArea;
    public Sprite faceSymptomSprite;
    public Sprite armsSymptomSprite;

    [Header("Diagnostic Selection")]
    public GameObject diagnosisPanel;
    public Button diagnosisPositiveBtn;
    public Button diagnosisNegativeBtn;

    private int activeStep = 0; // 0=Face, 1=Arms, 2=Speech, 3=Time
    private bool faceChecked = false;
    private bool armsChecked = false;
    private bool speechChecked = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupStrokeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupStrokeUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite innerFrameSprite = bootstrap != null ? bootstrap.winInnerFrameSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        // Main Panel
        panel = new GameObject("StrokePanel");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(900, 700);
        panelRT.anchoredPosition = Vector2.zero;

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else
        {
            bgImage.color = new Color(0.85f, 0.85f, 0.85f);
        }

        // Header Title
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 325), new Vector2(890, 40));
        RectTransform headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0.5f, 0.5f);
        headerRT.anchorMax = new Vector2(0.5f, 0.5f);
        headerRT.pivot = new Vector2(0.5f, 0.5f);

        Image hImg = header.GetComponent<Image>();
        if (headerSprite != null)
        {
            hImg.sprite = headerSprite;
            hImg.type = Image.Type.Sliced;
        }
        else
        {
            hImg.color = new Color(0.1f, 0.1f, 0.5f);
        }

        Text headerText = UIFactory.CreateText(header.transform, "Title", "Schlaganfall_Assistent.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;

        // Close Button
        GameObject closeBtn = UIFactory.CreateButton(headerRT, "CloseButton", "X", new Vector2(420, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(ClosePanel);
        Text cBtnTxt = closeBtn.GetComponentInChildren<Text>();
        if (cBtnTxt != null) cBtnTxt.color = Color.black;

        // Description Inner Frame
        GameObject descFrame = UIFactory.CreateUIElement(panelRT, "DescriptionFrame", new Vector2(0, 220), new Vector2(860, 100));
        descFrame.GetComponent<Image>().sprite = innerFrameSprite;
        descFrame.GetComponent<Image>().type = Image.Type.Sliced;
        descFrame.GetComponent<Image>().color = Color.white;

        scenarioDescText = UIFactory.CreateText(descFrame.transform, "DescText", "FAST-Test anwenden, um neurologische Symptome zu analysieren.", new Vector2(0, 0), 22, TextAnchor.MiddleCenter);
        scenarioDescText.color = Color.black;
        scenarioDescText.GetComponent<RectTransform>().sizeDelta = new Vector2(820, 80);

        // Sidebar Step Buttons Frame
        GameObject sidebar = UIFactory.CreateUIElement(panelRT, "Sidebar", new Vector2(-280, -90), new Vector2(260, 460));
        sidebar.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);

        // Step Buttons
        GameObject fBtn = UIFactory.CreateButton(sidebar.GetComponent<RectTransform>(), "FaceBtn", "1. GESICHT (Face)", new Vector2(0, 160), new Vector2(240, 60), buttonSprite);
        faceButton = fBtn.GetComponent<Button>();
        faceButton.onClick.AddListener(() => SetStep(0));

        GameObject aBtn = UIFactory.CreateButton(sidebar.GetComponent<RectTransform>(), "ArmsBtn", "2. ARME (Arms)", new Vector2(0, 60), new Vector2(240, 60), buttonSprite);
        armsButton = aBtn.GetComponent<Button>();
        armsButton.onClick.AddListener(() => SetStep(1));

        GameObject sBtn = UIFactory.CreateButton(sidebar.GetComponent<RectTransform>(), "SpeechBtn", "3. SPRACHE (Speech)", new Vector2(0, -40), new Vector2(240, 60), buttonSprite);
        speechButton = sBtn.GetComponent<Button>();
        speechButton.onClick.AddListener(() => SetStep(2));

        GameObject tBtn = UIFactory.CreateButton(sidebar.GetComponent<RectTransform>(), "TimeBtn", "4. ZEIT (Time)", new Vector2(0, -140), new Vector2(240, 60), buttonSprite);
        timeButton = tBtn.GetComponent<Button>();
        timeButton.onClick.AddListener(() => SetStep(3));

        // Interactive Content Area (Right-side)
        GameObject contentArea = UIFactory.CreateUIElement(panelRT, "ContentArea", new Vector2(120, -90), new Vector2(500, 460));
        contentArea.GetComponent<Image>().sprite = innerFrameSprite;
        contentArea.GetComponent<Image>().type = Image.Type.Sliced;
        contentArea.GetComponent<Image>().color = Color.white;

        // Visual Area (Upper portion of Content)
        GameObject visualFrame = UIFactory.CreateUIElement(contentArea.GetComponent<RectTransform>(), "VisualFrame", new Vector2(0, 70), new Vector2(460, 260));
        visualFrame.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f); // Dark elegant panel for drawings

        GameObject visualDraw = new GameObject("VisualDraw");
        visualDraw.transform.SetParent(visualFrame.transform, false);
        drawingArea = visualDraw.AddComponent<Image>();
        drawingArea.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 220);
        drawingArea.color = Color.white;
        drawingArea.preserveAspect = true;

        // Hints Area (Lower portion of Content)
        GameObject hintFrame = UIFactory.CreateUIElement(contentArea.GetComponent<RectTransform>(), "HintFrame", new Vector2(0, -140), new Vector2(460, 120));
        hintFrame.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);
        
        hintText = UIFactory.CreateText(hintFrame.transform, "HintText", "Klicke auf die Schritte links, um die Untersuchung zu starten.", new Vector2(0, 0), 20, TextAnchor.MiddleCenter);
        hintText.color = Color.black;
        hintText.GetComponent<RectTransform>().sizeDelta = new Vector2(420, 100);

        // Garbled speech bubble overlay (for Speech step)
        GameObject bubble = UIFactory.CreateUIElement(visualFrame.GetComponent<RectTransform>(), "SpeechBubble", new Vector2(0, -20), new Vector2(420, 80));
        bubble.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 0.9f);
        bubbleText = UIFactory.CreateText(bubble.transform, "BubbleText", "", Vector2.zero, 18, TextAnchor.MiddleCenter);
        bubbleText.color = Color.black;
        bubbleText.fontStyle = FontStyle.Italic;
        bubbleText.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 60);
        bubble.SetActive(false);

        // Diagnosis Overlay Panel (reused in Step 4)
        diagnosisPanel = UIFactory.CreateUIElement(contentArea.GetComponent<RectTransform>(), "DiagnosisPanel", Vector2.zero, new Vector2(500, 460));
        diagnosisPanel.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 0.95f);
        diagnosisPanel.SetActive(false);

        Text diagHeader = UIFactory.CreateText(diagnosisPanel.transform, "DiagHeader", "Diagnose stellen:", new Vector2(0, 140), 24, TextAnchor.MiddleCenter);
        diagHeader.color = Color.black;

        GameObject diagPos = UIFactory.CreateButton(diagnosisPanel.GetComponent<RectTransform>(), "DiagPosBtn", "Schlaganfall! (FAST positiv)", new Vector2(0, 40), new Vector2(400, 60), buttonSprite);
        diagnosisPositiveBtn = diagPos.GetComponent<Button>();
        diagnosisPositiveBtn.onClick.AddListener(DiagnoseStrokePositive);

        GameObject diagNeg = UIFactory.CreateButton(diagnosisPanel.GetComponent<RectTransform>(), "DiagNegBtn", "Kein Schlaganfall (Negativ)", new Vector2(0, -60), new Vector2(400, 60), buttonSprite);
        diagnosisNegativeBtn = diagNeg.GetComponent<Button>();
        diagnosisNegativeBtn.onClick.AddListener(DiagnoseStrokeNegative);

        // Load premium symptom sprites dynamically from Resources
        faceSymptomSprite = Resources.Load<Sprite>("stroke_face");
        armsSymptomSprite = Resources.Load<Sprite>("stroke_arms");
    }

    public void Activate()
    {
        if (panel == null) return;
        panel.SetActive(true);
        activeStep = 0;
        faceChecked = false;
        armsChecked = false;
        speechChecked = false;

        LocalizeUI();
        SetStep(0);
    }

    private void LocalizeUI()
    {
        if (scenarioDescText != null) scenarioDescText.text = LocalizationManager.Instance.Get("stroke_desc");
        if (faceButton != null) faceButton.GetComponentInChildren<Text>().text = LocalizationManager.Instance.Get("stroke_face");
        if (armsButton != null) armsButton.GetComponentInChildren<Text>().text = LocalizationManager.Instance.Get("stroke_arms");
        if (speechButton != null) speechButton.GetComponentInChildren<Text>().text = LocalizationManager.Instance.Get("stroke_speech");
        if (timeButton != null) timeButton.GetComponentInChildren<Text>().text = LocalizationManager.Instance.Get("stroke_time");
        if (diagnosisPositiveBtn != null) diagnosisPositiveBtn.GetComponentInChildren<Text>().text = LocalizationManager.Instance.Get("stroke_diag_positive");
        if (diagnosisNegativeBtn != null) diagnosisNegativeBtn.GetComponentInChildren<Text>().text = LocalizationManager.Instance.Get("stroke_diag_negative");
    }

    public void SetStep(int step)
    {
        activeStep = step;
        diagnosisPanel.SetActive(false);
        bubbleText.transform.parent.gameObject.SetActive(false);
        drawingArea.gameObject.SetActive(step < 2); // Only show drawingArea in Face (0) and Arms (1) steps!

        // Highlight active step button text colour
        ResetButtonColors();

        switch (step)
        {
            case 0:
                faceChecked = true;
                faceButton.GetComponentInChildren<Text>().color = new Color(0.1f, 0.4f, 0.8f);
                hintText.text = LocalizationManager.Instance.Get("stroke_face_hint");
                // Render our beautiful premium face symptom sprite cleanly
                drawingArea.color = Color.white; 
                if (faceSymptomSprite != null) drawingArea.sprite = faceSymptomSprite;
                else
                {
                    drawingArea.color = Color.red; // fallback procedurally tinted red box
                    drawingArea.sprite = null;
                }
                break;
            case 1:
                armsChecked = true;
                armsButton.GetComponentInChildren<Text>().color = new Color(0.1f, 0.4f, 0.8f);
                hintText.text = LocalizationManager.Instance.Get("stroke_arms_hint");
                drawingArea.color = Color.white;
                if (armsSymptomSprite != null) drawingArea.sprite = armsSymptomSprite;
                else
                {
                    drawingArea.color = Color.blue;
                    drawingArea.sprite = null;
                }
                break;
            case 2:
                speechChecked = true;
                speechButton.GetComponentInChildren<Text>().color = new Color(0.1f, 0.4f, 0.8f);
                hintText.text = LocalizationManager.Instance.Get("stroke_speech_hint");
                
                bubbleText.transform.parent.gameObject.SetActive(true);
                bubbleText.text = LocalizationManager.Instance.Get(LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE ? "stroke_garbled_bubble" : "stroke_garbled_bubble_en");

                // Play mumble audio
                GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
                if (bootstrap != null && bootstrap.muffledTalkSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(bootstrap.muffledTalkSound);
                }
                break;
            case 3:
                timeButton.GetComponentInChildren<Text>().color = new Color(0.1f, 0.4f, 0.8f);
                hintText.text = LocalizationManager.Instance.Get("stroke_time_hint");
                diagnosisPanel.SetActive(true);
                break;
        }
    }

    private void ResetButtonColors()
    {
        faceButton.GetComponentInChildren<Text>().color = Color.black;
        armsButton.GetComponentInChildren<Text>().color = Color.black;
        speechButton.GetComponentInChildren<Text>().color = Color.black;
        timeButton.GetComponentInChildren<Text>().color = Color.black;
    }

    private void DiagnoseStrokePositive()
    {
        // Correct diagnosis
        CompleteStrokeRescue(true);
    }

    private void DiagnoseStrokeNegative()
    {
        // Incorrect diagnosis
        CompleteStrokeRescue(false);
    }

    private void CompleteStrokeRescue(bool correct)
    {
        panel.SetActive(false);
        int score = correct ? 100 : 25;

        if (GameManager.Instance != null)
        {
            // Complete mission on GameManager with Stroke type
            GameManager.Instance.CompleteNewMission(GameManager.VictimType.Stroke, score);
            GameManager.Instance.strokeHelped = true;
            GameManager.Instance.SyncMissionProgress();
        }
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartIntroPhase();
        }
    }
}
