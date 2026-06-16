using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance { get; private set; }

    private GameObject panel;
    private GameObject contentContainer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Wait a frame before creating UI so GameBootstrap is ready
        Invoke(nameof(SetupUI), 0.1f);
    }

    private void SetupUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (bootstrap == null) return;

        // Main Window
        panel = new GameObject("HighscoreWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 500);
        panelRT.anchoredPosition = new Vector2(0, 0);

        Image bgImage = panel.AddComponent<Image>();
        if (bootstrap.winBaseSprite != null)
        {
            bgImage.sprite = bootstrap.winBaseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.88f, 0.88f, 0.9f);

        // Shadow
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "Shadow", new Vector2(4, -4), new Vector2(600, 500));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // Header Bar
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 230), new Vector2(592, 40));
        if (bootstrap.winHeaderSprite != null) UIFactory.SetupImage(header, bootstrap.winHeaderSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        Text titleText = UIFactory.CreateText(header.transform, "Title", "🏆 Bestenliste", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        titleText.color = Color.white;
        titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 40);

        // Close Button
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(270, 0), new Vector2(32, 28), bootstrap.winButtonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // Content Area (Scrollable)
        contentContainer = UIFactory.CreateUIElement(panelRT, "ContentArea", new Vector2(0, -20), new Vector2(560, 420));
        contentContainer.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);
        
        ScrollRect sr = contentContainer.AddComponent<ScrollRect>();
        sr.horizontal = false;
        contentContainer.AddComponent<Mask>().showMaskGraphic = true;

        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(contentContainer.transform, false);
        RectTransform innerRT = inner.AddComponent<RectTransform>();
        innerRT.anchorMin = new Vector2(0, 1);
        innerRT.anchorMax = new Vector2(1, 1);
        innerRT.pivot = new Vector2(0.5f, 1f);
        innerRT.sizeDelta = new Vector2(0, 0); 
        innerRT.anchoredPosition = Vector2.zero;
        sr.content = innerRT;

        VerticalLayoutGroup vlg = inner.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = inner.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void ToggleWindow()
    {
        if (panel == null) return;
        
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf)
        {
            panel.transform.SetAsLastSibling();
            RefreshList();
        }
    }

    private void RefreshList()
    {
        if (contentContainer == null) return;
        ScrollRect sr = contentContainer.GetComponent<ScrollRect>();
        if (sr == null || sr.content == null) return;
        
        Transform inner = sr.content;

        foreach (Transform child in inner)
        {
            Destroy(child.gameObject);
        }

        var loc = LocalizationManager.Instance;

        // Header Row
        CreateRow(inner, "Notfall / Mission", "Bestzeit", true);

        // Entries
        foreach (GameManager.VictimType type in Enum.GetValues(typeof(GameManager.VictimType)))
        {
            if (HasRecord(type))
            {
                float time = GetBestTime(type);
                string locKey = type.ToString();
                
                // Try to find mX_title
                string mKey = GetMissionIdForType(type);
                string title = loc != null && !string.IsNullOrEmpty(mKey) ? loc.Get(mKey + "_title") : type.ToString();
                
                string timeStr = FormatTime(time);
                CreateRow(inner, "🚑 " + title, timeStr, false);
            }
        }
    }

    private void CreateRow(Transform parent, string leftText, string rightText, bool isHeader)
    {
        GameObject row = new GameObject("Row");
        row.transform.SetParent(parent, false);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = isHeader ? new Color(0.2f, 0.2f, 0.3f) : new Color(0.95f, 0.95f, 0.95f);

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 40;

        Text leftTxt = UIFactory.CreateText(row.transform, "Left", leftText, new Vector2(10, 0), 16, TextAnchor.MiddleLeft);
        leftTxt.color = isHeader ? Color.white : Color.black;
        leftTxt.fontStyle = isHeader ? FontStyle.Bold : FontStyle.Normal;
        RectTransform leftRT = leftTxt.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0, 0); leftRT.anchorMax = new Vector2(1, 1); leftRT.sizeDelta = new Vector2(-150, 0); leftRT.anchoredPosition = new Vector2(10, 0);

        Text rightTxt = UIFactory.CreateText(row.transform, "Right", rightText, new Vector2(-10, 0), 16, TextAnchor.MiddleRight);
        rightTxt.color = isHeader ? Color.white : new Color(0.1f, 0.6f, 0.2f);
        rightTxt.fontStyle = isHeader ? FontStyle.Bold : FontStyle.Bold;
        RectTransform rightRT = rightTxt.GetComponent<RectTransform>();
        rightRT.anchorMin = new Vector2(1, 0); rightRT.anchorMax = new Vector2(1, 1); rightRT.sizeDelta = new Vector2(150, 0); rightRT.anchoredPosition = new Vector2(-10, 0);
        rightRT.pivot = new Vector2(1, 0.5f);
    }

    private string FormatTime(float timeInSeconds)
    {
        int m = Mathf.FloorToInt(timeInSeconds / 60f);
        int s = Mathf.FloorToInt(timeInSeconds % 60f);
        int ms = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", m, s, ms);
    }

    private string GetMissionIdForType(GameManager.VictimType type)
    {
        switch(type)
        {
            case GameManager.VictimType.BikeAccident: return "m1";
            case GameManager.VictimType.BleedingWound: return "m2";
            case GameManager.VictimType.UnconsciousPerson: return "m3";
            case GameManager.VictimType.BurnInjury: return "m4";
            case GameManager.VictimType.Choking: return "m5";
            case GameManager.VictimType.Heatstroke: return "m6";
            case GameManager.VictimType.ElectricShock: return "m7";
            case GameManager.VictimType.Poisoning: return "m8";
            case GameManager.VictimType.TriageScene: return "m9";
            case GameManager.VictimType.BoneFracture: return "m10";
            case GameManager.VictimType.AllergicShock: return "m11";
            case GameManager.VictimType.DrowningVictim: return "m12";
            case GameManager.VictimType.DiabeticShock: return "m13";
            case GameManager.VictimType.PanicAttack: return "m14";
            case GameManager.VictimType.Stroke: return "m15";
            case GameManager.VictimType.DogPoisoning: return "m17";
            case GameManager.VictimType.HeartAttack: return "m18";
            case GameManager.VictimType.Snakebite: return "m19";
            default: return "";
        }
    }

    public void SaveTime(GameManager.VictimType type, float timeInSeconds)
    {
        string key = "BestTime_" + type.ToString();
        float currentBest = PlayerPrefs.GetFloat(key, float.MaxValue);
        
        if (timeInSeconds < currentBest)
        {
            PlayerPrefs.SetFloat(key, timeInSeconds);
            PlayerPrefs.Save();
        }
    }

    public float GetBestTime(GameManager.VictimType type)
    {
        return PlayerPrefs.GetFloat("BestTime_" + type.ToString(), 0f);
    }
    
    public bool HasRecord(GameManager.VictimType type)
    {
        return PlayerPrefs.HasKey("BestTime_" + type.ToString());
    }

    public void ClearAllRecords()
    {
        foreach (GameManager.VictimType type in Enum.GetValues(typeof(GameManager.VictimType)))
        {
            PlayerPrefs.DeleteKey("BestTime_" + type.ToString());
        }
        PlayerPrefs.Save();
    }
}
