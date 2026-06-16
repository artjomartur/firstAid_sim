using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Right-click context menu on the desktop with retro Windows 98 styling.
/// Items: Refresh, New Folder (disabled), Properties, Screensaver, Wallpaper.
/// </summary>
public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager Instance;

    private GameObject contextMenu;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Update()
    {
        // Right-click detection
        if (Input.GetMouseButtonDown(1) && !isOpen)
        {
            // Only show context menu when in menu phase (not gameplay)
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null && gm.gameStarted) return;

            // Don't show if we're over a UI element that isn't the desktop
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Check if it's the desktop background (MenuPanel)
                PointerEventData ped = new PointerEventData(EventSystem.current);
                ped.position = Input.mousePosition;
                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(ped, results);
                
                bool isDesktop = false;
                if (results.Count > 0)
                {
                    string topName = results[0].gameObject.name;
                    if (topName == "MenuPanel" || topName == "DesktopBg" || topName.Contains("Wallpaper"))
                        isDesktop = true;
                }
                if (!isDesktop) return;
            }

            ShowContextMenu(Input.mousePosition);
        }

        // Close on left-click outside
        if (Input.GetMouseButtonDown(0) && isOpen)
        {
            CloseContextMenu();
        }

        // Close on Escape
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseContextMenu();
        }
    }

    private void ShowContextMenu(Vector2 screenPos)
    {
        if (contextMenu != null) Destroy(contextMenu);

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        // Convert screen position to canvas local position
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, canvas.worldCamera, out localPoint);

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;

        contextMenu = new GameObject("ContextMenu");
        contextMenu.transform.SetParent(canvasRT, false);

        RectTransform menuRT = contextMenu.AddComponent<RectTransform>();
        menuRT.pivot = new Vector2(0, 1); // Top-left corner at mouse
        menuRT.anchoredPosition = localPoint;
        menuRT.sizeDelta = new Vector2(200, 230);

        Image menuBg = contextMenu.AddComponent<Image>();
        if (baseSprite != null)
        {
            menuBg.sprite = baseSprite;
            menuBg.type = Image.Type.Sliced;
        }
        else menuBg.color = new Color(0.85f, 0.85f, 0.85f);

        // Shadow
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(contextMenu.transform, false);
        RectTransform shadowRT = shadow.AddComponent<RectTransform>();
        shadowRT.anchorMin = Vector2.zero;
        shadowRT.anchorMax = Vector2.one;
        shadowRT.sizeDelta = new Vector2(4, 4);
        shadowRT.anchoredPosition = new Vector2(2, -2);
        Image shadowImg = shadow.AddComponent<Image>();
        shadowImg.color = new Color(0, 0, 0, 0.3f);
        shadowImg.raycastTarget = false;
        shadow.transform.SetAsFirstSibling();

        contextMenu.transform.SetAsLastSibling();

        // Menu items
        string[] labels = { "🔄 Aktualisieren", "📁 Neuer Ordner", "──────────", "🖼️ Hintergrundbild...", "🌙 Bildschirmschoner", "💀 BSOD auslösen", "──────────", "ℹ️ Eigenschaften" };
        float itemHeight = 28f;
        float startY = -5f;

        for (int i = 0; i < labels.Length; i++)
        {
            float y = startY - i * itemHeight;

            if (labels[i].StartsWith("───"))
            {
                // Separator
                GameObject sep = new GameObject("Sep_" + i);
                sep.transform.SetParent(menuRT, false);
                RectTransform sepRT = sep.AddComponent<RectTransform>();
                sepRT.anchoredPosition = new Vector2(100, y);
                sepRT.sizeDelta = new Vector2(180, 2);
                Image sepImg = sep.AddComponent<Image>();
                sepImg.color = new Color(0.6f, 0.6f, 0.6f);
                sepImg.raycastTarget = false;
                continue;
            }

            int idx = i;
            GameObject item = new GameObject("MenuItem_" + i);
            item.transform.SetParent(menuRT, false);
            RectTransform itemRT = item.AddComponent<RectTransform>();
            itemRT.anchoredPosition = new Vector2(100, y);
            itemRT.sizeDelta = new Vector2(190, itemHeight);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0, 0, 0, 0);

            Button itemBtn = item.AddComponent<Button>();
            itemBtn.targetGraphic = itemBg;

            // Hover effect
            ColorBlock cb = itemBtn.colors;
            cb.highlightedColor = new Color(0.1f, 0.1f, 0.5f, 1f);
            cb.pressedColor = new Color(0.1f, 0.1f, 0.4f, 1f);
            itemBtn.colors = cb;

            Text itemText = UIFactory.CreateText(item.transform, "Label", labels[i], Vector2.zero, 15, TextAnchor.MiddleLeft);
            itemText.color = Color.black;
            itemText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            itemText.GetComponent<RectTransform>().anchorMax = Vector2.one;
            itemText.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, 0);
            itemText.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);

            // Disable "New Folder"
            if (labels[i].Contains("Neuer Ordner"))
            {
                itemBtn.interactable = false;
                itemText.color = new Color(0.6f, 0.6f, 0.6f);
            }

            // Actions
            switch (idx)
            {
                case 0: // Refresh
                    itemBtn.onClick.AddListener(() => { CloseContextMenu(); /* No-op refresh animation */ });
                    break;
                case 3: // Wallpaper
                    itemBtn.onClick.AddListener(() => {
                        CloseContextMenu();
                        if (WallpaperManager.Instance != null) WallpaperManager.Instance.ToggleWindow();
                    });
                    break;
                case 4: // Screensaver
                    itemBtn.onClick.AddListener(() => {
                        CloseContextMenu();
                        if (ScreensaverManager.Instance != null) ScreensaverManager.Instance.ForceActivate();
                    });
                    break;
                case 5: // BSOD
                    itemBtn.onClick.AddListener(() => {
                        CloseContextMenu();
                        if (BSODManager.Instance != null) BSODManager.Instance.TriggerBSOD();
                    });
                    break;
                case 7: // Properties
                    itemBtn.onClick.AddListener(() => {
                        CloseContextMenu();
                        ShowProperties();
                    });
                    break;
            }
        }

        isOpen = true;
    }

    private void CloseContextMenu()
    {
        if (contextMenu != null) Destroy(contextMenu);
        contextMenu = null;
        isOpen = false;
    }

    private void ShowProperties()
    {
        // Show a small info dialog
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (bootstrap == null) return;

        GameObject dialog = bootstrap.CreateWindowsDialog(canvas.transform, "PropsDialog", "Systemeigenschaften", Vector2.zero, new Vector2(400, 300));

        string info = "System: First Aid OS 98\n" +
                       "Version: 2.0 Build 2026\n" +
                       "Prozessor: Intel Pentium MMX\n" +
                       "Arbeitsspeicher: 64 MB RAM\n" +
                       "Festplatte: 2.1 GB\n\n" +
                       "Registriert für: Erste-Hilfe-Schüler\n" +
                       "Produktschlüssel: FA1D-OS98-2026-HELP";

        Text infoText = UIFactory.CreateText(dialog.transform, "Info", info, new Vector2(0, -20), 16, TextAnchor.MiddleCenter);
        infoText.color = Color.black;
        infoText.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 200);

        // OK Button
        GameObject okBtn = UIFactory.CreateButton(dialog.GetComponent<RectTransform>(), "OKBtn", "OK", new Vector2(0, -120), new Vector2(120, 40), bootstrap.winButtonSprite);
        okBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(dialog, () => Destroy(dialog)));
        Text okTxt = okBtn.GetComponentInChildren<Text>();
        if (okTxt != null) okTxt.color = Color.black;
    }
}
