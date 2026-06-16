using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Achievement/Badge display window showing all possible badges and their unlock status.
/// Follows the ExamManager self-contained UI pattern.
/// </summary>
public class BadgeManager : MonoBehaviour
{
    public static BadgeManager Instance;

    public GameObject panel;
    private RectTransform panelRT;
    private Transform badgeContainer;

    private struct BadgeDef
    {
        public string emoji;
        public string nameDe;
        public string nameEn;
        public string descDe;
        public string descEn;
        public string matchKey; // The badge string from ScoreManager.GetBadges()
    }

    private BadgeDef[] allBadges;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupBadgeDefinitions();
            SetupBadgeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleWindow()
    {
        if (panel != null)
        {
            bool newState = !panel.activeSelf;
            panel.SetActive(newState);
            if (newState)
            {
                panel.transform.SetAsLastSibling();
                RefreshBadges();
            }
        }
    }

    private void SetupBadgeDefinitions()
    {
        allBadges = new BadgeDef[]
        {
            new BadgeDef { emoji = "🧠", nameDe = "Quiz-Master", nameEn = "Quiz Master", descDe = "Alle Quiz-Fragen richtig", descEn = "All quiz answers correct", matchKey = "🧠 Quiz-Master" },
            new BadgeDef { emoji = "📖", nameDe = "Theorie-Versteher", nameEn = "Theory Pro", descDe = "Über 50% Quiz-Score", descEn = "Over 50% quiz score", matchKey = "📖 Theorie-Versteher" },
            new BadgeDef { emoji = "❤️", nameDe = "Perfekter Lebensretter", nameEn = "Perfect Lifesaver", descDe = "CPR perfekt ausgeführt", descEn = "CPR performed perfectly", matchKey = "❤️ Perfekter Lebensretter" },
            new BadgeDef { emoji = "👍", nameDe = "Solider Helfer", nameEn = "Solid Helper", descDe = "CPR fast perfekt", descEn = "CPR nearly perfect", matchKey = "👍 Solider Helfer" },
            new BadgeDef { emoji = "🩹", nameDe = "Verband-Profi", nameEn = "Bandage Pro", descDe = "Verband perfekt angelegt", descEn = "Bandage applied perfectly", matchKey = "🩹 Verband-Profi" },
            new BadgeDef { emoji = "💼", nameDe = "Koffer-Profi", nameEn = "Kit Pro", descDe = "Notfallkoffer perfekt gepackt", descEn = "Emergency kit packed perfectly", matchKey = "💼 Koffer-Profi" },
            new BadgeDef { emoji = "🐍", nameDe = "Snake-Meister", nameEn = "Snake Master", descDe = "Snake Highscore ≥ 20", descEn = "Snake high score ≥ 20", matchKey = "🐍 Snake-Meister" },
            new BadgeDef { emoji = "💣", nameDe = "Minenräumer", nameEn = "Mine Sweeper", descDe = "Minesweeper in unter 120s gewonnen", descEn = "Minesweeper won under 120s", matchKey = "💣 Minenräumer" },
            new BadgeDef { emoji = "🦴", nameDe = "Knochen-Profi", nameEn = "Bone Pro", descDe = "Schiene erfolgreich angelegt", descEn = "Splint successfully applied", matchKey = "🦴 Knochen-Profi" },
            new BadgeDef { emoji = "💉", nameDe = "Allergie-Retter", nameEn = "Allergy Savior", descDe = "EpiPen richtig angewendet", descEn = "EpiPen correctly applied", matchKey = "💉 Allergie-Retter" },
            new BadgeDef { emoji = "🐶", nameDe = "Tierretter", nameEn = "Animal Rescuer", descDe = "Erste Hilfe beim Hund geleistet", descEn = "First aid provided to dog", matchKey = "🐶 Tierretter" },
            new BadgeDef { emoji = "📚", nameDe = "Erste-Hilfe-Schüler", nameEn = "First Aid Student", descDe = "Dein erster Badge!", descEn = "Your first badge!", matchKey = "📚 Erste-Hilfe-Schüler" },
        };
    }

    private void SetupBadgeUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        float windowWidth = 520;
        float windowHeight = 680;

