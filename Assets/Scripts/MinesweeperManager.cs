using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classic Minesweeper minigame with retro Windows 98 styling.
/// 8x8 grid, 10 mines, left-click to dig, right-click/flag-mode to flag.
/// First click is always safe. Winning grants 20 coins.
/// </summary>
public class MinesweeperManager : MonoBehaviour
{
    public static MinesweeperManager Instance;

    private const int GRID_W = 8;
    private const int GRID_H = 8;
    private const int MINE_COUNT = 10;
    private const float CELL_SIZE = 36f;

    // Cell state
    private int[,] grid; // -1 = mine, 0..8 = neighbor count
    private bool[,] revealed;
    private bool[,] flagged;

    // UI
    public GameObject panel;
    private RectTransform panelRT;
    private Text mineCounterText;
    private Text timerText;
    private Text smileyText;
    private Button smileyButton;
    private GameObject[,] cellObjects;
    private Text[,] cellTexts;
    private Image[,] cellImages;
    private GameObject flagToggleBtn;
    private Text flagToggleText;

    // Game state
    private bool gameStarted = false; // mines placed?
    private bool gameOver = false;
    private bool gameWon = false;
    private bool flagMode = false;
    private int flagCount = 0;
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    // Color mapping for numbers (classic Minesweeper palette)
    private static readonly Color[] numberColors = {
        Color.clear,                          // 0 - not shown
        new Color(0f, 0f, 1f),                // 1 - Blue
        new Color(0f, 0.5f, 0f),              // 2 - Green
        new Color(1f, 0f, 0f),                // 3 - Red
        new Color(0f, 0f, 0.5f),              // 4 - Dark Blue
        new Color(0.5f, 0f, 0f),              // 5 - Dark Red
        new Color(0f, 0.5f, 0.5f),            // 6 - Teal
        Color.black,                          // 7 - Black
        new Color(0.5f, 0.5f, 0.5f)           // 8 - Gray
    };

