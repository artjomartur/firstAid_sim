using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class StabilizeManager : MonoBehaviour
{
    public GameObject stabilizePanel;
    public Image victimBodyImage;
    public Image backgroundOverlay;
    public Text instructionText;
    
    [Header("Poses")]
    public Sprite originalPose;
    public Sprite armUpPose;
    public Sprite legBentPose;
    public Sprite finalSidePose;
    
    [Header("Interaction")]
    public RectTransform dragHandle; // Das Element, das der Spieler zieht
    public RectTransform targetZone; // Wo das Element hin soll

    private int currentStep = 0; // 0: Head (Check), 1: Arm, 2: Leg, 3: Tilt
    private bool isDragging = false;
    private bool gameActive = false;

    public void Activate()
    {
        stabilizePanel.SetActive(true);
        currentStep = 0;
        gameActive = true;
        victimBodyImage.sprite = originalPose;
        dragHandle.gameObject.SetActive(true);
        UpdateUI();
    }

    private void UpdateUI()
    {
        switch (currentStep)
        {
            case 0: instructionText.text = "1. Prüfe die Atmung! Ziehe den Kopf leicht nach hinten."; break;
            case 1: instructionText.text = "2. Ziehe den nahen Arm nach oben."; break;
            case 2: instructionText.text = "3. Ziehe das ferne Bein nach oben."; break;
            case 3: instructionText.text = "4. Ziehe an der Schulter, um die Person auf die Seite zu rollen."; break;
            case 4: instructionText.text = "Hervorragend! Die Person ist stabilisiert."; break;
        }
        
        // Positioniere Handle und Target je nach Schritt
        UpdateInteractionPoints();
    }

    private void UpdateInteractionPoints()
    {
        Vector2 handlePos = Vector2.zero;
        Vector2 targetPos = Vector2.zero;

        switch (currentStep)
        {
            case 0: // Head
                handlePos = new Vector2(0, 300);
                targetPos = new Vector2(0, 350);
                break;
            case 1: // Arm
                handlePos = new Vector2(-150, 50);
                targetPos = new Vector2(-150, 250);
                break;
            case 2: // Leg
                handlePos = new Vector2(-100, -250);
                targetPos = new Vector2(-250, -150);
                break;
            case 3: // Shoulder
                handlePos = new Vector2(150, 0);
                targetPos = new Vector2(-100, 0);
                break;
        }

        dragHandle.anchoredPosition = handlePos;
        targetZone.anchoredPosition = targetPos;
    }

    void Update()
    {
        if (!gameActive || currentStep >= 4) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(dragHandle, Input.mousePosition))
            {
                isDragging = true;
            }
        }

        if (isDragging)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(stabilizePanel.GetComponent<RectTransform>(), Input.mousePosition, null, out localPoint);
            dragHandle.anchoredPosition = localPoint;

            if (Vector2.Distance(dragHandle.anchoredPosition, targetZone.anchoredPosition) < 50f)
            {
                OnStepCompleted();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            UpdateInteractionPoints(); // Reset position if not reached target
        }
    }

    private void OnStepCompleted()
    {
        isDragging = false;
        currentStep++;
        
        // Update Sprite
        switch (currentStep)
        {
            case 1: break; // Stay original for check
            case 2: victimBodyImage.sprite = armUpPose; break;
            case 3: victimBodyImage.sprite = legBentPose; break;
            case 4: victimBodyImage.sprite = finalSidePose; break;
        }

        UpdateUI();

        if (currentStep >= 4)
        {
            dragHandle.gameObject.SetActive(false);
            targetZone.gameObject.SetActive(false);
            Invoke("FinishLevel", 2.5f);
        }
    }

    private void FinishLevel()
    {
        gameActive = false;
        FindObjectOfType<GameManager>().OnStabilizeCompleted();
    }
}
