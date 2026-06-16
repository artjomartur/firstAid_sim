using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BandageManager : MonoBehaviour
{
    public GameObject bandagePanel;
    public RectTransform woundArea;
    public Text instructionText;
    public Slider timerSlider;

    private int currentPhase = 0; // 0: Clean, 1: Gauze, 2: Wrap
    private float timeLeft = 45f;
    private bool gameActive = false;

    private GameObject rucksackPanel;
    private DropZone woundDropZone;

    private GameObject btnClean;
    private GameObject btnGauze;
    private GameObject btnWrap;

    public void Activate()
    {
        bandagePanel.SetActive(true);
        currentPhase = 0;
        timeLeft = 45f;
        gameActive = true;
        
        if (timerSlider != null)
        {
            timerSlider.maxValue = timeLeft;
            timerSlider.value = timeLeft;
        }

        SetupDragAndDropUI();
        UpdateInstructions();
    }

    private void SetupDragAndDropUI()
    {
        // Cleanup old Rucksack
        if (rucksackPanel != null) Destroy(rucksackPanel);
        
        // Ensure wound area has a DropZone
        if (woundDropZone == null)
        {
            woundDropZone = woundArea.gameObject.GetComponent<DropZone>();
            if (woundDropZone == null) woundDropZone = woundArea.gameObject.AddComponent<DropZone>();
            
            // Need an Image component to detect raycasts for Drop
            Image woundImg = woundArea.GetComponent<Image>();
            if (woundImg == null)
            {
                woundImg = woundArea.gameObject.AddComponent<Image>();
                woundImg.color = new Color(1, 1, 1, 0); // Transparent but raycastable
            }
            woundDropZone.onDropSuccess = OnItemDropped;
        }

        // Create Rucksack Panel at bottom or side
        rucksackPanel = UIFactory.CreateUIElement(bandagePanel.GetComponent<RectTransform>(), "RucksackPanel", new Vector2(0, -250), new Vector2(600, 150));
        Image bgImg = rucksackPanel.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Create the 3 Draggable Items
        btnClean = CreateDraggableItem("CleanSpray", "Wund-Spray", new Vector2(-200, 0), Color.blue);
        btnGauze = CreateDraggableItem("Gauze", "Gaze", new Vector2(0, 0), Color.white);
        btnWrap = CreateDraggableItem("Wrap", "Verband", new Vector2(200, 0), Color.green);
    }

    private GameObject CreateDraggableItem(string id, string label, Vector2 pos, Color col)
    {
        GameObject itemObj = new GameObject("DragItem_" + id);
        itemObj.transform.SetParent(rucksackPanel.transform, false);
        
        RectTransform rt = itemObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        rt.anchoredPosition = pos;

        Image img = itemObj.AddComponent<Image>();
        img.color = col;

        Text txt = UIFactory.CreateText(itemObj.transform, "Label", label, Vector2.zero, 18, TextAnchor.MiddleCenter);
        txt.color = Color.black;

        DragItem drag = itemObj.AddComponent<DragItem>();
        drag.isDropped = false;
        
        return itemObj;
    }

    private void OnItemDropped()
    {
        // Find which item was just dropped
        DragItem dragClean = btnClean?.GetComponent<DragItem>();
        DragItem dragGauze = btnGauze?.GetComponent<DragItem>();
        DragItem dragWrap = btnWrap?.GetComponent<DragItem>();

        if (currentPhase == 0 && dragClean != null && dragClean.isDropped)
        {
            // Success Clean
            ScoreManager.Instance?.RecordSuccess();
            Destroy(btnClean);
            currentPhase++;
            UpdateInstructions();
        }
        else if (currentPhase == 1 && dragGauze != null && dragGauze.isDropped)
        {
            // Success Gauze
            ScoreManager.Instance?.RecordSuccess();
            Destroy(btnGauze);
            currentPhase++;
            UpdateInstructions();
        }
        else if (currentPhase == 2 && dragWrap != null && dragWrap.isDropped)
        {
            // Success Wrap
            ScoreManager.Instance?.RecordSuccess();
            Destroy(btnWrap);
            currentPhase++;
            UpdateInstructions();
            Win();
        }
        else
        {
            // Wrong item or order
            ScoreManager.Instance?.RecordError();
            
            // Revert the incorrectly dropped items
            if (dragClean != null && dragClean.isDropped && currentPhase != 0)
            {
                dragClean.isDropped = false;
                dragClean.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 0);
            }
            if (dragGauze != null && dragGauze.isDropped && currentPhase != 1)
            {
                dragGauze.isDropped = false;
                dragGauze.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            }
            if (dragWrap != null && dragWrap.isDropped && currentPhase != 2)
            {
                dragWrap.isDropped = false;
                dragWrap.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, 0);
            }
        }
    }

    private void Update()
    {
        if (!gameActive) return;

        timeLeft -= Time.deltaTime;
        if (timerSlider != null) timerSlider.value = timeLeft;

        if (timeLeft <= 0) Fail();
    }

    private void UpdateInstructions()
    {
        switch (currentPhase)
        {
            case 0: instructionText.text = "REINIGEN: Ziehe das Wund-Spray auf die Wunde!"; break;
            case 1: instructionText.text = "GAZE: Ziehe die Gaze auf die Wunde!"; break;
            case 2: instructionText.text = "FIXIEREN: Ziehe den Verband auf die Wunde!"; break;
        }
    }

    private void Win()
    {
        gameActive = false;
        instructionText.text = "WUNDE PERFEKT VERSORGT!";
        if (ScoreManager.Instance != null) ScoreManager.Instance.bandageScore = (int)(timeLeft * 20);
        Invoke("NotifyManager", 2f);
    }

    private void Fail()
    {
        gameActive = false;
        instructionText.text = "ZEIT ABGELAUFEN!";
        FindObjectOfType<GameManager>().lastMissionSuccess = false;
        Invoke("NotifyManager", 2f);
    }

    private void NotifyManager()
    {
        if (rucksackPanel != null) Destroy(rucksackPanel);
        bandagePanel.SetActive(false);
        FindObjectOfType<GameManager>().OnBandageCompleted();
    }
}
