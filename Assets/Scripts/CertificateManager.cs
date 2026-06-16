using UnityEngine;
using UnityEngine.UI;

public class CertificateManager : MonoBehaviour
{
    public static CertificateManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject panel;
    public InputField nameInputField;
    public Text certifiedNameText;
    public Text coinsText;
    public GameObject certificateCanvas; // The specific sub-frame to print
    public Button printButton;

    [Header("Lock Settings")]
    public GameObject lockOverlay;
    public Text lockText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupCertificateUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupCertificateUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite innerFrameSprite = bootstrap != null ? bootstrap.winInnerFrameSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        // Window Panel
        panel = new GameObject("CertificateWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(850, 680);
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

        // Window shadow (parented to panel so it moves with it)
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "CertificateWindow_Shadow", new Vector2(4, -4), new Vector2(850, 680));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        // Header
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 315), new Vector2(840, 40));
        header.AddComponent<WindowDragger>();
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

        Text headerText = UIFactory.CreateText(header.transform, "Title", "Zertifikat_Generator.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;

        // Close Button
        GameObject closeBtn = UIFactory.CreateButton(headerRT, "CloseButton", "X", new Vector2(395, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeBtnText = closeBtn.GetComponentInChildren<Text>();
        if (closeBtnText != null) closeBtnText.color = Color.black;

        // Input Field for User Name
        GameObject inputObj = new GameObject("NameInputField");
        inputObj.transform.SetParent(panelRT, false);
        RectTransform inputRT = inputObj.AddComponent<RectTransform>();
        inputRT.anchoredPosition = new Vector2(-150, 240);
        inputRT.sizeDelta = new Vector2(300, 40);

        Image inputImg = inputObj.AddComponent<Image>();
        inputImg.color = Color.white;

        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(inputObj.transform, false);
        RectTransform placeRT = placeholder.AddComponent<RectTransform>();
        placeRT.anchorMin = Vector2.zero;
        placeRT.anchorMax = Vector2.one;
        placeRT.sizeDelta = new Vector2(-10, -10);

        Text placeTxt = placeholder.AddComponent<Text>();
        placeTxt.text = "Ihr Name hier...";
        placeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeTxt.fontSize = 20;
        placeTxt.color = Color.gray;

        GameObject textShow = new GameObject("Text");
        textShow.transform.SetParent(inputObj.transform, false);
        RectTransform showRT = textShow.AddComponent<RectTransform>();
        showRT.anchorMin = Vector2.zero;
        showRT.anchorMax = Vector2.one;
        showRT.sizeDelta = new Vector2(-10, -10);

        Text txtComponent = textShow.AddComponent<Text>();
        txtComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtComponent.fontSize = 20;
        txtComponent.color = Color.black;

        nameInputField = inputObj.AddComponent<InputField>();
        nameInputField.placeholder = placeTxt;
        nameInputField.textComponent = txtComponent;
        nameInputField.onValueChanged.AddListener(UpdateNameOnCertificate);

        // Certificate Frame Container
        certificateCanvas = UIFactory.CreateUIElement(panelRT, "CertificateFrame", new Vector2(0, -60), new Vector2(800, 480));
        certificateCanvas.GetComponent<Image>().sprite = innerFrameSprite;
        certificateCanvas.GetComponent<Image>().type = Image.Type.Sliced;
        certificateCanvas.GetComponent<Image>().color = new Color(0.98f, 0.96f, 0.9f); // Elegant papyrus tint

        // Inside Border Line
        GameObject border = UIFactory.CreateUIElement(certificateCanvas.GetComponent<RectTransform>(), "BorderLine", Vector2.zero, new Vector2(760, 440));
        border.GetComponent<Image>().color = new Color(0.6f, 0.4f, 0.2f, 0.4f); // Golden brown thin border

        // Certificate Text Elements
        Text certTitle = UIFactory.CreateText(border.transform, "TitleText", "ZERTIFIKAT DER ERSTEN HILFE", new Vector2(0, 150), 32, TextAnchor.MiddleCenter);
        certTitle.color = new Color(0.4f, 0.2f, 0f); // Dark gold brown

        Text certText1 = UIFactory.CreateText(border.transform, "CertifiedText", "Dieses Dokument bescheinigt stolz, dass", new Vector2(0, 80), 20, TextAnchor.MiddleCenter);
        certText1.color = Color.black;

        certifiedNameText = UIFactory.CreateText(border.transform, "CertifiedNameText", "MAX MUSTERMANN", new Vector2(0, 0), 28, TextAnchor.MiddleCenter);
        certifiedNameText.color = new Color(0.8f, 0.1f, 0.1f); // Heroic crimson red
        certifiedNameText.fontStyle = FontStyle.Bold;

        Text certText2 = UIFactory.CreateText(border.transform, "CertBody", "erfolgreich alle Erste-Hilfe-Simulationen im Park absolviert und Leben gerettet hat.", new Vector2(0, -60), 20, TextAnchor.MiddleCenter);
        certText2.color = Color.black;
        certText2.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 100);

        coinsText = UIFactory.CreateText(border.transform, "ScoreCoins", "Gesammelte Münzen: 0", new Vector2(0, -130), 20, TextAnchor.MiddleCenter);
        coinsText.color = Color.black;

        Text signText = UIFactory.CreateText(border.transform, "Signature", "Unterschrift: AG Serious Games (TU Darmstadt)", new Vector2(0, -180), 16, TextAnchor.MiddleCenter);
        signText.color = Color.gray;

        // Print Button
        GameObject printBtnObj = UIFactory.CreateButton(panelRT, "PrintButton", "Drucken (Simulation)", new Vector2(250, 240), new Vector2(240, 42), buttonSprite);
        printButton = printBtnObj.GetComponent<Button>();
        printButton.onClick.AddListener(PrintCertificateSim);

        // Lock Overlay
        lockOverlay = UIFactory.CreateUIElement(panelRT, "LockOverlay", new Vector2(0, -30), new Vector2(810, 520));
        UIFactory.SetupImage(lockOverlay, innerFrameSprite, false);
        lockOverlay.GetComponent<Image>().type = Image.Type.Sliced;
        lockOverlay.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 1f);

        lockText = UIFactory.CreateText(lockOverlay.transform, "LockText", "Prüfung ausstehend...", Vector2.zero, 20, TextAnchor.MiddleCenter);
        lockText.color = new Color(0.7f, 0.15f, 0.15f);
        lockText.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 460);
        lockText.supportRichText = true;
        lockOverlay.SetActive(false);
    }

    public void ToggleWindow()
    {
        if (panel == null) return;
        bool isAct = !panel.activeSelf;
        panel.SetActive(isAct);

        if (isAct)
        {
            // Bring to front
            panel.transform.SetAsLastSibling();

            bool passed = PlayerPrefs.GetInt("ExamPassed", 0) == 1;

            if (passed)
            {
                if (lockOverlay != null) lockOverlay.SetActive(false);
                if (certificateCanvas != null) certificateCanvas.SetActive(true);
                if (nameInputField != null) nameInputField.gameObject.SetActive(true);
                if (printButton != null) printButton.gameObject.SetActive(true);

                // Sync details
                int scoreVal = ScoreManager.Instance != null ? ScoreManager.Instance.lifeCoins : 0;

                // Translate / Localize text fields
                if (nameInputField != null && nameInputField.placeholder != null)
                    nameInputField.placeholder.GetComponent<Text>().text = LocalizationManager.Instance.Get("cert_placeholder_name");

                var titleText = certificateCanvas.transform.Find("BorderLine/TitleText")?.GetComponent<Text>();
                if (titleText != null) titleText.text = LocalizationManager.Instance.Get("cert_title");

                var certText1 = certificateCanvas.transform.Find("BorderLine/CertifiedText")?.GetComponent<Text>();
                if (certText1 != null) certText1.text = LocalizationManager.Instance.Get("cert_body1");

                var certBody = certificateCanvas.transform.Find("BorderLine/CertBody")?.GetComponent<Text>();
                if (certBody != null) certBody.text = LocalizationManager.Instance.Get("cert_body2");

                if (coinsText != null) coinsText.text = LocalizationManager.Instance.Get("cert_score") + scoreVal;

                var signatureText = certificateCanvas.transform.Find("BorderLine/Signature")?.GetComponent<Text>();
                if (signatureText != null) signatureText.text = LocalizationManager.Instance.Get("cert_signature");

                UpdateNameOnCertificate(nameInputField.text);
            }
            else
            {
                if (lockOverlay != null) lockOverlay.SetActive(true);
                if (certificateCanvas != null) certificateCanvas.SetActive(false);
                if (nameInputField != null) nameInputField.gameObject.SetActive(false);
                if (printButton != null) printButton.gameObject.SetActive(false);

                if (lockText != null)
                {
                    lockText.text = LocalizationManager.Instance.Get("cert_locked_message");
                }

                // Play error sound on attempt
                if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound);
                }
            }
        }
    }

    private void UpdateNameOnCertificate(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            certifiedNameText.text = "MAX MUSTERMANN";
        }
        else
        {
            certifiedNameText.text = name.ToUpper();
        }
    }

    private void PrintCertificateSim()
    {
        // Play printing SFX
        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (bootstrap != null && bootstrap.telephoneSound != null && AudioManager.Instance != null)
        {
            // Simulate dot matrix print noise using a high pitch sound
            AudioManager.Instance.PlaySFX(bootstrap.telephoneSound, 0.5f);
        }

        // Mute button temporarily and flash the certificate screen
        StartCoroutine(PrintFlashRoutine());
    }

    private System.Collections.IEnumerator PrintFlashRoutine()
    {
        Image img = certificateCanvas.GetComponent<Image>();
        Color originalCol = img.color;
        
        // High flash
        img.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        img.color = originalCol;
        yield return new WaitForSeconds(0.1f);
        img.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        img.color = originalCol;
    }
}
