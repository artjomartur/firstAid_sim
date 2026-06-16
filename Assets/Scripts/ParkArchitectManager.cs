using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ParkArchitectManager : MonoBehaviour
{
    public static ParkArchitectManager Instance { get; private set; }

    private GameObject panel;
    private int selectedTool = 1; // 0=Clear, 1=Tree, 2=Bench, 3=Bush
    private const int GridSizeX = 12;
    private const int GridSizeY = 8;
    private int[,] mapData = new int[GridSizeX, GridSizeY];
    private Image[,] gridImages = new Image[GridSizeX, GridSizeY];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        LoadMap();
        SetupUI();
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
            }
        }
    }

    private void LoadMap()
    {
        string data = PlayerPrefs.GetString("CustomParkData", "");
        if (string.IsNullOrEmpty(data)) return;
        
        string[] cells = data.Split(',');
        if (cells.Length == GridSizeX * GridSizeY)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    int.TryParse(cells[y * GridSizeX + x], out mapData[x, y]);
                }
            }
        }
    }

    public void SaveMap()
    {
        List<string> cells = new List<string>();
        for (int y = 0; y < GridSizeY; y++)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                cells.Add(mapData[x, y].ToString());
            }
        }
        PlayerPrefs.SetString("CustomParkData", string.Join(",", cells));
        PlayerPrefs.Save();

        // Notify CustomParkBuilder to rebuild
        if (CustomParkBuilder.Instance != null)
            CustomParkBuilder.Instance.BuildPark();
    }

    private void SetupUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;
        Sprite innerFrameSprite = bootstrap != null ? bootstrap.winInnerFrameSprite : null;

        // ── Main Window ──
        panel = new GameObject("ParkArchitectWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(650, 550);
        panelRT.anchoredPosition = new Vector2(0, 0); // Center

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null) { bgImage.sprite = baseSprite; bgImage.type = Image.Type.Sliced; }
        else bgImage.color = new Color(0.88f, 0.88f, 0.9f);

        // Window shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "Shadow", new Vector2(4, -4), new Vector2(650, 550));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // ── Header Bar ──
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 255), new Vector2(642, 40));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        Text titleText = UIFactory.CreateText(header.transform, "Title", "🏗️ ParkArchitect.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        titleText.color = Color.white;
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 40);

        // Close Button
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(295, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        closeBtn.GetComponentInChildren<Text>().color = Color.black;

        // ── Toolbar ──
        GameObject toolbar = UIFactory.CreateUIElement(panelRT, "Toolbar", new Vector2(0, 210), new Vector2(630, 40));
        string[] toolNames = { "Eraser", "Tree", "Bench", "Bush" };
        Color[] toolColors = { Color.white, new Color(0.2f, 0.8f, 0.2f), new Color(0.6f, 0.3f, 0.1f), new Color(0.1f, 0.5f, 0.1f) };
        
        for (int i = 0; i < toolNames.Length; i++)
        {
            int index = i;
            GameObject tBtn = CreateToolButton(toolbar.transform, toolNames[i], toolColors[i], new Vector2(-250 + (i * 100), 0), buttonSprite);
            tBtn.GetComponent<Button>().onClick.AddListener(() => { selectedTool = index; });
        }

        GameObject saveBtn = CreateToolButton(toolbar.transform, "Save & Build", Color.yellow, new Vector2(220, 0), buttonSprite);
        saveBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 30);
        saveBtn.GetComponent<Button>().onClick.AddListener(SaveMap);

        // ── Grid Area ──
        GameObject gridArea = UIFactory.CreateUIElement(panelRT, "GridArea", new Vector2(0, -20), new Vector2(620, 420));
        if (innerFrameSprite != null) { Image gaImg = gridArea.GetComponent<Image>(); gaImg.sprite = innerFrameSprite; gaImg.type = Image.Type.Sliced; }
        
        float cellW = 50f;
        float cellH = 50f;
        float startX = -((GridSizeX * cellW) / 2f) + (cellW / 2f);
        float startY = ((GridSizeY * cellH) / 2f) - (cellH / 2f);

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                int cx = x;
                int cy = y;
                GameObject cell = UIFactory.CreateButton(gridArea.GetComponent<RectTransform>(), $"Cell_{x}_{y}", "", new Vector2(startX + (x * cellW), startY - (y * cellH)), new Vector2(48, 48), null);
                Image cImg = cell.GetComponent<Image>();
                gridImages[x, y] = cImg;
                
                Button cBtn = cell.GetComponent<Button>();
                cBtn.onClick.AddListener(() => {
                    mapData[cx, cy] = selectedTool;
                    UpdateCellColor(cx, cy);
                });
                
                UpdateCellColor(cx, cy);
            }
        }
    }

    private void UpdateCellColor(int x, int y)
    {
        int val = mapData[x, y];
        Color c = Color.white;
        if (val == 1) c = new Color(0.2f, 0.8f, 0.2f); // Tree
        else if (val == 2) c = new Color(0.6f, 0.3f, 0.1f); // Bench
        else if (val == 3) c = new Color(0.1f, 0.5f, 0.1f); // Bush
        gridImages[x, y].color = c;
    }

    private GameObject CreateToolButton(Transform parent, string label, Color iconColor, Vector2 pos, Sprite btnSprite)
    {
        GameObject btn = UIFactory.CreateButton(parent.GetComponent<RectTransform>(), label, label, pos, new Vector2(80, 30), btnSprite);
        Text txt = btn.GetComponentInChildren<Text>();
        txt.color = Color.black;
        txt.fontSize = 12;
        
        GameObject icon = UIFactory.CreateUIElement(btn.GetComponent<RectTransform>(), "Icon", new Vector2(-30, 0), new Vector2(16, 16));
        icon.GetComponent<Image>().color = iconColor;
        
        return btn;
    }
}
