using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classic Snake minigame with retro Windows 98 styling.
/// 20x15 grid, arrow/WASD controls, coins awarded per 5 points.
/// </summary>
public class SnakeManager : MonoBehaviour
{
    public static SnakeManager Instance;

    private const int GRID_W = 20;
    private const int GRID_H = 15;
    private const float CELL_SIZE = 24f;
    private const float BASE_TICK = 0.18f;

    // Game state
    private List<Vector2Int> snake = new List<Vector2Int>();
    private Vector2Int direction = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;
    private Vector2Int food;
    private int score = 0;
    private int highScore = 0;
    private bool gameRunning = false;
    private bool gameOver = false;
    private float tickTimer = 0f;
    private float currentTick = BASE_TICK;

    // UI
    public GameObject panel;
    private RectTransform panelRT;
    private Image[,] cellImages;
    private Text scoreText;
    private Text highScoreText;
    private Text statusText;
    private Button restartButton;

    // Colors
    private Color bgColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Color snakeHead = new Color(0.2f, 0.8f, 0.2f, 1f);
    private Color snakeBody = new Color(0.15f, 0.6f, 0.15f, 1f);
    private Color foodColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    private Color gridLine = new Color(0.15f, 0.15f, 0.15f, 1f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            highScore = PlayerPrefs.GetInt("SnakeHighScore", 0);
            SetupSnakeUI();
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
            else 
            { 
                panel.SetActive(true); 
                panel.transform.SetAsLastSibling(); 
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void SetupSnakeUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        float windowWidth = GRID_W * CELL_SIZE + 40;
        float windowHeight = GRID_H * CELL_SIZE + 160;

        // Main Window
        panel = new GameObject("SnakeWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(windowWidth, windowHeight);
        panelRT.anchoredPosition = new Vector2(-100, 50);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.85f, 0.85f, 0.85f);

        // Shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "SnakeWindow_Shadow", new Vector2(4, -4), new Vector2(windowWidth, windowHeight));
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

        Text headerText = UIFactory.CreateText(header.transform, "Title", "🐍 Snake.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;
        headerText.fontStyle = FontStyle.Bold;
        headerText.GetComponent<RectTransform>().sizeDelta = new Vector2(windowWidth - 60, headerHeight);

        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(windowWidth / 2f - 28, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // Accent Line
        GameObject accentLine = UIFactory.CreateUIElement(panelRT, "AccentLine", new Vector2(0, windowHeight / 2f - headerHeight - 2f), new Vector2(windowWidth - 4, 4));
        accentLine.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);
        accentLine.AddComponent<AccentPulse>();

        float contentTop = windowHeight / 2f - headerHeight - 10f;

        // Status bar
        float statusY = contentTop - 20f;
        scoreText = UIFactory.CreateText(panelRT, "ScoreText", "Score: 0", new Vector2(-windowWidth / 4f, statusY), 18, TextAnchor.MiddleLeft);
        scoreText.color = Color.black;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);

        highScoreText = UIFactory.CreateText(panelRT, "HighScoreText", "Best: " + highScore, new Vector2(windowWidth / 4f, statusY), 18, TextAnchor.MiddleRight);
        highScoreText.color = new Color(0.5f, 0.5f, 0.5f);
        highScoreText.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);

        // Grid Panel
        float gridW = GRID_W * CELL_SIZE;
        float gridH = GRID_H * CELL_SIZE;
        float gridY = statusY - 25f - gridH / 2f;

        GameObject gridPanel = new GameObject("GridPanel");
        gridPanel.transform.SetParent(panelRT, false);
        RectTransform gridRT = gridPanel.AddComponent<RectTransform>();
        gridRT.anchoredPosition = new Vector2(0, gridY);
        gridRT.sizeDelta = new Vector2(gridW, gridH);

        Image gridBg = gridPanel.AddComponent<Image>();
        gridBg.color = bgColor;

        cellImages = new Image[GRID_W, GRID_H];
        for (int x = 0; x < GRID_W; x++)
        {
            for (int y = 0; y < GRID_H; y++)
            {
                float posX = (x - GRID_W / 2f + 0.5f) * CELL_SIZE;
                float posY = (y - GRID_H / 2f + 0.5f) * CELL_SIZE;

                GameObject cellObj = new GameObject("SC_" + x + "_" + y);
                cellObj.transform.SetParent(gridRT, false);
                RectTransform cellRT = cellObj.AddComponent<RectTransform>();
                cellRT.anchoredPosition = new Vector2(posX, posY);
                cellRT.sizeDelta = new Vector2(CELL_SIZE - 1, CELL_SIZE - 1);

                Image cellImg = cellObj.AddComponent<Image>();
                cellImg.color = gridLine;
                cellImg.raycastTarget = false;

                cellImages[x, y] = cellImg;
            }
        }

        // Status / Restart area
        float bottomY = gridY - gridH / 2f - 30f;

        statusText = UIFactory.CreateText(panelRT, "StatusText", "Pfeiltasten / WASD zum Starten & Steuern", new Vector2(0, bottomY), 16, TextAnchor.MiddleCenter);
        statusText.color = Color.black;
        statusText.GetComponent<RectTransform>().sizeDelta = new Vector2(windowWidth - 20, 50);

        ResetGame();
    }

