using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Vergiftungs-Mission: Drei Phasen.
/// 1. Pilze untersuchen
/// 2. Giftnotruf 19240 aktiv am Telefon eintippen
/// 3. Anweisung befolgen (Wasser geben)
/// </summary>
public class PoisonManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject poisonPanel;
    public Text instructionText;

    [Header("Phase 1 – Untersuchen")]
    public Button examineButton;

    [Header("Phase 2 – Telefon")]
    public GameObject phonePanel;       // The phone UI container
    public Text dialDisplay;            // Shows the typed number
    public Text phoneStatusText;        // "Verbinde..." / "Falsche Nummer!" etc.
    public Button[] digitButtons;       // 0-9 buttons
    public Button callDialButton;       // Green "Anrufen" button
    public Button deleteButton;         // Backspace button

    [Header("Phase 3 – Wasser")]
    public Button waterButton;
    public GameObject waterPanel;

    // ── State ──
    private enum Phase { Examine, Dial, Water, Done }
    private Phase currentPhase = Phase.Examine;
    private string dialedNumber = "";
    private const string CORRECT_NUMBER = "19240";
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
        if (type == GameManager.VictimType.Poisoning)
        {
            Activate();
        }
    }

    public void Activate()
    {
        poisonPanel.SetActive(true);
        currentPhase = Phase.Examine;
        dialedNumber = "";
        isActive = true;

        ShowPhase(Phase.Examine);
    }

    // ── Phase control ──────────────────────────────────────────────────────────

    private void ShowPhase(Phase p)
    {
        currentPhase = p;

        bool showExamine = (p == Phase.Examine);
        bool showPhone   = (p == Phase.Dial);
        bool showWater   = (p == Phase.Water);

        if (examineButton != null) examineButton.gameObject.SetActive(showExamine);
        if (phonePanel != null)   phonePanel.SetActive(showPhone);
        if (waterPanel != null)   waterPanel.SetActive(showWater);

        switch (p)
        {
            case Phase.Examine:
                instructionText.text = "⚠️ Die Person hat giftige Pilze gegessen!\n\n" +
                                       "Schritt 1: Untersuche die Pilze, um das Gift zu identifizieren.";
                break;

            case Phase.Dial:
                dialedNumber = "";
                UpdateDialDisplay();
                if (phoneStatusText != null)
                    phoneStatusText.text = "Wähle den Giftnotruf: 1 9 2 4 0";
                instructionText.text = "📞 Schritt 2: Rufe den Giftnotruf an!\n" +
                                       "Tippe die Nummer 19240 ein und drücke ANRUFEN.";
                break;

            case Phase.Water:
                instructionText.text = "💧 Schritt 3: Der Giftnotruf rät zur Flüssigkeit.\n" +
                                       "Gib der Person stilles Wasser oder Tee (kleine Schlückchen).";
                break;
        }
    }

    // ── Phase 1 ───────────────────────────────────────────────────────────────

    public void OnExamineClicked()
    {
        ScoreManager.Instance?.RecordSuccess();
        ShowPhase(Phase.Dial);
    }

    // ── Phase 2: Telefon ──────────────────────────────────────────────────────

    public void OnDigitPressed(string digit)
    {
        if (currentPhase != Phase.Dial) return;
        if (dialedNumber.Length >= 8) return; // Max length guard

        dialedNumber += digit;
        UpdateDialDisplay();

        // Live validation hint
        if (phoneStatusText != null)
        {
            bool prefixOk = CORRECT_NUMBER.StartsWith(dialedNumber);
            phoneStatusText.text = prefixOk
                ? "Wähle den Giftnotruf: 1 9 2 4 0"
                : "<color=red>⚠ Falsche Ziffer!</color>";
        }
    }

    public void OnDeletePressed()
    {
        if (dialedNumber.Length == 0) return;
        dialedNumber = dialedNumber.Substring(0, dialedNumber.Length - 1);
        UpdateDialDisplay();
        if (phoneStatusText != null)
            phoneStatusText.text = "Wähle den Giftnotruf: 1 9 2 4 0";
    }

    public void OnCallPressed()
    {
        if (currentPhase != Phase.Dial) return;

        if (dialedNumber == CORRECT_NUMBER)
        {
            // Correct!
            ScoreManager.Instance?.RecordSuccess();
            if (phoneStatusText != null)
                phoneStatusText.text = "✅ Verbinde mit Giftnotruf 19240...";
            StartCoroutine(ConnectCall());
        }
        else
        {
            // Wrong number
            ScoreManager.Instance?.RecordError();
            if (phoneStatusText != null)
                phoneStatusText.text = string.IsNullOrEmpty(dialedNumber)
                    ? "<color=red>Keine Nummer eingegeben!</color>"
                    : $"<color=red>❌ Falsche Nummer: {dialedNumber}\nRichtig: 19240 (Giftnotruf)</color>";

            StartCoroutine(ShakeDisplay());
        }
    }

    private IEnumerator ConnectCall()
    {
        // Simulate dial tone + connection
        if (dialDisplay != null) dialDisplay.text = "📞 Verbinde...";
        yield return new WaitForSeconds(0.5f);
        if (dialDisplay != null) dialDisplay.text = "📞 GIFTNOTRUF 19240";
        if (phoneStatusText != null)
            phoneStatusText.text = "Giftnotruf: \"Gib Wasser/Tee, KEIN Erbrechen auslösen!\"";
        yield return new WaitForSeconds(2f);
        ShowPhase(Phase.Water);
    }

    private IEnumerator ShakeDisplay()
    {
        if (dialDisplay == null) yield break;
        RectTransform rt = dialDisplay.GetComponent<RectTransform>();
        Vector2 orig = rt.anchoredPosition;
        for (int i = 0; i < 6; i++)
        {
            rt.anchoredPosition = orig + new Vector2(Random.Range(-8f, 8f), 0);
            yield return new WaitForSeconds(0.05f);
        }
        rt.anchoredPosition = orig;
    }

    private void UpdateDialDisplay()
    {
        if (dialDisplay == null) return;
        // Show as spaced digits for readability
        string spaced = "";
        foreach (char c in dialedNumber) spaced += c + " ";
        dialDisplay.text = string.IsNullOrEmpty(dialedNumber) ? "_ _ _ _ _" : spaced.TrimEnd();
    }

    // ── Phase 3 ───────────────────────────────────────────────────────────────

    public void OnWaterClicked()
    {
        ScoreManager.Instance?.RecordSuccess();
        instructionText.text = "✅ Sehr gut! Erste Hilfe bei Vergiftung erfolgreich!\nRettungsdienst wurde verständigt.";
        StartCoroutine(FinishSequence());
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(2.5f);
        poisonPanel.SetActive(false);
        isActive = false;
        GameEvents.OnMinigameCompleted?.Invoke(GameManager.VictimType.Poisoning);
    }
}
