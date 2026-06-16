using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GafferManager : MonoBehaviour
{
    public static GafferManager Instance { get; private set; }

    private List<GameObject> activeGaffers = new List<GameObject>();
    private GameObject blockerPanel;
    private Text warningText;

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
        }
    }

    public void SpawnUIGaffers(Transform parentPanel, int count)
    {
        ClearGaffers();

        // Create a semi-transparent blocker panel that prevents interacting with the minigame until gaffers are cleared
        blockerPanel = new GameObject("GafferBlocker");
        blockerPanel.transform.SetParent(parentPanel, false);
        RectTransform blockerRT = blockerPanel.AddComponent<RectTransform>();
        blockerRT.anchorMin = Vector2.zero;
        blockerRT.anchorMax = Vector2.one;
        blockerRT.sizeDelta = Vector2.zero;
        blockerRT.anchoredPosition = Vector2.zero;

        Image blockerImg = blockerPanel.AddComponent<Image>();
        blockerImg.color = new Color(0f, 0f, 0f, 0.4f);
        blockerImg.raycastTarget = true; // Blocks clicks to UI underneath

        warningText = UIFactory.CreateText(blockerPanel.transform, "Warning", "⚠ PLATZ MACHEN!\nKlicke auf die Schaulustigen, um sie wegzuschicken!", new Vector2(0, 150), 24, TextAnchor.MiddleCenter);
        warningText.color = Color.red;
        warningText.fontStyle = FontStyle.Bold;
        warningText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite btnSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        string[] gafferLabels = { "GAFFER!", "FOTO!", "HANDY!", "WEG DA!", "STÖRER!" };

        for (int i = 0; i < count; i++)
        {
            float randomX = Random.Range(-300f, 300f);
            float randomY = Random.Range(-200f, 50f);

            GameObject gafferBtn = UIFactory.CreateButton((RectTransform)blockerPanel.transform, "Gaffer_" + i, gafferLabels[Random.Range(0, gafferLabels.Length)], new Vector2(randomX, randomY), new Vector2(120, 60), btnSprite);
            
            Text btnTxt = gafferBtn.GetComponentInChildren<Text>();
            if (btnTxt != null) btnTxt.fontSize = 16;

            Button btn = gafferBtn.GetComponent<Button>();
            GameObject capturedGaffer = gafferBtn; // capture for closure
            btn.onClick.AddListener(() => {
                RemoveGaffer(capturedGaffer);
            });

            // Add simple floating animation
            capturedGaffer.AddComponent<GafferWobble>();

            activeGaffers.Add(capturedGaffer);
        }
        
        // Ensure blocker is the very last sibling so it renders on top of everything
        blockerPanel.transform.SetAsLastSibling();
    }

    private void RemoveGaffer(GameObject gafferObj)
    {
        if (activeGaffers.Contains(gafferObj))
        {
            activeGaffers.Remove(gafferObj);
            Destroy(gafferObj);

            if (activeGaffers.Count == 0)
            {
                ClearGaffers(); // Removes blocker
            }
        }
    }

    public void ClearGaffers()
    {
        foreach (var g in activeGaffers)
        {
            if (g != null) Destroy(g);
        }
        activeGaffers.Clear();

        if (blockerPanel != null)
        {
            Destroy(blockerPanel);
        }
    }
}

public class GafferWobble : MonoBehaviour
{
    private RectTransform rt;
    private float speed;
    private float amp;
    private Vector2 startPos;
    private float offset;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
        speed = Random.Range(2f, 4f);
        amp = Random.Range(10f, 20f);
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (rt != null)
        {
            rt.anchoredPosition = startPos + new Vector2(Mathf.Sin(Time.time * speed + offset) * amp, Mathf.Cos(Time.time * speed * 0.8f + offset) * (amp * 0.5f));
        }
    }
}
