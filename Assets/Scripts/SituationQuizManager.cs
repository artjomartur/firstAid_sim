using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Zeigt nach dem Notruf-Call ein kurzes Situations-Quiz:
/// "Was soll ich in dieser Situation tun?"
/// Nach allen Fragen wechselt das Spiel in die Intro-Phase (Freeroam).
/// </summary>
public class SituationQuizManager : MonoBehaviour
{
    public GameManager gameManager;

    [Header("UI References (set by GameBootstrap)")]
    public Text questionText;
    public Button[] answerButtons;   // 3 Buttons
    public Text feedbackText;
    public Button continueButton;    // "Weiter" / "Einsatz starten!"
    public Text progressText;        // z.B. "Frage 1 / 4"

    // ──────────────────────────────────────────────────────────────
    // Quiz-Inhalt
    // ──────────────────────────────────────────────────────────────
    private struct SituationQuestion
    {
        public string question;
        public string[] options;      // exakt 3 Optionen
        public int    correctIdx;
        public string explanation;    // wird bei falscher Antwort angezeigt
    }

    private List<SituationQuestion> questions = new List<SituationQuestion>();

    private int  currentIndex  = 0;
    private int  correctCount  = 0;
    private bool awaitingNext  = false;

    // ──────────────────────────────────────────────────────────────
    // Farben für Feedback
    // ──────────────────────────────────────────────────────────────
    private static readonly Color ColorCorrect = new Color(0.18f, 0.65f, 0.18f);
    private static readonly Color ColorWrong   = new Color(0.80f, 0.15f, 0.15f);
    private static readonly Color ColorNeutral = Color.black;

