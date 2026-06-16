using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TriageManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject triagePanel;
    public Text victimInfoText;
    public Button redButton, yellowButton, greenButton, blackButton;
    
    private int currentVictim = 0;
    private int totalVictims = 3;
    private string[] victimSymptoms = {
        "Person atmet nicht, kein Puls.",
        "Person hat eine stark blutende Wunde am Bein.",
        "Person ist ansprechbar, hat nur Schürfwunden."
    };
    private int[] correctTriage = { 3, 1, 2 }; // 3: Black, 1: Yellow, 2: Green

    
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
        if (type == GameManager.VictimType.TriageScene)
        {
            Activate();
        }
    }

    public void Activate()
    {
        triagePanel.SetActive(true);
        currentVictim = 0;
        ShowVictim();
    }

    private void ShowVictim()
    {
        if (currentVictim >= totalVictims)
        {
            CompleteTriage();
            return;
        }
        victimInfoText.text = "PATIENT #" + (currentVictim + 1) + ":\n" + victimSymptoms[currentVictim];
    }

    public void AssignTriage(int colorIdx)
    {
        if (colorIdx == correctTriage[currentVictim])
        {
            currentVictim++;
            ShowVictim();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ShowErrorRoutine());
        }
    }

    private IEnumerator ShowErrorRoutine()
    {
        victimInfoText.text = "<color=red>FALSCHE EINSCHÄTZUNG!</color>\nPrüfe die Symptome noch einmal.";
        yield return new WaitForSeconds(2f);
        ShowVictim();
    }

    private void CompleteTriage()
    {
        victimInfoText.text = "TRIAGE ABGESCHLOSSEN! RETTUNG IST UNTERWEGS.";
        StartCoroutine(FinishSequence());
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(2f);
        triagePanel.SetActive(false);
        GameEvents.OnMinigameCompleted?.Invoke(GameManager.VictimType.TriageScene);
    }
}
