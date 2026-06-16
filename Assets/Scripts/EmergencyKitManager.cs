using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EmergencyKitManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject emergencyKitPanel;
    
    // UI Connections
    public RectTransform itemContainer; // Where items spawn initially
    public DropZone kitDropZone;        // Target Koffer
    public DropZone binDropZone;        // Target Mülleimer
    public Text feedbackTitleText;      // Large warning/success text
    public Text feedbackBodyText;       // Explanation of the item
    public Button finishButton;         // Clickable when all items sorted
    public Text progressText;           // e.g. "Einsortiert: 0/10"

    [Header("Game Feel")]
    public float popupSpeed = 10f;
    public AudioClip kitDropSound;
    public AudioClip binDropSound;

    public enum ItemCategory { MustHave, ThinkAboutIt, NoGo }

    [System.Serializable]
    public class KitItem
    {
        public string id;
        public string displayName;
        public string spriteResourcePath;
        public ItemCategory category;
        public string explanationText;
        
        [HideInInspector] public bool isSorted = false;
        [HideInInspector] public bool isPackedInKit = false; // true if dropped in kit, false if in bin
        [HideInInspector] public GameObject uiInstance;
    }

    public List<KitItem> items = new List<KitItem>();
    private int sortedCount = 0;
    private bool isEvaluated = false;

    public void InitializeItems()
    {
        items.Clear();

        // 1. MUST-HAVES (100% in den Koffer)
        items.Add(new KitItem {
            id = "smartphone",
            displayName = "Smartphone",
            spriteResourcePath = "EmergencyKit/item_smartphone",
            category = ItemCategory.MustHave,
            explanationText = "100% REIN: Zum Absetzen des Notrufs (112), zur Positionsbestimmung und als Taschenlampe absolut unverzichtbar im Alltag!"
        });

        items.Add(new KitItem {
            id = "plasters",
            displayName = "Pflaster & Verband",
            spriteResourcePath = "EmergencyKit/item_plasters",
            category = ItemCategory.MustHave,
            explanationText = "100% REIN: Kleinere Schnittwunden oder stärkere Blutungen müssen sofort versorgt werden. Gehört in jede Alltagstasche!"
        });

        items.Add(new KitItem {
            id = "emergency_card",
            displayName = "Notfallkarte",
            spriteResourcePath = "EmergencyKit/item_emergency_card",
            category = ItemCategory.MustHave,
            explanationText = "100% REIN: Informiert Ersthelfer und Rettungsdienst sofort über Allergien, Vorerkrankungen, Kontaktpersonen und deinen Organspende-Status."
        });

        items.Add(new KitItem {
            id = "gloves",
            displayName = "Einmalhandschuhe",
            spriteResourcePath = "EmergencyKit/item_gloves",
            category = ItemCategory.MustHave,
            explanationText = "100% REIN: Schützt DICH vor Infektionen bei Kontakt mit Blut/Körperflüssigkeiten und schützt das Opfer vor Keimen."
        });

        // 2. THINK ABOUT IT (Nuancierte Alltags-Sachen)
        items.Add(new KitItem {
            id = "pocket_knife",
            displayName = "Taschenmesser",
            spriteResourcePath = "EmergencyKit/item_pocket_knife",
            category = ItemCategory.ThinkAboutIt,
            explanationText = "NACHDENKEN: Extrem nützlich (z.B. Kleidung/Gurt aufschneiden), aber Achtung bei rechtlichen Verboten (Schulen, Flughäfen, Events)!"
        });

        items.Add(new KitItem {
            id = "water_bottle",
            displayName = "Trinkwasser",
            spriteResourcePath = "EmergencyKit/item_water_bottle",
            category = ItemCategory.ThinkAboutIt,
            explanationText = "NACHDENKEN: Überlebenswichtig! Zwar schwer und sperrig im Alltag, aber eine kleine, frische Flasche Wasser schadet in keiner Tasche."
        });

        items.Add(new KitItem {
            id = "pills",
            displayName = "Schmerzmittel",
            spriteResourcePath = "EmergencyKit/item_pills",
            category = ItemCategory.ThinkAboutIt,
            explanationText = "NACHDENKEN: Hilfreich für dich selbst! Aber als Ersthelfer darfst du NIEMALS Medikamente an fremde Personen verabreichen!"
        });

        // 3. NO-GOs (Offensichtlicher Ballast)
        items.Add(new KitItem {
            id = "game_console",
            displayName = "Spielkonsole",
            spriteResourcePath = "EmergencyKit/item_game_console",
            category = ItemCategory.NoGo,
            explanationText = "MÜLLEIMER: Bietet keinerlei Nutzen im Notfall, nimmt wertvollen Platz weg und erhöht nur unnötig das Gewicht deiner Tasche."
        });

        items.Add(new KitItem {
            id = "coffee_mug",
            displayName = "Kaffeetasse",
            spriteResourcePath = "EmergencyKit/item_coffee_mug",
            category = ItemCategory.NoGo,
            explanationText = "MÜLLEIMER: Zerbrechlich, schwer und absolut nutzlos für die Erste Hilfe oder Notfallsituationen im Freien."
        });

        items.Add(new KitItem {
            id = "plush_bear",
            displayName = "Teddybär",
            spriteResourcePath = "EmergencyKit/item_plush_bear",
            category = ItemCategory.NoGo,
            explanationText = "MÜLLEIMER: Zwar tröstend für Kinder, aber als Notfall-Ausrüstung im Alltag nimmt er schlichtweg zu viel Platz weg."
        });
    }

    public void Activate()
    {
        emergencyKitPanel.SetActive(true);
        emergencyKitPanel.transform.localScale = Vector3.zero;
        StartCoroutine(PopupAnimation());
        
        InitializeItems();
        sortedCount = 0;
        isEvaluated = false;
        finishButton.interactable = false;
        finishButton.GetComponentInChildren<Text>().text = "BEWERTUNG ABSCHLIESSEN";
        
        feedbackTitleText.text = "NOTFALLKOFFER PLANER";
        feedbackBodyText.text = "Ziehe die Gegenstände per Drag & Drop:\n- In den KOFFER, wenn sie 100% rein gehören oder nützlich sind.\n- In den MÜLLEIMER, wenn sie unnötiger Ballast sind.";
        
        UpdateProgressUI();
        SpawnItems();

        // Register drop triggers
        kitDropZone.onDropSuccess = () => OnItemDropped(true);
        binDropZone.onDropSuccess = () => OnItemDropped(false);
    }

    private IEnumerator PopupAnimation()
    {
        while (emergencyKitPanel.transform.localScale.x < 0.99f)
        {
            emergencyKitPanel.transform.localScale = Vector3.Lerp(emergencyKitPanel.transform.localScale, Vector3.one, Time.deltaTime * popupSpeed);
            yield return null;
        }
        emergencyKitPanel.transform.localScale = Vector3.one;
    }

    private void SpawnItems()
    {
        // Clear old ones
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn items in a grid/shelf layout
        for (int i = 0; i < items.Count; i++)
        {
            KitItem item = items[i];
            
            GameObject itemObj = new GameObject("DragItem_" + item.id);
            itemObj.transform.SetParent(itemContainer, false);
            
            // Layout position on a 2x5 grid
            float col = i % 5;
            float row = i / 5;
            Vector2 pos = new Vector2(-280f + (col * 140f), 60f - (row * 130f));
            
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);
            rect.anchoredPosition = pos;

            Image img = itemObj.AddComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>(item.spriteResourcePath);
            if (sprite != null)
            {
                img.sprite = sprite;
            }
            img.preserveAspect = true;

            // Make it draggable
            DragItem drag = itemObj.AddComponent<DragItem>();
            drag.isDropped = false;

            // Save reference
            item.uiInstance = itemObj;
        }
    }

    private void OnItemDropped(bool droppedInKit)
    {
        // Find which item was dropped by checking the active drag item
        foreach (var item in items)
        {
            if (item.uiInstance != null && !item.isSorted)
            {
                DragItem dItem = item.uiInstance.GetComponent<DragItem>();
                if (dItem != null && dItem.isDropped)
                {
                    item.isSorted = true;
                    item.isPackedInKit = droppedInKit;
                    sortedCount++;
                    
                    // Show custom educational feedback based on the item
                    ShowItemFeedback(item, droppedInKit);
                    
                    if (droppedInKit && kitDropSound != null)
                    {
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(kitDropSound);
                    }
                    else if (!droppedInKit && binDropSound != null)
                    {
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(binDropSound);
                    }
                    
                    // Smooth transition or hide the item image or parent it
                    if (!droppedInKit)
                    {
                        // In trash bin: fade out or shrink
                        StartCoroutine(FadeOutUI(item.uiInstance));
                    }
                    
                    break;
                }
            }
        }

        UpdateProgressUI();

        if (sortedCount >= items.Count)
        {
            finishButton.interactable = true;
            feedbackTitleText.text = "ALLE SACHEN EINSORTIERT!";
            feedbackBodyText.text += "\n\nKlicke unten auf 'BEWERTUNG ABSCHLIESSEN', um dein Ergebnis zu sehen!";
        }
    }

    private IEnumerator FadeOutUI(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 4f;
            cg.alpha = t;
            obj.transform.localScale = Vector3.one * t;
            yield return null;
        }
        obj.SetActive(false);
    }

    private void ShowItemFeedback(KitItem item, bool droppedInKit)
    {
        if (droppedInKit)
        {
            feedbackTitleText.text = item.displayName + " eingepackt!";
            feedbackTitleText.color = new Color(0f, 0.4f, 0f); // Dark green
        }
        else
        {
            feedbackTitleText.text = item.displayName + " entsorgt!";
            feedbackTitleText.color = new Color(0.6f, 0.1f, 0.1f); // Dark red
        }

        feedbackBodyText.text = item.explanationText;
    }

    private void UpdateProgressUI()
    {
        progressText.text = string.Format("Einsortiert: {0} / {1}", sortedCount, items.Count);
    }

    public void OnFinishButtonPressed()
    {
        if (isEvaluated)
        {
            // Close immediately
            emergencyKitPanel.SetActive(false);
            if (gameManager != null)
            {
                gameManager.currentPhase = GameManager.GamePhase.Intro;
                gameManager.StartIntroPhase();
                if (gameManager.monitorPanel != null)
                {
                    gameManager.monitorPanel.SetActive(true);
                }
            }
            return;
        }

        isEvaluated = true;
        finishButton.GetComponentInChildren<Text>().text = "SCHLIESSEN";
        finishButton.interactable = true; // Let user close it when they are ready
        
        // Evaluate mistakes
        int mistakes = 0;
        string evalText = "DEINE BEWERTUNG:\n";

        foreach (var item in items)
        {
            if (item.category == ItemCategory.MustHave && !item.isPackedInKit)
            {
                mistakes++;
                evalText += string.Format("❌ {0} MUSS in den Koffer!\n", item.displayName);
            }
            else if (item.category == ItemCategory.NoGo && item.isPackedInKit)
            {
                mistakes++;
                evalText += string.Format("❌ {0} gehört in den Mülleimer!\n", item.displayName);
            }
        }

        int score = Mathf.Max(0, 100 - (mistakes * 25));
        int rewardCoins = score / 2;

        evalText += string.Format("\nErgebnis: {0}% korrekt | Belohnung: {1} Coins!", score, rewardCoins);

        feedbackTitleText.text = score == 100 ? "PERFEKT GEPLANT! 🌟" : "TRAINING BEENDET";
        feedbackTitleText.color = score == 100 ? new Color(0f, 0.6f, 0f) : Color.black;
        feedbackBodyText.text = evalText;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(rewardCoins);
            if (score == 100)
            {
                ScoreManager.Instance.emergencyKitPerfect = true;
            }
        }
    }
}
