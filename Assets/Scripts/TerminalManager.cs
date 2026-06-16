using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class TerminalManager : MonoBehaviour
{
    public static TerminalManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject terminalPanel;
    public Text outputText;
    public InputField inputField;

    private StringBuilder logBuffer = new StringBuilder();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupTerminalUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupTerminalUI()
    {
        // 1. Find or create GameCanvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 2. Create the main Window Dialog panel using UIFactory methods if possible
        // We will proceduralize it so it integrates flawlessly with the Windows UI styling
        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite innerFrameSprite = bootstrap != null ? bootstrap.winInnerFrameSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        terminalPanel = new GameObject("TerminalWindow");
        terminalPanel.transform.SetParent(canvas.transform, false);
        terminalPanel.SetActive(false);

        RectTransform panelRT = terminalPanel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(800, 600);
        panelRT.anchoredPosition = new Vector2(0, 0);

        Image bgImage = terminalPanel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else
        {
            bgImage.color = new Color(0.8f, 0.8f, 0.8f);
        }

        // Window shadow (parented to panel so it moves with it)
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "TerminalWindow_Shadow", new Vector2(4, -4), new Vector2(800, 600));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        // Add standard pop-in animation
        terminalPanel.AddComponent<DialogPopIn>();

        // Header Title Bar
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 275), new Vector2(790, 40));
        header.AddComponent<WindowDragger>();
        RectTransform headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0.5f, 0.5f);
        headerRT.anchorMax = new Vector2(0.5f, 0.5f);
        headerRT.pivot = new Vector2(0.5f, 0.5f);
        
        Image headerImg = header.GetComponent<Image>();
        if (headerSprite != null)
        {
            headerImg.sprite = headerSprite;
            headerImg.type = Image.Type.Sliced;
        }
        else
        {
            headerImg.color = new Color(0.1f, 0.1f, 0.5f);
        }

        Text headerText = UIFactory.CreateText(header.transform, "Title", "C:\\Windows\\System32\\cmd.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;

        // Close Button in header
        GameObject closeBtn = UIFactory.CreateButton(headerRT, "CloseButton", "X", new Vector2(370, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => terminalPanel.SetActive(false));
        Text closeBtnText = closeBtn.GetComponentInChildren<Text>();
        if (closeBtnText != null) closeBtnText.color = Color.black;

        // Terminal Inner Frame (Black console viewport)
        GameObject consoleBg = UIFactory.CreateUIElement(panelRT, "ConsoleBackground", new Vector2(0, -25), new Vector2(780, 520));
        consoleBg.GetComponent<Image>().color = Color.black;

        // Make the entire console black screen focus the input field when clicked
        Button consoleBtn = consoleBg.AddComponent<Button>();
        consoleBtn.transition = Selectable.Transition.None;
        consoleBtn.onClick.AddListener(() => {
            if (inputField != null)
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
        });

        // Text Log Viewport
        outputText = UIFactory.CreateText(consoleBg.transform, "OutputText", "", new Vector2(10, 20), 20, TextAnchor.UpperLeft);
        outputText.color = new Color(0.2f, 1f, 0.2f); // Retro hacker green!
        outputText.fontStyle = FontStyle.Normal;
        RectTransform textRT = outputText.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = new Vector2(-20, -60);
        textRT.anchoredPosition = new Vector2(10, -20);
        textRT.pivot = new Vector2(0, 1);

        // Command Input Field
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(consoleBg.transform, false);
        RectTransform inputRT = inputObj.AddComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0, 0);
        inputRT.anchorMax = new Vector2(1, 0);
        inputRT.sizeDelta = new Vector2(-20, 40);
        inputRT.anchoredPosition = new Vector2(10, 25);
        inputRT.pivot = new Vector2(0.5f, 0.5f);

        Image inputImg = inputObj.AddComponent<Image>();
        inputImg.color = new Color(0.05f, 0.05f, 0.05f); // Slightly lighter black

        // Input text display component
        GameObject textShow = new GameObject("Text");
        textShow.transform.SetParent(inputObj.transform, false);
        RectTransform showRT = textShow.AddComponent<RectTransform>();
        showRT.anchorMin = Vector2.zero;
        showRT.anchorMax = Vector2.one;
        showRT.sizeDelta = new Vector2(-10, -10);

        Text txtComponent = textShow.AddComponent<Text>();
        txtComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtComponent.fontSize = 20;
        txtComponent.color = Color.green;
        txtComponent.alignment = TextAnchor.MiddleLeft;

        inputField = inputObj.AddComponent<InputField>();
        inputField.targetGraphic = inputImg;
        inputField.textComponent = txtComponent;
        inputField.transition = Selectable.Transition.None;

        // Hook Enter key
        inputField.onSubmit.AddListener(OnSubmitCommand);

        // Initial message
        WriteLine("Microsoft Windows 98 [Version 4.10.1998]");
        WriteLine("(C) Copyright Microsoft Corp 1981-1998.");
        WriteLine("");
        WriteLine("Type 'help' to see list of diagnostic commands.");
        WriteLine("");
        WriteLine("C:\\>");
    }

    public void ToggleTerminal()
    {
        if (terminalPanel == null) return;
        bool isAct = !terminalPanel.activeSelf;
        terminalPanel.SetActive(isAct);
        if (isAct)
        {
            terminalPanel.transform.SetAsLastSibling();
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    public void WriteLine(string text)
    {
        logBuffer.AppendLine(text);
        
        // Cap lines at 25 to prevent overflow of UI view
        string[] lines = logBuffer.ToString().Split('\n');
        if (lines.Length > 20)
        {
            logBuffer.Clear();
            for (int i = lines.Length - 19; i < lines.Length; i++)
            {
                logBuffer.AppendLine(lines[i]);
            }
        }

        if (outputText != null)
        {
            outputText.text = logBuffer.ToString();
        }
    }

    private void OnSubmitCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return;

        WriteLine("C:\\> " + cmd);
        inputField.text = "";
        inputField.ActivateInputField();

        ParseCommand(cmd.Trim());
    }

    private void ParseCommand(string commandLine)
    {
        string[] parts = commandLine.Split(' ');
        string baseCmd = parts[0].ToLower();

        switch (baseCmd)
        {
            case "help":
                WriteLine("Supported commands:");
                WriteLine("  help               - Shows this menu.");
                WriteLine("  give_coins <num>   - Cheats/adds coins directly.");
                WriteLine("  set_time <0|0.5|1> - Set time of day (0=Day, 0.5=Night).");
                WriteLine("  cpr_slow           - Slows down chest compression rhythm.");
                WriteLine("  skip_intro         - Jumps straight past intro video.");
                WriteLine("  clear              - Clears the console log.");
                WriteLine("  exit               - Closes the cmd terminal.");
                break;

            case "clear":
                logBuffer.Clear();
                if (outputText != null) outputText.text = "";
                break;

            case "exit":
                terminalPanel.SetActive(false);
                break;

            case "give_coins":
                if (parts.Length > 1 && int.TryParse(parts[1], out int amount))
                {
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddCoins(amount);
                        ScoreManager.Instance.SaveProgress();
                        WriteLine($"Added {amount} coins. Current: {ScoreManager.Instance.lifeCoins}");
                    }
                    else
                    {
                        WriteLine("Error: ScoreManager instance not found!");
                    }
                }
                else
                {
                    WriteLine("Usage: give_coins <amount>");
                }
                break;

            case "set_time":
                if (parts.Length > 1 && float.TryParse(parts[1], out float timeVal))
                {
                    DayNightCycle dnc = FindFirstObjectByType<DayNightCycle>();
                    if (dnc != null)
                    {
                        dnc.timeOfDay = Mathf.Clamp01(timeVal);
                        WriteLine($"TimeOfDay successfully updated to: {timeVal}");
                    }
                    else
                    {
                        WriteLine("Error: DayNightCycle script not found in scene!");
                    }
                }
                else
                {
                    WriteLine("Usage: set_time <0 = day, 0.5 = night, 1 = day>");
                }
                break;

            case "cpr_slow":
                if (CPRManager.Instance != null)
                {
                    CPRManager.Instance.SlowDownRhythm();
                    WriteLine("Cheat activated: CPR compression rhythm successfully slowed down!");
                }
                else
                {
                    WriteLine("Error: CPRManager instance not found in scene!");
                }
                break;

            case "skip_intro":
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartIntroPhase();
                    WriteLine("Intro successfully skipped!");
                    terminalPanel.SetActive(false);
                }
                else
                {
                    WriteLine("Error: GameManager instance not active!");
                }
                break;

            default:
                WriteLine($"'{baseCmd}' is not recognized as an internal or external command.");
                break;
        }
    }
}