    // Retro cell colors
    private Color cellUnrevealed = new Color(0.75f, 0.75f, 0.75f, 1f);
    private Color cellRevealed = new Color(0.85f, 0.85f, 0.85f, 1f);
    private Color cellMine = new Color(1f, 0.3f, 0.3f, 1f);
    private Color headerBg = new Color(0.75f, 0.75f, 0.75f, 1f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupMinesweeperUI();
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
            if (panel.activeSelf) DialogPopOut.Trigger(panel);
            else { panel.SetActive(true); panel.transform.SetAsLastSibling(); }
        }
    }

    private void SetupMinesweeperUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        float windowWidth = GRID_W * CELL_SIZE + 60;
        float windowHeight = GRID_H * CELL_SIZE + 200;

        // Main Window Panel
        panel = new GameObject("MinesweeperWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(windowWidth, windowHeight);
        panelRT.anchoredPosition = new Vector2(120, 30);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.85f, 0.85f, 0.85f);

        // Window shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "MineWindow_Shadow", new Vector2(4, -4), new Vector2(windowWidth, windowHeight));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // Header Title Bar
        float headerHeight = 40f;
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, windowHeight / 2f - headerHeight / 2f), new Vector2(windowWidth - 4, headerHeight));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        Text headerText = UIFactory.CreateText(header.transform, "Title", "💣 Minesweeper.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;
        headerText.fontStyle = FontStyle.Bold;
        headerText.GetComponent<RectTransform>().sizeDelta = new Vector2(windowWidth - 60, headerHeight);

        // Close Button in header
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(windowWidth / 2f - 28, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // Accent Line
        GameObject accentLine = UIFactory.CreateUIElement(panelRT, "AccentLine", new Vector2(0, windowHeight / 2f - headerHeight - 2f), new Vector2(windowWidth - 4, 4));
        accentLine.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f);
        accentLine.AddComponent<AccentPulse>();

        // Content area starts below accent line
        float contentTop = windowHeight / 2f - headerHeight - 10f;

        // === Status Bar ===
        float statusBarHeight = 50f;
        float statusBarY = contentTop - statusBarHeight / 2f;

        GameObject statusBar = new GameObject("StatusBar");
        statusBar.transform.SetParent(panelRT, false);
        RectTransform statusBarRT = statusBar.AddComponent<RectTransform>();
        statusBarRT.anchoredPosition = new Vector2(0, statusBarY);
        statusBarRT.sizeDelta = new Vector2(GRID_W * CELL_SIZE + 20, statusBarHeight);
        Image statusBg = statusBar.AddComponent<Image>();
        statusBg.color = new Color(0.65f, 0.65f, 0.65f, 1f);

        // Mine Counter (left)
        GameObject mineCountObj = new GameObject("MineCounter");
        mineCountObj.transform.SetParent(statusBarRT, false);
        RectTransform mcRT = mineCountObj.AddComponent<RectTransform>();
        mcRT.anchoredPosition = new Vector2(-100, 0);
        mcRT.sizeDelta = new Vector2(80, 40);

        Image mcBg = mineCountObj.AddComponent<Image>();
        mcBg.color = Color.black;

        mineCounterText = CreateRetroDigitText(mineCountObj.transform, "MCText", "010");

        // Smiley Button (center)
        GameObject smileyObj = new GameObject("SmileyButton");
        smileyObj.transform.SetParent(statusBarRT, false);
        RectTransform smileyRT = smileyObj.AddComponent<RectTransform>();
        smileyRT.anchoredPosition = Vector2.zero;
        smileyRT.sizeDelta = new Vector2(42, 42);

        Image smileyBg = smileyObj.AddComponent<Image>();
        if (buttonSprite != null)
        {
            smileyBg.sprite = buttonSprite;
            smileyBg.type = Image.Type.Sliced;
            smileyBg.color = Color.white;
        }
        else smileyBg.color = cellUnrevealed;

        smileyButton = smileyObj.AddComponent<Button>();
        smileyButton.targetGraphic = smileyBg;
        smileyButton.onClick.AddListener(ResetGame);

        GameObject smileyTxtObj = new GameObject("SmileyText");
        smileyTxtObj.transform.SetParent(smileyObj.transform, false);
        RectTransform smileyTxtRT = smileyTxtObj.AddComponent<RectTransform>();
        smileyTxtRT.anchorMin = Vector2.zero;
        smileyTxtRT.anchorMax = Vector2.one;
        smileyTxtRT.sizeDelta = Vector2.zero;

        smileyText = smileyTxtObj.AddComponent<Text>();
        smileyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        smileyText.fontSize = 24;
        smileyText.alignment = TextAnchor.MiddleCenter;
        smileyText.text = ":)";
        smileyText.raycastTarget = false;

        // Timer (right)
        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(statusBarRT, false);
        RectTransform timerRT = timerObj.AddComponent<RectTransform>();
        timerRT.anchoredPosition = new Vector2(100, 0);
        timerRT.sizeDelta = new Vector2(80, 40);

        Image timerBg = timerObj.AddComponent<Image>();
        timerBg.color = Color.black;

        timerText = CreateRetroDigitText(timerObj.transform, "TimerText", "000");

        // === Grid Panel ===
        float gridTotalWidth = GRID_W * CELL_SIZE;
        float gridTotalHeight = GRID_H * CELL_SIZE;

        GameObject gridPanel = new GameObject("GridPanel");
        gridPanel.transform.SetParent(panelRT, false);
        RectTransform gridRT = gridPanel.AddComponent<RectTransform>();
        gridRT.anchoredPosition = new Vector2(0, statusBarY - statusBarHeight / 2f - gridTotalHeight / 2f - 8);
        gridRT.sizeDelta = new Vector2(gridTotalWidth, gridTotalHeight);

        Image gridBg = gridPanel.AddComponent<Image>();
        gridBg.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        cellObjects = new GameObject[GRID_W, GRID_H];
        cellTexts = new Text[GRID_W, GRID_H];
        cellImages = new Image[GRID_W, GRID_H];

        for (int x = 0; x < GRID_W; x++)
        {
            for (int y = 0; y < GRID_H; y++)
            {
                float posX = (x - GRID_W / 2f + 0.5f) * CELL_SIZE;
                float posY = (y - GRID_H / 2f + 0.5f) * CELL_SIZE;

                GameObject cellObj = new GameObject("Cell_" + x + "_" + y);
                cellObj.transform.SetParent(gridRT, false);
                RectTransform cellRT = cellObj.AddComponent<RectTransform>();
                cellRT.anchoredPosition = new Vector2(posX, posY);
                cellRT.sizeDelta = new Vector2(CELL_SIZE - 2, CELL_SIZE - 2);

                Image cellImg = cellObj.AddComponent<Image>();
                if (baseSprite != null)
                {
                    cellImg.sprite = baseSprite;
                    cellImg.type = Image.Type.Sliced;
                }
                cellImg.color = cellUnrevealed;

                Button cellBtn = cellObj.AddComponent<Button>();
                cellBtn.targetGraphic = cellImg;

                // Capture closure
                int cx = x, cy = y;
                cellBtn.onClick.AddListener(() => OnCellLeftClick(cx, cy));

                // Right-click handler
                CellRightClickHandler rch = cellObj.AddComponent<CellRightClickHandler>();
                rch.minesweeper = this;
                rch.cellX = cx;
                rch.cellY = cy;

                // Cell text (number/flag/mine)
                GameObject txtObj = new GameObject("CellText");
                txtObj.transform.SetParent(cellObj.transform, false);
                RectTransform txtRT = txtObj.AddComponent<RectTransform>();
                txtRT.anchorMin = Vector2.zero;
                txtRT.anchorMax = Vector2.one;
                txtRT.sizeDelta = Vector2.zero;

                Text txt = txtObj.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 20;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.text = "";
                txt.raycastTarget = false;

                cellObjects[x, y] = cellObj;
                cellTexts[x, y] = txt;
                cellImages[x, y] = cellImg;
            }
        }

        // === Flag Mode Toggle Button ===
        float flagBtnY = statusBarY - statusBarHeight / 2f - gridTotalHeight - 28;
        flagToggleBtn = new GameObject("FlagToggle");
        flagToggleBtn.transform.SetParent(panelRT, false);
        RectTransform flagBtnRT = flagToggleBtn.AddComponent<RectTransform>();
        flagBtnRT.anchoredPosition = new Vector2(0, flagBtnY);
        flagBtnRT.sizeDelta = new Vector2(180, 38);

        Image flagBg = flagToggleBtn.AddComponent<Image>();
        if (buttonSprite != null)
        {
            flagBg.sprite = buttonSprite;
            flagBg.type = Image.Type.Sliced;
            flagBg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
        else flagBg.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        Button flagBtnComp = flagToggleBtn.AddComponent<Button>();
        flagBtnComp.targetGraphic = flagBg;
        flagBtnComp.onClick.AddListener(ToggleFlagMode);

        flagToggleText = UIFactory.CreateText(flagToggleBtn.transform, "Label", "F Flag-Modus", Vector2.zero, 14, TextAnchor.MiddleCenter);
        flagToggleText.fontStyle = FontStyle.Bold;
        flagToggleText.color = Color.black;
        flagToggleText.raycastTarget = false;

        UIFactory.AddHoverEffect(flagToggleBtn);

        // Initialize game state
        ResetGame();
    }

    private Text CreateRetroDigitText(Transform parent, string name, string defaultValue)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Text txt = obj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(1f, 0.2f, 0.2f, 1f); // Classic red LED digits
        txt.text = defaultValue;
        txt.raycastTarget = false;
        return txt;
    }

    public void ResetGame()
    {
        grid = new int[GRID_W, GRID_H];
        revealed = new bool[GRID_W, GRID_H];
        flagged = new bool[GRID_W, GRID_H];

        gameStarted = false;
        gameOver = false;
        gameWon = false;
        flagMode = false;
        flagCount = 0;
        elapsedTime = 0f;
        timerRunning = false;

        if (smileyText != null) smileyText.text = ":)";
        if (mineCounterText != null) mineCounterText.text = FormatDigits(MINE_COUNT);
        if (timerText != null) timerText.text = "000";
        if (flagToggleText != null)
        {
            flagToggleText.text = "F Flag-Modus";
            flagToggleText.color = Color.black;
            flagToggleBtn.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f);
        }

        // Reset all cells
        if (cellTexts != null && cellImages != null && cellObjects != null)
        {
            for (int x = 0; x < GRID_W; x++)
            {
                for (int y = 0; y < GRID_H; y++)
                {
                    if (cellTexts[x, y] != null) cellTexts[x, y].text = "";
                    if (cellImages[x, y] != null) cellImages[x, y].color = cellUnrevealed;
                    if (cellObjects[x, y] != null)
                    {
                        Button btn = cellObjects[x, y].GetComponent<Button>();
                        if (btn != null) btn.interactable = true;
                    }
                }
            }
        }
    }

    private void PlaceMines(int safeX, int safeY)
    {
        // Clear grid
        for (int x = 0; x < GRID_W; x++)
            for (int y = 0; y < GRID_H; y++)
                grid[x, y] = 0;

        int placed = 0;
        while (placed < MINE_COUNT)
        {
            int rx = Random.Range(0, GRID_W);
            int ry = Random.Range(0, GRID_H);

            // Skip if already a mine
            if (grid[rx, ry] == -1) continue;

            // First-click safety: safe zone = clicked cell + neighbors
            if (Mathf.Abs(rx - safeX) <= 1 && Mathf.Abs(ry - safeY) <= 1) continue;

            grid[rx, ry] = -1;
            placed++;
        }

        // Calculate neighbor counts
        for (int x = 0; x < GRID_W; x++)
        {
            for (int y = 0; y < GRID_H; y++)
            {
                if (grid[x, y] == -1) continue;
                int count = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && nx < GRID_W && ny >= 0 && ny < GRID_H && grid[nx, ny] == -1)
                            count++;
                    }
                }
                grid[x, y] = count;
            }
        }
    }

    private void OnCellLeftClick(int x, int y)
    {
        if (gameOver) return;
        if (revealed[x, y]) return;

        if (flagMode)
        {
            ToggleFlag(x, y);
            return;
        }

        if (flagged[x, y]) return; // Can't dig flagged cells

        // First click — place mines
        if (!gameStarted)
        {
            PlaceMines(x, y);
            gameStarted = true;
            timerRunning = true;
        }

        // Hit a mine
        if (grid[x, y] == -1)
        {
            GameLose(x, y);
            return;
        }

        // Reveal cell
        FloodReveal(x, y);
        CheckWin();
    }

    public void OnCellRightClick(int x, int y)
    {
        if (gameOver) return;
        if (revealed[x, y]) return;
        ToggleFlag(x, y);
    }

    private void ToggleFlag(int x, int y)
    {
        if (revealed[x, y]) return;

        flagged[x, y] = !flagged[x, y];
        if (flagged[x, y])
        {
            flagCount++;
            cellTexts[x, y].text = "F";
            cellTexts[x, y].color = Color.red;
            cellTexts[x, y].fontStyle = FontStyle.Bold;
        }
        else
        {
            flagCount--;
            cellTexts[x, y].text = "";
        }

        int remaining = MINE_COUNT - flagCount;
        if (mineCounterText != null)
            mineCounterText.text = FormatDigits(Mathf.Max(remaining, -99));
    }

    private void FloodReveal(int startX, int startY)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            int x = pos.x, y = pos.y;

            if (x < 0 || x >= GRID_W || y < 0 || y >= GRID_H) continue;
            if (revealed[x, y]) continue;
            if (flagged[x, y]) continue;
            if (grid[x, y] == -1) continue;

            revealed[x, y] = true;
            cellImages[x, y].color = cellRevealed;

            int val = grid[x, y];
            if (val > 0)
            {
                cellTexts[x, y].text = val.ToString();
                cellTexts[x, y].color = numberColors[val];
            }
            else
            {
                cellTexts[x, y].text = "";
                // Flood to neighbors
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (dx != 0 || dy != 0)
                            queue.Enqueue(new Vector2Int(x + dx, y + dy));
            }

            // Disable button interactivity
            Button btn = cellObjects[x, y].GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    private void CheckWin()
    {
        int unrevealed = 0;
        for (int x = 0; x < GRID_W; x++)
            for (int y = 0; y < GRID_H; y++)
                if (!revealed[x, y]) unrevealed++;

        if (unrevealed == MINE_COUNT)
        {
            GameWin();
        }
    }

    private void GameWin()
    {
        gameOver = true;
        gameWon = true;
        timerRunning = false;

        if (smileyText != null) smileyText.text = "B)";

        // Auto-flag all mines
        for (int x = 0; x < GRID_W; x++)
        {
            for (int y = 0; y < GRID_H; y++)
            {
                if (grid[x, y] == -1 && !flagged[x, y])
                {
                    cellTexts[x, y].text = "F";
                    cellTexts[x, y].color = Color.green;
                }
            }
        }

        if (mineCounterText != null) mineCounterText.text = "000";

        // Play fanfare
        if (AudioManager.Instance != null && AudioManager.Instance.fanfareSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.fanfareSound, 0.8f);
        }

        // Award coins
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(20);
        }
    }

    private void GameLose(int mineX, int mineY)
    {
        gameOver = true;
        timerRunning = false;

        if (smileyText != null) smileyText.text = "X(";

        // Reveal all mines
        for (int x = 0; x < GRID_W; x++)
        {
            for (int y = 0; y < GRID_H; y++)
            {
                if (grid[x, y] == -1)
                {
                    cellTexts[x, y].text = "*";
                    cellImages[x, y].color = (x == mineX && y == mineY) ? cellMine : cellRevealed;
                }
                // Wrong flags
                else if (flagged[x, y])
                {
                    cellTexts[x, y].text = "X";
                    cellTexts[x, y].color = Color.black;
                }
            }
        }

        // Play explosion
        if (AudioManager.Instance != null && AudioManager.Instance.explosionSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSound, 0.8f);
        }
    }

    private void ToggleFlagMode()
    {
        flagMode = !flagMode;
        if (flagToggleText != null)
        {
            if (flagMode)
            {
                flagToggleBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f); // Red active
                flagToggleText.text = "F Flag-Modus: AN";
                flagToggleText.color = Color.white;
            }
            else
            {
                flagToggleBtn.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f);
                flagToggleText.text = "F Flag-Modus";
                flagToggleText.color = Color.black;
            }
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.unscaledDeltaTime;
            int seconds = Mathf.Min(Mathf.FloorToInt(elapsedTime), 999);
            if (timerText != null) timerText.text = FormatDigits(seconds);
        }

        // Smiley reaction while clicking (only if window is active)
        if (panel != null && panel.activeSelf && !gameOver && gameStarted)
        {
            if (Input.GetMouseButton(0))
            {
                if (smileyText != null) smileyText.text = ":O";
            }
            else
            {
                if (smileyText != null && smileyText.text == "😮") smileyText.text = "😊";
            }
        }
    }

    private string FormatDigits(int val)
    {
        if (val < 0) return "-" + Mathf.Abs(val).ToString("D2");
        return val.ToString("D3");
    }
}

/// <summary>
/// Helper component for detecting right-clicks on individual Minesweeper cells.
/// </summary>
public class CellRightClickHandler : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public MinesweeperManager minesweeper;
    [HideInInspector] public int cellX;
    [HideInInspector] public int cellY;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (minesweeper != null) minesweeper.OnCellRightClick(cellX, cellY);
        }
    }
}
