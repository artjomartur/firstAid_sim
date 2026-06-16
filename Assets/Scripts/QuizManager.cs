using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Verwalter für das interaktive Quiz-System.
/// </summary>
public class QuizManager : MonoBehaviour
{
    [Header("UI Referenzen")]
    public GameObject quizPanel;
    public Text questionText;
    public Button[] answerButtons;

    [Header("Inhalt")]
    public List<QuestionData> allQuestions = new List<QuestionData>();
    private List<QuestionData> activeQuestions = new List<QuestionData>();

    private int currentQuestionIndex = 0;
    private bool isProcessing = false;

    private void OnEnable()
    {
        ResetQuiz();
    }

    /// <summary>
    /// Setzt das Quiz auf den Anfangszustand zurück und wählt 3 zufällige Fragen.
    /// </summary>
    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        isProcessing = false;

        // Reset active list
        activeQuestions.Clear();
        if (allQuestions != null && allQuestions.Count > 0)
        {
            // Create a copy to shuffle
            List<QuestionData> shuffleList = new List<QuestionData>(allQuestions);
            for (int i = 0; i < shuffleList.Count; i++)
            {
                QuestionData temp = shuffleList[i];
                int randomIndex = Random.Range(i, shuffleList.Count);
                shuffleList[i] = shuffleList[randomIndex];
                shuffleList[randomIndex] = temp;
            }

            // Take all questions in the pool
            for (int i = 0; i < shuffleList.Count; i++)
            {
                activeQuestions.Add(shuffleList[i]);
            }
        }

        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (activeQuestions == null || activeQuestions.Count == 0)
        {
            Debug.LogError("QuizManager: Keine aktiven Fragen!");
            EndQuiz();
            return;
        }

        if (currentQuestionIndex >= activeQuestions.Count)
        {
            EndQuiz();
            return;
        }

        QuestionData currentQ = activeQuestions[currentQuestionIndex];
        UpdateUI(currentQ);
    }

    private void UpdateUI(QuestionData q)
    {
        if (questionText != null)
        {
            questionText.text = q.questionText;
        }

        // Shuffle answer order for this display
        List<int> indices = new List<int> { 0, 1, 2 };
        for (int i = 0; i < indices.Count; i++)
        {
            int temp = indices[i];
            int randomIndex = Random.Range(i, indices.Count);
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }

        // New temp correct index based on shuffle
        int visualCorrectIdx = -1;
        for (int i = 0; i < indices.Count; i++)
        {
            if (indices[i] == q.correctAnswerIndex) visualCorrectIdx = i;
        }

        int hiddenIdx = -1;
        ShopManager sm = FindObjectOfType<ShopManager>();
        if (sm != null && sm.sharpEyeActive)
        {
            // Pick a random wrong answer to hide (from the shuffled list)
            List<int> wrongIndices = new List<int>();
            for (int i = 0; i < indices.Count; i++)
            {
                if (indices[i] != q.correctAnswerIndex) wrongIndices.Add(i);
            }
            if (wrongIndices.Count > 0) hiddenIdx = wrongIndices[Random.Range(0, wrongIndices.Count)];
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];
            if (btn == null) continue;

            if (i < indices.Count && i != hiddenIdx)
            {
                int originalIdx = indices[i];
                SetupAnswerButton(btn, q.answers[originalIdx], i, visualCorrectIdx);
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    private void SetupAnswerButton(Button btn, string text, int index, int correctIdx)
    {
        btn.gameObject.SetActive(true);
        btn.interactable = true;
        
        // Farbe zurücksetzen (Premium Blau)
        btn.GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.75f, 0.9f);

        // Text setzen
        Text btnText = btn.GetComponentInChildren<Text>();
        if (btnText != null) btnText.text = text;

        // Klick-Event neu binden mit dem "visuellen" Index
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnAnswerSelected(index, correctIdx));
    }

    private void OnAnswerSelected(int index, int correctIdx)
    {
        if (isProcessing) return;
        StartCoroutine(ProcessAnswerRoutine(index, correctIdx));
    }

    private IEnumerator ProcessAnswerRoutine(int selectedIndex, int correctIdx)
    {
        isProcessing = true;
        LockButtons();

        bool wasCorrect = (selectedIndex == correctIdx);

        // Visuelles Feedback
        ShowVisualFeedback(selectedIndex, correctIdx, wasCorrect);

        if (ScoreManager.Instance != null)
        {
            if (wasCorrect)
            {
                ScoreManager.Instance.quizScore++;
                ScoreManager.Instance.RecordSuccess();
                ScoreManager.Instance.AddCoins(15); // Reward 15 coins for each correct answer!
            }
            else
            {
                ScoreManager.Instance.RecordError();
            }
        }

        // Kurze Pause zum Lesen des Feedbacks
        yield return new WaitForSeconds(1.5f);

        currentQuestionIndex++;
        isProcessing = false;
        ShowQuestion();
    }

    private void LockButtons()
    {
        foreach (Button btn in answerButtons)
        {
            if (btn != null) btn.interactable = false;
        }
    }

    private void ShowVisualFeedback(int selectedIdx, int correctIdx, bool wasCorrect)
    {
        if (selectedIdx < answerButtons.Length && answerButtons[selectedIdx] != null)
        {
            answerButtons[selectedIdx].GetComponent<Image>().color = wasCorrect ? Color.green : Color.red;
        }

        // Wenn falsch geantwortet, zeige die richtige Lösung zusätzlich in Grün
        if (!wasCorrect && correctIdx < answerButtons.Length && answerButtons[correctIdx] != null)
        {
            answerButtons[correctIdx].GetComponent<Image>().color = Color.green;
        }
    }

    private void EndQuiz()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (ScoreManager.Instance != null && ScoreManager.Instance.quizScore < 2)
            {
                gm.lastMissionSuccess = false;
            }
            gm.OnQuizCompleted();
        }
    }
}
