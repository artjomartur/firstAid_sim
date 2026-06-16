using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// A mail client for OS 98 showing messages from the boss for Career Mode.
/// </summary>
public class BossMailManager : MonoBehaviour
{
    public static BossMailManager Instance { get; private set; }

    private GameObject panel;
    private Text mailContentText;
    private Text titleText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        SetupMailUI();
    }

    public void ToggleWindow()
    {
        if (panel != null)
        {
            if (panel.activeSelf) DialogPopOut.Trigger(panel);
            else
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
                UpdateMailContent();
            }
        }
    }

    private void SetupMailUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;
        Sprite innerFrameSprite = bootstrap != null ? bootstrap.winInnerFrameSprite : null;

        // ── Main Window ──
        panel = new GameObject("BossMailWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 450);
        panelRT.anchoredPosition = new Vector2(0, 0); // Center

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.88f, 0.88f, 0.9f);

        // Window shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "BossMail_Shadow", new Vector2(4, -4), new Vector2(600, 450));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // ── Header Bar ──
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 205), new Vector2(592, 40));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        titleText = UIFactory.CreateText(header.transform, "Title", "📧 BossMail.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        titleText.color = Color.white;
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 40);

        // ── Close Button ──
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(270, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) { closeTxt.fontStyle = FontStyle.Bold; closeTxt.fontSize = 18; closeTxt.color = Color.black; }

        // ── Toolbar ──
        GameObject toolbar = UIFactory.CreateUIElement(panelRT, "Toolbar", new Vector2(0, 160), new Vector2(580, 40));
        Text toolbarTxt = UIFactory.CreateText(toolbar.transform, "Txt", "Von: Leitstelle (Dr. Sauer)   |   An: Azubi", new Vector2(10, 0), 16, TextAnchor.MiddleLeft);
        toolbarTxt.color = Color.black;
        toolbarTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(560, 40);

        // ── Content Area ──
        GameObject contentArea = UIFactory.CreateUIElement(panelRT, "ContentArea", new Vector2(0, -30), new Vector2(560, 320));
        if (innerFrameSprite != null)
        {
            Image caImg = contentArea.GetComponent<Image>();
            caImg.sprite = innerFrameSprite;
            caImg.type = Image.Type.Sliced;
            caImg.color = Color.white;
        }

        mailContentText = UIFactory.CreateText(contentArea.transform, "MailText", "", new Vector2(0, 0), 18, TextAnchor.UpperLeft);
        mailContentText.color = Color.black;
        mailContentText.fontStyle = FontStyle.Normal;
        
        RectTransform mailTextRT = mailContentText.GetComponent<RectTransform>();
        mailTextRT.anchorMin = new Vector2(0, 0);
        mailTextRT.anchorMax = new Vector2(1, 1);
        mailTextRT.sizeDelta = new Vector2(-20, -20);
        mailTextRT.anchoredPosition = new Vector2(0, -10);
    }

    private void UpdateMailContent()
    {
        if (CareerManager.Instance == null) return;
        int day = CareerManager.Instance.currentDay;
        
        bool isDe = true;
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.EN)
            isDe = false;

        string content = "";

        if (day == 1)
        {
            content = isDe ? 
                "Willkommen im Team, Azubi!\n\nHeute ist dein erster Tag. Es wird ein ruhiger Start. Wir erwarten ein paar leichte Schnittwunden und vielleicht leichte Verbrennungen am Grillplatz.\n\nPass gut auf und verwechsle nicht das Pflaster mit der Brandsalbe!\n\nGruß,\nDr. Sauer" :
                "Welcome to the team, rookie!\n\nToday is your first day. It will be a slow start. We expect some minor bleeding wounds and maybe slight burns at the BBQ area.\n\nPay attention and don't mix up the band-aids with the burn gel!\n\nRegards,\nDr. Sauer";
        }
        else if (day == 2)
        {
            content = isDe ? 
                "Guten Morgen,\n\ngestern hast du dich gut geschlagen. Heute ist das Wetter perfekt für Radfahrer – was bedeutet, wir erwarten Stürze! Halte die Dreieckstücher bereit.\nAuch mit bewusstlosen Personen ist an heißen Tagen zu rechnen. Stabile Seitenlage nicht vergessen!\n\nGruß,\nDr. Sauer" :
                "Good morning,\n\nyou did well yesterday. Today the weather is perfect for cyclists – which means we expect crashes! Keep your triangle bandages ready.\nAlso expect unconscious people on hot days. Don't forget the recovery position!\n\nRegards,\nDr. Sauer";
        }
        else if (day == 3)
        {
            content = isDe ? 
                "Achtung Azubi,\n\nheute ist Picknick-Tag im Park. Da verschluckt sich garantiert jemand am Essen. Weißt du noch, wie der Heimlich-Handgriff funktioniert?\nUnd pass auf freiliegende Kabel am Festival-Stand auf, wir hatten dort gestern Probleme mit Stromschlägen.\n\nSei wachsam!\n\nGruß,\nDr. Sauer" :
                "Attention rookie,\n\ntoday is picnic day in the park. Someone is guaranteed to choke on their food. Do you remember how the Heimlich maneuver works?\nAnd watch out for exposed cables at the festival booth, we had issues with electric shocks there yesterday.\n\nStay vigilant!\n\nRegards,\nDr. Sauer";
        }
        else
        {
            content = isDe ? 
                "Hallo,\n\ndu bist jetzt ein voll ausgebildeter Rettungssanitäter! Ab jetzt gibt es keine festen Schichten mehr für dich. Du übernimmst den freien Park-Dienst. Alles kann passieren, jederzeit.\n\nViel Erfolg!\n\nGruß,\nDr. Sauer" :
                "Hello,\n\nyou are now a fully trained paramedic! From now on, there are no fixed shifts for you. You take over the free park duty. Anything can happen, anytime.\n\nGood luck!\n\nRegards,\nDr. Sauer";
        }

        mailContentText.text = content;
        
        if (titleText != null)
        {
            titleText.text = isDe ? $"📧 BossMail.exe - Tag {day}" : $"📧 BossMail.exe - Day {day}";
        }
    }
}