    private void ResetGame()
    {
        snake.Clear();
        Vector2Int center = new Vector2Int(GRID_W / 2, GRID_H / 2);
        snake.Add(center);
        snake.Add(center + Vector2Int.left);
        snake.Add(center + Vector2Int.left * 2);

        direction = Vector2Int.right;
        nextDirection = Vector2Int.right;
        score = 0;
        gameRunning = false;
        gameOver = false;
        tickTimer = 0f;
        currentTick = BASE_TICK;

        SpawnFood();
        UpdateScoreUI();
        RenderGrid();

        if (statusText != null) statusText.text = "Pfeiltasten / WASD zum Starten & Steuern";
    }

    private void SpawnFood()
    {
        List<Vector2Int> free = new List<Vector2Int>();
        for (int x = 0; x < GRID_W; x++)
            for (int y = 0; y < GRID_H; y++)
                if (!snake.Contains(new Vector2Int(x, y)))
                    free.Add(new Vector2Int(x, y));

        if (free.Count > 0)
            food = free[Random.Range(0, free.Count)];
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf) return;

        // Input
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (direction != Vector2Int.down) nextDirection = Vector2Int.up;
            if (!gameRunning && !gameOver) { gameRunning = true; if (statusText != null) statusText.text = ""; }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (direction != Vector2Int.up) nextDirection = Vector2Int.down;
            if (!gameRunning && !gameOver) { gameRunning = true; if (statusText != null) statusText.text = ""; }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (direction != Vector2Int.right) nextDirection = Vector2Int.left;
            if (!gameRunning && !gameOver) { gameRunning = true; if (statusText != null) statusText.text = ""; }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (direction != Vector2Int.left) nextDirection = Vector2Int.right;
            if (!gameRunning && !gameOver) { gameRunning = true; if (statusText != null) statusText.text = ""; }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gameOver) ResetGame();
            else if (!gameRunning) { gameRunning = true; if (statusText != null) statusText.text = ""; }
        }

        if (!gameRunning || gameOver) return;

        tickTimer += Time.unscaledDeltaTime;
        if (tickTimer >= currentTick)
        {
            tickTimer = 0f;
            Tick();
        }
    }

    private void Tick()
    {
        direction = nextDirection;
        Vector2Int head = snake[0] + direction;

        // Wall collision
        if (head.x < 0 || head.x >= GRID_W || head.y < 0 || head.y >= GRID_H)
        {
            OnGameOver();
            return;
        }

        // Self collision (check before adding new head)
        for (int i = 0; i < snake.Count - 1; i++)
        {
            if (snake[i] == head)
            {
                OnGameOver();
                return;
            }
        }

        snake.Insert(0, head);

        // Eat food
        if (head == food)
        {
            score++;
            UpdateScoreUI();
            SpawnFood();

            // Speed up every 5 points
            if (score % 5 == 0)
            {
                currentTick = Mathf.Max(0.06f, currentTick - 0.015f);
            }

            if (AudioManager.Instance != null && AudioManager.Instance.eatSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.eatSound, 0.6f);
            }
        }
        else
        {
            snake.RemoveAt(snake.Count - 1);
        }

        RenderGrid();
    }

    private void OnGameOver()
    {
        gameOver = true;
        gameRunning = false;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("SnakeHighScore", highScore);
        }

        if (statusText != null) statusText.text = "GAME OVER! Score: " + score + "\nLeertaste zum Neustarten";

        // Award coins: 1 per 5 points
        int coinsEarned = score / 5;
        if (coinsEarned > 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(coinsEarned);
        }

        if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound, 0.5f);
        }

        UpdateScoreUI();
        RenderGrid();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (highScoreText != null) highScoreText.text = "Best: " + highScore;
    }

    private void RenderGrid()
    {
        if (cellImages == null) return;

        // Clear
        for (int x = 0; x < GRID_W; x++)
            for (int y = 0; y < GRID_H; y++)
                if (cellImages[x, y] != null)
                    cellImages[x, y].color = gridLine;

        // Food
        if (food.x >= 0 && food.x < GRID_W && food.y >= 0 && food.y < GRID_H)
            cellImages[food.x, food.y].color = foodColor;

        // Snake
        for (int i = 0; i < snake.Count; i++)
        {
            Vector2Int s = snake[i];
            if (s.x >= 0 && s.x < GRID_W && s.y >= 0 && s.y < GRID_H)
            {
                cellImages[s.x, s.y].color = (i == 0) ? snakeHead : snakeBody;
            }
        }
    }
}
