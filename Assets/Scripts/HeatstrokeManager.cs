using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeatstrokeManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject heatstrokePanel;
    public Button shadowButton;
    public Button waterButton;
    public Button legsButton;
    public Text instructionText;
    
    private bool inShadow = false;
    private bool watered = false;
    private bool legsUp = false;
    private bool isActive = false;

    
    private void OnEnable()
    {
        GameEvents.OnMinigameStarted += HandleMinigameStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnMinigameStarted -= HandleMinigameStarted;
    }

    private void HandleMinigameStarted(GameManager.VictimType type)
    {
        if (type == GameManager.VictimType.Heatstroke)
        {
            Activate();
        }
    }

    public void Activate()
    {
        heatstrokePanel.SetActive(true);
        inShadow = false;
        watered = false;
        legsUp = false;
        isActive = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        string text = "<b>☀ HITZSCHLAG-NOTFALL</b>\n\n";
        text += "<b>Führe die Schritte der Reihe nach aus:</b>\n\n";
        
        text += (inShadow ? "  ✅" : "  ➤") + " 1. Person in den Schatten bringen\n";
        text += (legsUp ? "  ✅" : (inShadow ? "  ➤" : "  ⬜")) + " 2. Beine hochlagern\n";
        text += (watered ? "  ✅" : (legsUp ? "  ➤" : "  ⬜")) + " 3. Schlückchen Wasser geben\n";

        instructionText.text = text;
        instructionText.supportRichText = true;

        shadowButton.interactable = !inShadow;
        legsButton.interactable = inShadow && !legsUp;
        waterButton.interactable = legsUp && !watered;

        if (inShadow && watered && legsUp) CompleteHeatstroke();
    }

    public void MoveToShadow() { inShadow = true; if (ScoreManager.Instance != null) ScoreManager.Instance.currentMissionCorrect++; UpdateUI(); }
    public void RaiseLegs() { legsUp = true; if (ScoreManager.Instance != null) ScoreManager.Instance.currentMissionCorrect++; UpdateUI(); }
    public void GiveWater() { watered = true; if (ScoreManager.Instance != null) ScoreManager.Instance.currentMissionCorrect++; UpdateUI(); }

    private void CompleteHeatstroke()
    {
        isActive = false;
        instructionText.text = "<b>✅ PERSON IST STABILISIERT!</b>\n\nAlle Maßnahmen erfolgreich durchgeführt.";
        StartCoroutine(FinishSequence());
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(2f);
        heatstrokePanel.SetActive(false);
        GameEvents.OnMinigameCompleted?.Invoke(GameManager.VictimType.Heatstroke);
    }
}
