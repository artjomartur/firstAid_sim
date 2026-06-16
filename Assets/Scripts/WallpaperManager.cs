using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles wallpaper selection and rendering for the desktop.
/// Stores selection in PlayerPrefs.
/// </summary>
public class WallpaperManager : MonoBehaviour
{
    public static WallpaperManager Instance;

    public GameObject panel;
    private Image desktopBackground;
    private int currentWallpaperIndex = 0;

    private readonly Color[] bgColors = {
        new Color(0.0f, 0.5f, 0.5f), // Classic Win95 Teal (Default)
        new Color(0.2f, 0.4f, 0.8f), // Windows 98 Blue
        new Color(0.1f, 0.1f, 0.1f), // Dark Mode
        new Color(0.5f, 0.0f, 0.0f), // Deep Red
        new Color(0.0f, 0.4f, 0.2f), // Forest Green
        new Color(0.3f, 0.1f, 0.4f)  // Purple
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentWallpaperIndex = PlayerPrefs.GetInt("SelectedWallpaper", 0);
            if (currentWallpaperIndex < 0 || currentWallpaperIndex >= bgColors.Length) currentWallpaperIndex = 0;
            
            // Wait for GameBootstrap to initialize desktop before applying
            StartCoroutine(InitWallpaper());
            SetupWallpaperUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator InitWallpaper()
    {
        yield return new WaitForSeconds(0.5f);
        ApplyWallpaper();
    }

    public void ToggleWindow()
    {
        if (panel != null)
        {
            if (panel.activeSelf) DialogPopOut.Trigger(panel);
            else { panel.SetActive(true); panel.transform.SetAsLastSibling(); }
        }
    }

    private void SetupWallpaperUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        float windowWidth = 400;
        float windowHeight = 350;

        panel = new GameObject("WallpaperWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(windowWidth, windowHeight);
        panelRT.anchoredPosition = new Vector2(-50, 50);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.85f, 0.85f, 0.85f);

        // Shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "WallWindow_Shadow", new Vector2(4, -4), new Vector2(windowWidth, windowHeight));
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

        Text headerText = UIFactory.CreateText(header.transform, "Title", "🖼️ Darstellung.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;
        headerText.fontStyle = FontStyle.Bold;
        headerText.GetComponent<RectTransform>().sizeDelta = new Vector2(windowWidth - 60, headerHeight);

        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(windowWidth / 2f - 28, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // Content
        float contentY = 40f;
        Text instTxt = UIFactory.CreateText(panel.transform, "Inst", "Hintergrundfarbe auswählen:", new Vector2(0, contentY + 60), 16, TextAnchor.MiddleCenter);
        instTxt.color = Color.black;

        // Color buttons grid
        float startX = -100f;
        float startYPos = contentY;
        
        for (int i = 0; i < bgColors.Length; i++)
        {
            int index = i;
            int row = i / 3;
            int col = i % 3;
            
            Vector2 pos = new Vector2(startX + (col * 100), startYPos - (row * 100));
            
            GameObject btnObj = UIFactory.CreateButton(panelRT, "ColorBtn_" + i, "", pos, new Vector2(80, 80), buttonSprite);
            Button btn = btnObj.GetComponent<Button>();
            
            GameObject colorBlock = UIFactory.CreateUIElement(btnObj.transform as RectTransform, "Color", Vector2.zero, new Vector2(60, 60));
            colorBlock.GetComponent<Image>().color = bgColors[i];
            
            btn.onClick.AddListener(() => SelectWallpaper(index));
        }
    }

    private void SelectWallpaper(int index)
    {
        if (index >= 0 && index < bgColors.Length)
        {
            currentWallpaperIndex = index;
            PlayerPrefs.SetInt("SelectedWallpaper", currentWallpaperIndex);
            PlayerPrefs.Save();
            ApplyWallpaper();
        }
    }

    private void ApplyWallpaper()
    {
        if (desktopBackground == null)
        {
            // Find the desktop background. It's usually the MenuPanel or a specific image.
            GameObject menuPanel = GameObject.Find("MenuPanel");
            if (menuPanel != null)
            {
                // We'll look for an image that isn't the window base sprite (which might be the taskbar container)
                // The main background is usually the one filling the screen.
                Image[] images = menuPanel.GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    if (img.gameObject.name == "DesktopBg" || img.gameObject.name == "MenuPanel")
                    {
                        desktopBackground = img;
                        break;
                    }
                }
            }
        }

        if (desktopBackground != null)
        {
            desktopBackground.color = bgColors[currentWallpaperIndex];
        }
    }
}
