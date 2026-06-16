using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CleanParkMission : MonoBehaviour
{
    private GameManager gameManager;
    private int totalTrash = 5;
    private int cleanedTrash = 0;
    private List<GameObject> trashObjects = new List<GameObject>();
    private Text progressText;
    private GameObject progressPanel;

    public void StartMission(GameManager gm, Transform[] spawnPoints)
    {
        gameManager = gm;
        cleanedTrash = 0;

        // Spawn trash objects at random positions around the spawn points
        for (int i = 0; i < totalTrash; i++)
        {
            Transform center = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector2 randomOffset = Random.insideUnitCircle * 1f; // Reduced from 4f to 1f to prevent spawning indoors
            Vector3 pos = center.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject trash = new GameObject("TrashPile");
            trash.transform.position = pos;


            // Randomly choose leaf or trash
            bool isLeaf = Random.value > 0.5f;
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>(isLeaf ? "leaf_pile" : "trash_pile");
            
            SpriteRenderer sr = trash.AddComponent<SpriteRenderer>();
            
            if (loadedSprites != null && loadedSprites.Length > 0)
            {
                sr.sprite = loadedSprites[0];
            }
            else
            {
                // Generate fallback sprite for trash if resource not found
                Texture2D tex = new Texture2D(32, 32);
                Color[] colors = new Color[32 * 32];
                for (int c = 0; c < colors.Length; c++) colors[c] = new Color(0.4f, 0.2f, 0f); // Brown leaves/trash
                tex.SetPixels(colors);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            }
            
            // Fix scale based on actual sprite size to ensure it's ~1 world unit large
            if (sr.sprite != null)
            {
                float maxBound = Mathf.Max(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
                if (maxBound > 0)
                {
                    float scale = 1f / maxBound;
                    trash.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
            
            sr.sortingLayerName = "Layer 1";
            sr.sortingOrder = 50;

            BoxCollider2D bc = trash.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = new Vector2(1f, 1f);

            InteractableTrash it = trash.AddComponent<InteractableTrash>();
            it.mission = this;

            trashObjects.Add(trash);
        }

        // Setup UI (Hidden per user request)
        /*
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas != null)
        {
            progressPanel = new GameObject("CleanupProgress");
            progressPanel.transform.SetParent(canvas.transform, false);
            RectTransform rt = progressPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -50);
            rt.sizeDelta = new Vector2(300, 50);

            Image bg = progressPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(progressPanel.transform, false);
            progressText = textObj.AddComponent<Text>();
            progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            progressText.color = Color.white;
            progressText.alignment = TextAnchor.MiddleCenter;
            progressText.fontSize = 24;
            
            UpdateUI();
        }
        */
    }

    public void OnTrashCleaned(GameObject trash)
    {
        cleanedTrash++;
        trashObjects.Remove(trash);
        Destroy(trash);
        UpdateUI();

        if (cleanedTrash >= totalTrash)
        {
            CompleteMission();
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = $"Park aufräumen: {cleanedTrash}/{totalTrash}";
        }
    }

    private void CompleteMission()
    {
        if (progressPanel != null) Destroy(progressPanel);

        gameManager.parkCleanupHelped = true;
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(20);
            ScoreManager.Instance.SaveProgress();
        }
        
        // Optional: show a small toast or just log it, since it's a parallel background mission
        Debug.Log("Park Cleanup Complete! +20 Coins");
        
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (progressPanel != null) Destroy(progressPanel);
        foreach(var t in trashObjects)
        {
            if (t != null) Destroy(t);
        }
    }
}

public class InteractableTrash : MonoBehaviour
{
    public CleanParkMission mission;
    private bool playerInRange = false;
    private bool isCleaning = false;

    private GameObject promptUI;
    private GameObject progressUI;
    private Slider progressSlider;

    private Canvas GetCanvas()
    {
        return Object.FindFirstObjectByType<Canvas>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!isCleaning) ShowPrompt();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HidePrompt();
        }
    }

    void Update()
    {
        if (playerInRange && !isCleaning && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(CleanRoutine());
        }
    }

    private void ShowPrompt()
    {
        Canvas canvas = GetCanvas();
        if (canvas == null) return;
        
        GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        
        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        string textContent = isDe ? "[LEERTASTE] Müll aufräumen" : "[SPACE] Clean up trash";
        
        promptUI = UIFactory.CreateUIElement(canvas.transform as RectTransform, "TrashPrompt", new Vector2(0, -320), new Vector2(340, 50));
        
        Image img = promptUI.GetComponent<Image>();
        if (baseSprite != null)
        {
            img.sprite = baseSprite;
            img.type = Image.Type.Sliced;
        }
        else
        {
            img.color = new Color(0.85f, 0.85f, 0.85f);
        }
        
        Outline outline = promptUI.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        Text txt = UIFactory.CreateText(promptUI.transform, "Text", textContent, Vector2.zero, 18, TextAnchor.MiddleCenter);
        txt.color = Color.black;
        txt.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 40);
        
        StartCoroutine(BobbingPromptRoutine());
    }

    private System.Collections.IEnumerator BobbingPromptRoutine()
    {
        if (promptUI == null) yield break;
        RectTransform rt = promptUI.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;
        while (promptUI != null && playerInRange && !isCleaning)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition = startPos + new Vector2(0, Mathf.Sin(elapsed * 4f) * 5f);
            yield return null;
        }
    }

    private void HidePrompt()
    {
        if (promptUI != null)
        {
            Destroy(promptUI);
            promptUI = null;
        }
    }

    private void ShowProgress()
    {
        Canvas canvas = GetCanvas();
        if (canvas == null) return;
        
        GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        
        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        string titleContent = isDe ? "Park_Aufraeumen.exe" : "Park_Cleanup.exe";
        string bodyContent = isDe ? "Säubere Park..." : "Cleaning park...";
        
        // Main Progress Panel (Retro Window Style)
        progressUI = UIFactory.CreateUIElement(canvas.transform as RectTransform, "TrashProgressWindow", new Vector2(0, 0), new Vector2(360, 120));
        Image bgImg = progressUI.GetComponent<Image>();
        if (baseSprite != null)
        {
            bgImg.sprite = baseSprite;
            bgImg.type = Image.Type.Sliced;
        }
        else
        {
            bgImg.color = new Color(0.85f, 0.85f, 0.85f);
        }
        
        Outline outline = progressUI.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        // Window Title Bar
        GameObject titleBar = UIFactory.CreateUIElement(progressUI.GetComponent<RectTransform>(), "TitleBar", new Vector2(0, 48), new Vector2(352, 24));
        Image titleImg = titleBar.GetComponent<Image>();
        if (headerSprite != null)
        {
            titleImg.sprite = headerSprite;
            titleImg.type = Image.Type.Sliced;
        }
        else
        {
            titleImg.color = new Color(0.0f, 0.0f, 0.5f);
        }
        
        Text titleTxt = UIFactory.CreateText(titleBar.transform, "TitleText", titleContent, new Vector2(8, 0), 12, TextAnchor.MiddleLeft);
        titleTxt.color = Color.white;
        titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 20);
        
        // Status Label
        Text bodyTxt = UIFactory.CreateText(progressUI.transform, "StatusLabel", bodyContent, new Vector2(0, 15), 14, TextAnchor.MiddleCenter);
        bodyTxt.color = Color.black;
        bodyTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 24);
        
        // Progress Slider (Green/Blue progress bar)
        progressSlider = UIFactory.CreateSlider(progressUI.transform, "ProgressBar", new Vector2(0, -25), new Vector2(320, 24), new Color(0.1f, 0.8f, 0.3f));
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.value = 0f;
    }

    private void HideProgress()
    {
        if (progressUI != null)
        {
            Destroy(progressUI);
            progressUI = null;
        }
    }

    private void ShowRewardNotice()
    {
        Canvas canvas = GetCanvas();
        if (canvas == null) return;
        
        GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        
        // Award 5 coins immediately for each trash pile cleaned!
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(5);
            ScoreManager.Instance.SaveProgress();
        }
        
        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        string rewardContent = isDe ? "🎉 Müll entfernt! +5 Münzen" : "🎉 Trash cleared! +5 Coins";
        
        // Play success/coin SFX using hovered or click clip
        if (bootstrap != null && bootstrap.buttonClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(bootstrap.buttonClickSound, 1.25f); // High pitch click for coin noise
        }
        
        GameObject toast = UIFactory.CreateUIElement(canvas.transform as RectTransform, "TrashRewardToast", new Vector2(0, 240), new Vector2(340, 60));
        Image bgImg = toast.GetComponent<Image>();
        if (baseSprite != null)
        {
            bgImg.sprite = baseSprite;
            bgImg.type = Image.Type.Sliced;
        }
        else
        {
            bgImg.color = new Color(0.95f, 0.95f, 0.75f);
        }
        
        Outline outline = toast.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        Text txt = UIFactory.CreateText(toast.transform, "ToastText", rewardContent, Vector2.zero, 16, TextAnchor.MiddleCenter);
        txt.color = Color.black;
        txt.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 50);
        
        StartCoroutine(AnimateToast(toast));
    }

    private System.Collections.IEnumerator AnimateToast(GameObject toast)
    {
        if (toast == null) yield break;
        RectTransform rt = toast.GetComponent<RectTransform>();
        CanvasGroup cg = toast.AddComponent<CanvasGroup>();
        
        float duration = 2.0f;
        float elapsed = 0f;
        
        Vector2 startPos = new Vector2(0, 300);
        Vector2 endPos = new Vector2(0, 220);
        
        while (elapsed < duration && toast != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (t < 0.15f)
            {
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t / 0.15f);
                cg.alpha = t / 0.15f;
            }
            else if (t > 0.75f)
            {
                cg.alpha = (1f - t) / 0.25f;
            }
            else
            {
                rt.anchoredPosition = endPos;
                cg.alpha = 1f;
            }
            
            yield return null;
        }
        
        if (toast != null) Destroy(toast);
    }

    private System.Collections.IEnumerator CleanRoutine()
    {
        isCleaning = true;
        HidePrompt();
        ShowProgress();
        
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null) player.TriggerWin();
        
        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (progressSlider != null)
            {
                progressSlider.value = elapsed / duration;
            }
            yield return null;
        }
        
        if (player != null) player.TriggerWin();
        
        HideProgress();
        ShowRewardNotice();
        
        mission.OnTrashCleaned(gameObject);
    }

    private void OnDisable()
    {
        HidePrompt();
        HideProgress();
    }
}