        panel = new GameObject("BadgeWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(windowWidth, windowHeight);
        panelRT.anchoredPosition = new Vector2(50, -20);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.85f, 0.85f, 0.85f);

        // Shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "BadgeWindow_Shadow", new Vector2(4, -4), new Vector2(windowWidth, windowHeight));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // Header
        float headerHeight = 40f;
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, windowHeight / 2f - headerHeight / 2f), new Vector2(windowWidth - 4, headerHeight));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        Text headerText = UIFactory.CreateText(header.transform, "Title", "🏆 Erfolge.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;
        headerText.fontStyle = FontStyle.Bold;
        headerText.GetComponent<RectTransform>().sizeDelta = new Vector2(windowWidth - 60, headerHeight);

        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(windowWidth / 2f - 28, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // Accent
        GameObject accentLine = UIFactory.CreateUIElement(panelRT, "AccentLine", new Vector2(0, windowHeight / 2f - headerHeight - 2f), new Vector2(windowWidth - 4, 4));
        accentLine.GetComponent<Image>().color = new Color(1f, 0.85f, 0.1f);
        accentLine.AddComponent<AccentPulse>();

        // Badge container
        float contentTop = windowHeight / 2f - headerHeight - 15f;
        GameObject container = new GameObject("BadgeContainer");
        container.transform.SetParent(panelRT, false);
        RectTransform containerRT = container.AddComponent<RectTransform>();
        containerRT.anchoredPosition = new Vector2(0, contentTop - 280f);
        containerRT.sizeDelta = new Vector2(windowWidth - 30, 560);
        badgeContainer = container.transform;
    }

    public void RefreshBadges()
    {
        if (badgeContainer == null) return;

        // Clear existing entries
        foreach (Transform child in badgeContainer)
            Destroy(child.gameObject);

        List<string> earned = ScoreManager.Instance != null ? ScoreManager.Instance.GetBadges() : new List<string>();

        // Add snake highscore badge check
        int snakeHS = PlayerPrefs.GetInt("SnakeHighScore", 0);
        if (snakeHS >= 20 && !earned.Contains("🐍 Snake-Meister"))
            earned.Add("🐍 Snake-Meister");

        RectTransform containerRT = badgeContainer.GetComponent<RectTransform>();
        float itemHeight = 50f;
        float startY = containerRT.sizeDelta.y / 2f - 10f;

        for (int i = 0; i < allBadges.Length; i++)
        {
            BadgeDef bd = allBadges[i];
            bool unlocked = earned.Contains(bd.matchKey);

            float y = startY - i * itemHeight;

            GameObject row = new GameObject("Badge_" + i);
            row.transform.SetParent(badgeContainer, false);
            RectTransform rowRT = row.AddComponent<RectTransform>();
            rowRT.anchoredPosition = new Vector2(0, y);
            rowRT.sizeDelta = new Vector2(containerRT.sizeDelta.x - 10, itemHeight - 4);

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = unlocked ? new Color(0.95f, 0.98f, 0.9f, 1f) : new Color(0.8f, 0.8f, 0.8f, 0.5f);
            rowBg.raycastTarget = false;

            // Emoji
            Text emojiTxt = UIFactory.CreateText(row.transform, "Emoji", unlocked ? bd.emoji : "🔒", new Vector2(-200, 0), 26, TextAnchor.MiddleCenter);
            emojiTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 46);

            // Name
            string name = unlocked ? bd.nameDe : "???";
            Text nameTxt = UIFactory.CreateText(row.transform, "Name", name, new Vector2(-80, 8), 18, TextAnchor.MiddleLeft);
            nameTxt.fontStyle = FontStyle.Bold;
            nameTxt.color = unlocked ? Color.black : new Color(0.5f, 0.5f, 0.5f);
            nameTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 24);

            // Description
            string desc = unlocked ? bd.descDe : "Noch nicht freigeschaltet";
            Text descTxt = UIFactory.CreateText(row.transform, "Desc", desc, new Vector2(-80, -12), 14, TextAnchor.MiddleLeft);
            descTxt.color = unlocked ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            descTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 20);
        }
    }
}