    // ──────────────────────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        BuildQuestions();
    }

    private void BuildQuestions()
    {
        questions.Add(new SituationQuestion {
            question    = "Du findest eine Person bewusstlos am Boden.\nWas tust du als ERSTES?",
            options     = new[] {
                "Sofort Notruf 112 wählen",
                "Wasser über das Gesicht schütten",
                "Die Person schütteln und schreien"
            },
            correctIdx  = 0,
            explanation = "Richtig! Der Notruf ist immer der erste Schritt –\nnur die Leitstelle kann weitere Hilfe koordinieren."
        });

        questions.Add(new SituationQuestion {
            question    = "Ein Kind hat sich beim Fahrradfahren verletzt\nund blutet stark. Was tust du?",
            options     = new[] {
                "Wunde abdecken & starken Druck ausüben",
                "Die Wunde mit Wasser ausspülen",
                "Abwarten, ob das Bluten von selbst stoppt"
            },
            correctIdx  = 0,
            explanation = "Richtig! Direkter Druck auf die Wunde\nist die effektivste Sofortmaßnahme bei starker Blutung."
        });

        questions.Add(new SituationQuestion {
            question    = "Eine Person greift sich an den Hals\nund kann nicht sprechen oder atmen. Was bedeutet das?",
            options     = new[] {
                "Sie hat sich verschluckt – Heimlich-Manöver!",
                "Sie hat Herzrasen",
                "Sie ist allergisch – Fenster öffnen"
            },
            correctIdx  = 0,
            explanation = "Richtig! Das klassische Zeichen fürs Verschlucken.\nDas Heimlich-Manöver kann Leben retten."
        });

        questions.Add(new SituationQuestion {
            question    = "Eine Person kollabiert und reagiert nicht mehr.\nWas prüfst du als ERSTES?",
            options     = new[] {
                "Bewusstsein & Atmung prüfen",
                "Sofort mit CPR beginnen",
                "Nach Ausweis suchen"
            },
            correctIdx  = 0,
            explanation = "Richtig! Erst Bewusstsein & Atmung prüfen,\ndann entscheiden ob CPR nötig ist."
        });
    }

    // ──────────────────────────────────────────────────────────────
    // Öffentliche API
    // ──────────────────────────────────────────────────────────────
    public void Activate()
    {
        currentIndex = 0;
        correctCount = 0;
        awaitingNext = false;

        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (feedbackText   != null) feedbackText.text = "";

        ShowQuestion();
    }

    // ──────────────────────────────────────────────────────────────
    // Interne Logik
    // ──────────────────────────────────────────────────────────────
    private void ShowQuestion()
    {
        if (currentIndex >= questions.Count)
        {
            ShowCompletion();
            return;
        }

        awaitingNext = false;

        SituationQuestion q = questions[currentIndex];

        if (questionText  != null) questionText.text  = q.question;
        if (feedbackText  != null) feedbackText.text  = "";
        if (progressText  != null) progressText.text  = $"Frage {currentIndex + 1} / {questions.Count}";

        // Reset continue button
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        // Setup answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < q.options.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].interactable = true;

                // Reset button color
                Image btnImg = answerButtons[i].GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(0.85f, 0.85f, 0.85f); // Windows gray

                Text label = answerButtons[i].GetComponentInChildren<Text>();
                if (label != null) label.text = q.options[i];

                int capturedIdx = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIdx));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnAnswerSelected(int idx)
    {
        if (awaitingNext) return;

        awaitingNext = true;
        SituationQuestion q = questions[currentIndex];

        // Disable all buttons so no double-click
        foreach (Button b in answerButtons)
            if (b != null) b.interactable = false;

        if (idx == q.correctIdx)
        {
            correctCount++;

            // Green feedback on correct button
            Image btnImg = answerButtons[idx].GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.6f, 1f, 0.6f);

            if (feedbackText != null)
            {
                feedbackText.color = ColorCorrect;
                feedbackText.text  = "✓ Richtig!\n" + q.explanation;
            }

            if (ScoreManager.Instance != null) ScoreManager.Instance.AddCoins(5);
            GameManager gm = gameManager != null ? gameManager : FindObjectOfType<GameManager>();
            if (gm != null && gm.sfxSource != null && gm.unpauseSound != null)
            {
                gm.sfxSource.PlayOneShot(gm.unpauseSound);
            }
        }
        else
        {
            GameManager gm = gameManager != null ? gameManager : FindObjectOfType<GameManager>();
            if (gm != null && gm.sfxSource != null && gm.pauseSound != null)
            {
                gm.sfxSource.PlayOneShot(gm.pauseSound);
            }
            // Red feedback on wrong button, highlight correct
            Image wrongImg   = answerButtons[idx].GetComponent<Image>();
            Image correctImg = answerButtons[q.correctIdx].GetComponent<Image>();
            if (wrongImg   != null) wrongImg.color   = new Color(1f, 0.6f, 0.6f);
            if (correctImg != null) correctImg.color = new Color(0.6f, 1f, 0.6f);

            if (feedbackText != null)
            {
                feedbackText.color = ColorWrong;
                feedbackText.text  = "✗ Leider falsch.\n" + q.explanation;
            }
        }

        // Show continue / finish button
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            bool isLast = (currentIndex == questions.Count - 1);
            Text btnLabel = continueButton.GetComponentInChildren<Text>();
            if (btnLabel != null)
                btnLabel.text = isLast ? "EINSATZ STARTEN!" : "WEITER";
        }
    }

    /// <summary>Called by the continue button.</summary>
    public void OnContinuePressed()
    {
        currentIndex++;
        ShowQuestion();
    }

    private void ShowCompletion()
    {
        if (questionText != null)
            questionText.text = $"Gut gemacht!\n{correctCount} von {questions.Count} Fragen richtig beantwortet.\n\nDeine Ausrüstung wartet – viel Erfolg beim Einsatz!";

        if (feedbackText != null) feedbackText.text = "";
        if (progressText != null) progressText.text = "Fertig!";

        foreach (Button b in answerButtons)
            if (b != null) b.gameObject.SetActive(false);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            Text btnLabel = continueButton.GetComponentInChildren<Text>();
            if (btnLabel != null) btnLabel.text = "EINSATZ STARTEN!";

            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(LaunchIntroPhase);
        }
    }

    private void LaunchIntroPhase()
    {
        gameObject.SetActive(false); // hide panel
        
        // WICHTIG: Setze die Mission als erledigt, damit sie nicht direkt wieder triggert!
        if (gameManager != null)
        {
            gameManager.bikeAccidentHelped = true;
            gameManager.StartIntroPhase();
        }
    }
}
