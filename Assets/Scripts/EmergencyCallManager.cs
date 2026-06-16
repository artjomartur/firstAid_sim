using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class EmergencyCallManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject callPanel;
    public Text dispatcherText;
    public Button[] answerButtons;
    
    [Header("Audio Settings")]
    public AudioClip telephoneSound;
    public AudioClip muffledTalkSound;
    
    private int currentStep = 0;
    private bool isActive = false;
    private Action onCallCompleted;

    private struct CallStep {
        public string question;
        public string[] options;
        public int correctIdx;
    }

    private List<CallStep> callSteps = new List<CallStep>();

    private void SetupDynamicSteps(GameManager.VictimType type)
    {
        callSteps.Clear();

        // Wo
        callSteps.Add(new CallStep {
            question = "Leitstelle: 'Wo ist der Notfallort?'",
            options = new string[] { "Im Park", "Weiß ich nicht", "Zu Hause" },
            correctIdx = 0
        });

        // Was
        string wasGeschehen = "Ein medizinischer Notfall";
        string fakeWas1 = "Jemand hat sich verlaufen";
        string fakeWas2 = "Ein Feuer ist ausgebrochen";

        if (type == GameManager.VictimType.HeartAttack) wasGeschehen = "Jemand hat sich ans Herz gefasst und ist kollabiert";
        else if (type == GameManager.VictimType.BoneFracture) wasGeschehen = "Jemand ist gestürzt und hat Schmerzen am Bein";
        else if (type == GameManager.VictimType.Snakebite) wasGeschehen = "Jemand wurde von einer Schlange gebissen";

        callSteps.Add(new CallStep {
            question = "Leitstelle: 'Was ist passiert?'",
            options = new string[] { wasGeschehen, fakeWas1, fakeWas2 },
            correctIdx = 0
        });

        // Wie viele
        callSteps.Add(new CallStep {
            question = "Leitstelle: 'Wie viele Personen sind verletzt?'",
            options = new string[] { "Eine Person", "Drei Personen", "Zehn Personen" },
            correctIdx = 0
        });

        // Welche Verletzungen
        string verletzung = "Unklar, braucht Hilfe";
        if (type == GameManager.VictimType.HeartAttack) verletzung = "Bewusstlosigkeit, Verdacht auf Herzinfarkt";
        else if (type == GameManager.VictimType.BoneFracture) verletzung = "Sichtbarer Knochenbruch";
        else if (type == GameManager.VictimType.Snakebite) verletzung = "Bisswunde, Kreislaufprobleme";

        callSteps.Add(new CallStep {
            question = "Leitstelle: 'Welche Verletzungen liegen vor?'",
            options = new string[] { verletzung, "Nur ein kleiner Kratzer", "Keine sichtbaren" },
            correctIdx = 0
        });

        // Warten
        callSteps.Add(new CallStep {
            question = "Leitstelle: 'Warten Sie auf Rückfragen!'",
            options = new string[] { "Ich bleibe am Apparat", "Ich lege jetzt auf", "Tschüss!" },
            correctIdx = 0
        });
    }

    // Overload for Intro compatibility
    public void Activate()
    {
        ActivateDynamic(GameManager.VictimType.BikeAccident, () => {
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddCoins(20);
            if (gameManager != null) gameManager.StartIntroPhase();
        });
    }

    public void ActivateDynamic(GameManager.VictimType type, Action onCompleted)
    {
        SetupDynamicSteps(type);
        onCallCompleted = onCompleted;

        callPanel.SetActive(true);
        callPanel.transform.SetAsLastSibling();
        currentStep = 0;
        isActive = true;

        if (gameManager != null && gameManager.sfxSource != null)
        {
            float delay = 0f;
            if (telephoneSound != null)
            {
                gameManager.sfxSource.PlayOneShot(telephoneSound);
                delay = telephoneSound.length;
            }
            StartCoroutine(PlayMuffledTalkAfterDelay(delay));
        }

        ShowStep();
    }

    private IEnumerator PlayMuffledTalkAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameManager != null && gameManager.sfxSource != null && muffledTalkSound != null && isActive)
        {
            gameManager.sfxSource.PlayOneShot(muffledTalkSound);
        }
    }

    private void ShowStep()
    {
        if (currentStep >= callSteps.Count)
        {
            CompleteCall();
            return;
        }

        CallStep step = callSteps[currentStep];
        dispatcherText.text = step.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < step.options.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].GetComponentInChildren<Text>().text = step.options[i];
                int idx = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(idx));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnAnswerSelected(int idx)
    {
        if (idx == callSteps[currentStep].correctIdx)
        {
            currentStep++;
            ShowStep();
        }
        else
        {
            dispatcherText.text = "Leitstelle: 'Können Sie das genauer sagen?'";
        }
    }

    private void CompleteCall()
    {
        isActive = false;
        StopAllCoroutines();
        callPanel.SetActive(false);
        onCallCompleted?.Invoke();
    }
}
