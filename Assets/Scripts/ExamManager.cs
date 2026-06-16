using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ExamManager : MonoBehaviour
{
    public static ExamManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject panel;
    private RectTransform panelRT;
    
    // Screens
    private GameObject startScreen;
    private GameObject questionScreen;
    private GameObject resultScreen;

    // Start Screen UI
    private Text startDescription;
    private Button startButton;

    // Question Screen UI
    private Text progressText;
    private Text questionText;
    private Button[] answerButtons = new Button[3];
    private Text[] answerTexts = new Text[3];

    // Result Screen UI
    private Text resultTitle;
    private Text resultText;
    private Button actionButton; // OK or Retry
    private Button openCertButton; // Certificate direct link

    // Exam State
    private List<ExamQuestion> questionPool = new List<ExamQuestion>();
    private List<ExamQuestion> activeQuestions = new List<ExamQuestion>();
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private bool isProcessingAnswer = false;
    private List<int> visualToOriginalIndex = new List<int>();
    private int visualCorrectIndex = -1;

    private struct ExamQuestion
    {
        public string questionDe;
        public string questionEn;
        public string[] answersDe;
        public string[] answersEn;
        public int correctIndex; // 0-based index in the arrays
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupExamQuestions();
            SetupExamUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupExamQuestions()
    {
        // 1. Bicycle accident (Fahrradunfall)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was ist der erste Schritt bei einem Fahrradunfall mit einer bewusstlosen Person?",
            questionEn = "What is the first step in a bicycle accident with an unconscious person?",
            answersDe = new string[] { "Atmung prüfen & Stabile Seitenlage herstellen", "Wunden verbinden", "Direkt Herzdruckmassage starten" },
            answersEn = new string[] { "Check breathing & establish Recovery Position", "Bandage wounds", "Start chest compressions immediately" },
            correctIndex = 0
        });

        // 2. Severe bleeding (Stark blutende Wunde)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Wie versorgt man eine stark spritzende arterielle Blutung am Arm?",
            questionEn = "How do you treat a severely spurting arterial bleeding on the arm?",
            answersDe = new string[] { "Wunde mit Eis kühlen", "Einen Druckverband anlegen & Arm hochlagern", "Wunde mit einem nassen Tuch abdecken" },
            answersEn = new string[] { "Cool the wound with ice", "Apply a pressure bandage & elevate the arm", "Cover the wound with a wet cloth" },
            correctIndex = 1
        });

        // 3. Unconscious person (Bewusstlose Person)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was tun Sie, wenn eine Person bewusstlos ist, aber normal atmet?",
            questionEn = "What do you do if a person is unconscious but breathing normally?",
            answersDe = new string[] { "In die stabile Seitenlage bringen & 112 anrufen", "Sofort Herzdruckmassage beginnen", "Der Person Wasser zu trinken geben" },
            answersEn = new string[] { "Put in Recovery Position & call 112", "Start chest compressions immediately", "Give the person water to drink" },
            correctIndex = 0
        });

        // 4. Burns (Verbrennungen)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Wie kühlt man eine Brandwunde richtig?",
            questionEn = "How do you properly cool a burn wound?",
            answersDe = new string[] { "Mit eiskaltem Wasser oder Eiswürfeln", "Mit lauwarmem Wasser für ca. 10 Minuten", "Mit Brandsalbe oder Zahnpasta" },
            answersEn = new string[] { "With ice-cold water or ice cubes", "With lukewarm running water for about 10 minutes", "With burn ointment or toothpaste" },
            correctIndex = 1
        });

        // 5. Choking (Verschlucken)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was tun Sie zuerst, wenn sich ein Erwachsener schwer verschluckt hat und keine Luft bekommt?",
            questionEn = "What do you do first if an adult is choking severely and cannot breathe?",
            answersDe = new string[] { "5 kräftige Schläge zwischen die Schulterblätter geben", "Sofort den Heimlich-Griff (Baucheskompression) anwenden", "Den Patienten flach hinlegen" },
            answersEn = new string[] { "Give 5 firm back blows between the shoulder blades", "Use the Heimlich maneuver (abdominal thrusts) immediately", "Lay the patient flat on their back" },
            correctIndex = 0
        });

        // 6. Heatstroke (Hitzeschlag)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Wohin bringt man eine Person mit Verdacht auf Hitzeschlag?",
            questionEn = "Where do you move a person with suspected heatstroke?",
            answersDe = new string[] { "In die pralle Sonne zum Aufwärmen", "In den Schatten, Oberkörper erhöht lagern & kühlen", "Sofort in eine eiskalte Badewanne legen" },
            answersEn = new string[] { "In the direct sun to warm them up", "In the shade, elevate upper body & cool gently", "Put immediately into an ice-cold bathtub" },
            correctIndex = 1
        });

        // 7. Electric shock (Stromschlag)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was ist die allererste Maßnahme bei einem Stromunfall?",
            questionEn = "What is the absolute first action in an electrical accident?",
            answersDe = new string[] { "Eigenschutz beachten und Stromkreis trennen (Sicherung aus)", "Den Verletzten sofort anfassen und wegziehen", "Den Verletzten mit Wasser übergießen" },
            answersEn = new string[] { "Ensure self-protection & disconnect power (fuse off)", "Grab the victim immediately and pull them away", "Pour water over the victim" },
            correctIndex = 0
        });

        // 8. Poisoning (Vergiftung)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was sollte man nach dem Verschlucken von Giftstoffen NIEMALS tun?",
            questionEn = "What should you NEVER do after swallowing toxic substances?",
            answersDe = new string[] { "Erbrechen künstlich herbeiführen", "Den Giftnotruf oder 112 anrufen", "Die Verpackung des Giftstoffs für den Arzt sichern" },
            answersEn = new string[] { "Force vomiting", "Call Poison Control or 112", "Secure the toxic substance packaging for the doctor" },
            correctIndex = 0
        });

        // 9. Triage
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Welche Farbe erhalten Triage-Patienten, die sofortige Hilfe benötigen?",
            questionEn = "Which color tag is given to triage patients requiring immediate help?",
            answersDe = new string[] { "Rot (Immediate / Sofort)", "Gelb (Delayed / Verzögert)", "Grün (Minor / Leicht)" },
            answersEn = new string[] { "Red (Immediate)", "Yellow (Delayed)", "Green (Minor)" },
            correctIndex = 0
        });

        // 10. Bone fracture (Knochenbruch)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Wie versorgt man einen gebrochenen Arm richtig?",
            questionEn = "How do you properly treat a fractured arm?",
            answersDe = new string[] { "Den Knochen selbstständig geradebiegen", "Ruhigstellen, schienen und nicht unnötig bewegen", "Den Arm kräftig dehnen und massieren" },
            answersEn = new string[] { "Straighten the bone yourself", "Immobilize, splint, and do not move unnecessarily", "Stretch and massage the arm vigorously" },
            correctIndex = 1
        });

        // 11. Allergic shock (Allergischer Schock)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was ist das wichtigste Notfallmedikament bei einem anaphylaktischen Schock?",
            questionEn = "What is the most critical emergency medication in anaphylactic shock?",
            answersDe = new string[] { "Schmerzmittel (z.B. Ibuprofen)", "Adrenalin-Autoinjektor (EpiPen)", "Antiallergischer Hustensaft" },
            answersEn = new string[] { "Painkillers (e.g., Ibuprofen)", "Adrenaline auto-injector (EpiPen)", "Anti-allergic cough syrup" },
            correctIndex = 1
        });

        // 12. Drowning (Ertrinken)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was ist nach der Rettung einer ertrunkenen Person ohne Atmung sofort zu tun?",
            questionEn = "What must be done immediately after rescuing a drowning person who is not breathing?",
            answersDe = new string[] { "Den Patienten auf den Bauch legen und auf den Rücken schlagen", "5 Beatmungen durchführen, dann mit Herzdruckmassage starten", "Nur die stabile Seitenlage anwenden" },
            answersEn = new string[] { "Place the patient face down and beat their back", "Give 5 rescue breaths, then start chest compressions", "Establish the recovery position only" },
            correctIndex = 1
        });

        // 13. Diabetic shock (Unterzuckerung)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Was gibt man einer ansprechbaren Person mit starker Unterzuckerung?",
            questionEn = "What do you give to a conscious person with severe hypoglycemia?",
            answersDe = new string[] { "Traubenzucker oder zuckerhaltige Getränke", "Eine Dosis Insulin spritzen", "Nur ein Glas klares Wasser" },
            answersEn = new string[] { "Dextrose/sugar tablets or sugary drinks", "Inject a dose of insulin", "Only a glass of plain water" },
            correctIndex = 0
        });

        // 14. Panic attack (Panikattacke)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Wie beruhigt man jemanden mit einer akuten Hyperventilation?",
            questionEn = "How do you calm someone down who is hyperventilating?",
            answersDe = new string[] { "Die Person schütteln und laut anweisen aufzuhören", "Zum bewussten, langsamen Atmen anleiten (z. B. in Papiertüte)", "Sofort hinlegen und künstlich beatmen" },
            answersEn = new string[] { "Shake the person and loudly command them to stop", "Guide them to breathe slowly and consciously (e.g., in a paper bag)", "Lay down immediately and start rescue breaths" },
            correctIndex = 1
        });

        // 15. Stroke (Schlaganfall - FAST)
        questionPool.Add(new ExamQuestion
        {
            questionDe = "Wofür steht das 'S' im FAST-Test bei einem Schlaganfallverdacht?",
            questionEn = "What does the 'S' stand for in the FAST stroke assessment?",
            answersDe = new string[] { "Speech / Sprache prüfen (Sprechen lassen)", "Smile / Lächeln prüfen", "Shock / Schock-Lagerung durchführen" },
            answersEn = new string[] { "Speech / check speech (ask to repeat a sentence)", "Smile / check smile alignment", "Shock / perform shock position" },
            correctIndex = 0
        });
    }

    private void SetupExamUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        Sprite baseSprite = bootstrap != null ? bootstrap.winBaseSprite : null;
        Sprite headerSprite = bootstrap != null ? bootstrap.winHeaderSprite : null;
        Sprite innerFrameSprite = bootstrap != null ? bootstrap.winInnerFrameSprite : null;
        Sprite buttonSprite = bootstrap != null ? bootstrap.winButtonSprite : null;

        // Main Window Panel
        panel = new GameObject("ExamWindow");
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        panelRT = panel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(850, 640);
        panelRT.anchoredPosition = new Vector2(10, -10);

        Image bgImage = panel.AddComponent<Image>();
        if (baseSprite != null)
        {
            bgImage.sprite = baseSprite;
            bgImage.type = Image.Type.Sliced;
        }
        else bgImage.color = new Color(0.85f, 0.85f, 0.85f);

        // Window shadow (parented to panel so it moves with it)
        GameObject shadowObj = UIFactory.CreateUIElement(panelRT, "ExamWindow_Shadow", new Vector2(4, -4), new Vector2(850, 640));
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        panel.AddComponent<DialogPopIn>();

        // Header Title Bar
        GameObject header = UIFactory.CreateUIElement(panelRT, "Header", new Vector2(0, 296), new Vector2(840, 40));
        if (headerSprite != null) UIFactory.SetupImage(header, headerSprite, false);
        else header.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.5f);
        header.AddComponent<WindowDragger>();

        Text headerText = UIFactory.CreateText(header.transform, "Title", "✍️ Erste-Hilfe-Prüfung.exe", new Vector2(15, 0), 20, TextAnchor.MiddleLeft);
        headerText.color = Color.white;
        headerText.fontStyle = FontStyle.Bold;

        // Close Button in header
        GameObject closeBtn = UIFactory.CreateButton(header.GetComponent<RectTransform>(), "CloseBtn", "X", new Vector2(395, 0), new Vector2(32, 28), buttonSprite);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => DialogPopOut.Trigger(panel));
        Text closeTxt = closeBtn.GetComponentInChildren<Text>();
        if (closeTxt != null) closeTxt.color = Color.black;

        // Accent Line
        GameObject accentLine = UIFactory.CreateUIElement(panelRT, "AccentLine", new Vector2(0, 274), new Vector2(840, 4));
        accentLine.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f);
        accentLine.AddComponent<AccentPulse>();

        // ── Screens Setup ──
        // 1. Start Screen
        startScreen = UIFactory.CreateUIElement(panelRT, "StartScreen", new Vector2(0, -30), new Vector2(800, 520));
        startScreen.GetComponent<Image>().color = Color.clear;

        GameObject descFrame = UIFactory.CreateUIElement(startScreen.transform as RectTransform, "DescFrame", new Vector2(0, 60), new Vector2(760, 360));
        UIFactory.SetupImage(descFrame, innerFrameSprite, false);
        descFrame.GetComponent<Image>().type = Image.Type.Sliced;
        descFrame.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);

        startDescription = UIFactory.CreateText(descFrame.transform, "DescText", "", Vector2.zero, 18, TextAnchor.MiddleCenter);
        startDescription.color = Color.black;
        startDescription.supportRichText = true;
        startDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(710, 320);

        GameObject startBtnObj = UIFactory.CreateButton(startScreen.transform as RectTransform, "StartExamBtn", "PRÜFUNG STARTEN / START EXAM", new Vector2(0, -180), new Vector2(400, 60), buttonSprite);
        startButton = startBtnObj.GetComponent<Button>();
        startButton.onClick.AddListener(StartExam);

        // 2. Question Screen
        questionScreen = UIFactory.CreateUIElement(panelRT, "QuestionScreen", new Vector2(0, -30), new Vector2(800, 520));
        questionScreen.GetComponent<Image>().color = Color.clear;
        questionScreen.SetActive(false);

        progressText = UIFactory.CreateText(questionScreen.transform, "Progress", "Frage 1 von 10", new Vector2(0, 220), 20, TextAnchor.MiddleCenter);
        progressText.color = Color.black;
        progressText.fontStyle = FontStyle.Bold;

        GameObject qFrame = UIFactory.CreateUIElement(questionScreen.transform as RectTransform, "QFrame", new Vector2(0, 110), new Vector2(760, 160));
        UIFactory.SetupImage(qFrame, innerFrameSprite, false);
        qFrame.GetComponent<Image>().type = Image.Type.Sliced;
        qFrame.GetComponent<Image>().color = new Color(0.97f, 0.97f, 1f);

        questionText = UIFactory.CreateText(qFrame.transform, "QText", "Die Frage steht hier...", Vector2.zero, 20, TextAnchor.MiddleCenter);
        questionText.color = Color.black;
        questionText.fontStyle = FontStyle.Bold;
        questionText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 130);

        // Answer Buttons
        float buttonYStart = -20f;
        float buttonSpacing = 75f;
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            GameObject btnObj = UIFactory.CreateButton(questionScreen.transform as RectTransform, "AnswerBtn_" + i, "", new Vector2(0, buttonYStart - i * buttonSpacing), new Vector2(760, 60), buttonSprite);
            answerButtons[i] = btnObj.GetComponent<Button>();
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));

            answerTexts[i] = btnObj.GetComponentInChildren<Text>();
            answerTexts[i].fontSize = 18;
            answerTexts[i].alignment = TextAnchor.MiddleCenter;
            answerTexts[i].color = Color.black;
        }

        // 3. Result Screen
        resultScreen = UIFactory.CreateUIElement(panelRT, "ResultScreen", new Vector2(0, -30), new Vector2(800, 520));
        resultScreen.GetComponent<Image>().color = Color.clear;
        resultScreen.SetActive(false);

        resultTitle = UIFactory.CreateText(resultScreen.transform, "ResultTitle", "ERGEBNIS", new Vector2(0, 200), 28, TextAnchor.MiddleCenter);
        resultTitle.color = Color.black;
        resultTitle.fontStyle = FontStyle.Bold;

        GameObject resFrame = UIFactory.CreateUIElement(resultScreen.transform as RectTransform, "ResFrame", new Vector2(0, 50), new Vector2(760, 240));
        UIFactory.SetupImage(resFrame, innerFrameSprite, false);
        resFrame.GetComponent<Image>().type = Image.Type.Sliced;
        resFrame.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);

        resultText = UIFactory.CreateText(resFrame.transform, "ResultText", "", Vector2.zero, 18, TextAnchor.MiddleCenter);
        resultText.color = Color.black;
        resultText.supportRichText = true;
        resultText.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 200);

        GameObject actBtnObj = UIFactory.CreateButton(resultScreen.transform as RectTransform, "ActionButton", "OK", new Vector2(-150, -150), new Vector2(250, 60), buttonSprite);
        actionButton = actBtnObj.GetComponent<Button>();
        actionButton.onClick.AddListener(() => DialogPopOut.Trigger(panel));

        GameObject openCertBtnObj = UIFactory.CreateButton(resultScreen.transform as RectTransform, "OpenCertButton", "Zertifikat öffnen", new Vector2(150, -150), new Vector2(250, 60), buttonSprite);
        openCertButton = openCertBtnObj.GetComponent<Button>();
        openCertButton.onClick.AddListener(() => {
            panel.SetActive(false);
            if (CertificateManager.Instance != null) CertificateManager.Instance.ToggleWindow();
        });
        openCertButton.gameObject.SetActive(false);
    }

    public void ToggleWindow()
    {
        if (panel == null) return;
        bool isAct = !panel.activeSelf;
        panel.SetActive(isAct);

        if (isAct)
        {
            panel.transform.SetAsLastSibling();
            ResetToStart();
        }
    }

    private void ResetToStart()
    {
        startScreen.SetActive(true);
        questionScreen.SetActive(false);
        resultScreen.SetActive(false);

        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        panel.transform.Find("Header/Title").GetComponent<Text>().text = isDe ? "✍️ Erste-Hilfe-Prüfung" : "✍️ First Aid Exam";

        startDescription.text = isDe ?
            "<b>ERSTE-HILFE ZERTIFIZIERUNGSPRÜFUNG</b>\n\n" +
            "Stelle dein Wissen unter Beweis! Diese theoretische Prüfung besteht aus <b>10 zufälligen Fragen</b> zu allen Erste-Hilfe-Szenarien des Handbuchs.\n\n" +
            "• Um das offizielle Zertifikat freizuschalten, musst du mindestens <b>80% (8 von 10 Fragen)</b> richtig beantworten.\n" +
            "• Nimm dir Zeit, jede Frage sorgfältig zu lesen. Es gibt kein Zeitlimit.\n\n" +
            "Viel Erfolg beim Retten von virtuellen Leben!" :
            
            "<b>FIRST AID CERTIFICATION EXAM</b>\n\n" +
            "Prove your medical knowledge! This theoretical exam consists of <b>10 random questions</b> covering all first-aid scenarios in the handbook.\n\n" +
            "• To unlock the official certificate, you must answer at least <b>80% (8 out of 10 questions)</b> correctly.\n" +
            "• Take your time to read each question carefully. There is no time limit.\n\n" +
            "Good luck in saving virtual lives!";

        startButton.GetComponentInChildren<Text>().text = isDe ? "PRÜFUNG STARTEN" : "START EXAM";
    }

    private void StartExam()
    {
        startScreen.SetActive(false);
        questionScreen.SetActive(true);
        resultScreen.SetActive(false);

        currentQuestionIndex = 0;
        correctAnswersCount = 0;
        isProcessingAnswer = false;

        // Shuffle questions
        activeQuestions.Clear();
        List<ExamQuestion> shuffleList = new List<ExamQuestion>(questionPool);
        for (int i = 0; i < shuffleList.Count; i++)
        {
            ExamQuestion temp = shuffleList[i];
            int randIdx = Random.Range(i, shuffleList.Count);
            shuffleList[i] = shuffleList[randIdx];
            shuffleList[randIdx] = temp;
        }

        // Take 10 questions
        int selectCount = Mathf.Min(10, shuffleList.Count);
        for (int i = 0; i < selectCount; i++)
        {
            activeQuestions.Add(shuffleList[i]);
        }

        ShowNextQuestion();
    }

    private void ShowNextQuestion()
    {
        if (currentQuestionIndex >= activeQuestions.Count)
        {
            ShowResults();
            return;
        }

        isProcessingAnswer = false;

        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        progressText.text = isDe ?
            $"Frage {currentQuestionIndex + 1} von {activeQuestions.Count}" :
            $"Question {currentQuestionIndex + 1} of {activeQuestions.Count}";

        ExamQuestion q = activeQuestions[currentQuestionIndex];
        questionText.text = isDe ? q.questionDe : q.questionEn;

        // Shuffle answer visual buttons
        visualToOriginalIndex.Clear();
        List<int> indexList = new List<int> { 0, 1, 2 };
        for (int i = 0; i < indexList.Count; i++)
        {
            int temp = indexList[i];
            int randIdx = Random.Range(i, indexList.Count);
            indexList[i] = indexList[randIdx];
            indexList[randIdx] = temp;
        }

        visualCorrectIndex = -1;
        for (int i = 0; i < 3; i++)
        {
            int originalIdx = indexList[i];
            visualToOriginalIndex.Add(originalIdx);
            if (originalIdx == q.correctIndex) visualCorrectIndex = i;

            // Reset button visuals
            answerButtons[i].interactable = true;
            answerButtons[i].GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.75f, 0.9f); // Default retro blue

            answerTexts[i].text = isDe ? q.answersDe[originalIdx] : q.answersEn[originalIdx];
        }
    }

    private void OnAnswerClicked(int visualIndex)
    {
        if (isProcessingAnswer) return;
        StartCoroutine(ProcessAnswer(visualIndex));
    }

    private IEnumerator ProcessAnswer(int visualIndex)
    {
        isProcessingAnswer = true;

        // Disable interaction
        for (int i = 0; i < 3; i++)
        {
            answerButtons[i].interactable = false;
        }

        bool correct = (visualIndex == visualCorrectIndex);
        if (correct)
        {
            correctAnswersCount++;
            answerButtons[visualIndex].GetComponent<Image>().color = Color.green;

            // Play pleasant chime
            if (AudioManager.Instance != null && AudioManager.Instance.bootChimeSound != null)
            {
                // Play short click or boot sound pitched up
                AudioManager.Instance.PlaySFX(AudioManager.Instance.bootChimeSound, 0.4f);
            }
        }
        else
        {
            answerButtons[visualIndex].GetComponent<Image>().color = Color.red;
            answerButtons[visualCorrectIndex].GetComponent<Image>().color = Color.green;

            // Play error buzzer
            if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound, 0.6f);
            }
        }

        yield return new WaitForSecondsRealtime(1.5f);

        currentQuestionIndex++;
        ShowNextQuestion();
    }

    private void ShowResults()
    {
        startScreen.SetActive(false);
        questionScreen.SetActive(false);
        resultScreen.SetActive(true);

        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        resultTitle.text = isDe ? "PRÜFUNGSERGEBNIS" : "EXAM RESULTS";

        int total = activeQuestions.Count;
        float percent = ((float)correctAnswersCount / total) * 100f;
        bool passed = correctAnswersCount >= 8;

        if (passed)
        {
            PlayerPrefs.SetInt("ExamPassed", 1);
            PlayerPrefs.Save();

            resultText.text = isDe ?
                $"Du hast <b>{correctAnswersCount} von {total}</b> Fragen richtig beantwortet (<b>{percent:0}%</b>).\n\n" +
                "<color=green><b>HERZLICHEN GLÜCKWUNSCH! BESTANDEN!</b></color>\n\n" +
                "Du hast ein hervorragendes Verständnis für die Erste Hilfe bewiesen. " +
                "Dein persönliches Ersthelfer-Zertifikat (Zertifikat.exe) wurde freigeschaltet und kann jetzt ausgedruckt werden." :

                $"You answered <b>{correctAnswersCount} out of {total}</b> questions correctly (<b>{percent:0}%</b>).\n\n" +
                "<color=green><b>CONGRATULATIONS! PASSED!</b></color>\n\n" +
                "You have demonstrated an excellent understanding of first aid. " +
                "Your personal first responder certificate (Zertifikat.exe) has been unlocked and can now be printed.";

            actionButton.GetComponentInChildren<Text>().text = isDe ? "OK" : "OK";
            openCertButton.gameObject.SetActive(true);
            openCertButton.GetComponentInChildren<Text>().text = isDe ? "Zertifikat öffnen" : "Open Certificate";

            // Reposition actionButton to left, openCertButton to right
            actionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -160);
            openCertButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(150, -160);
        }
        else
        {
            resultText.text = isDe ?
                $"Du hast <b>{correctAnswersCount} von {total}</b> Fragen richtig beantwortet (<b>{percent:0}%</b>).\n\n" +
                "<color=red><b>LEIDER NICHT BESTANDEN.</b></color>\n\n" +
                "Du benötigst mindestens <b>80% (8 richtige Antworten)</b> zum Bestehen.\n" +
                "Bitte schaue im Handbuch (Handbuch.exe) nach, um dein Wissen aufzufrischen, und versuche es erneut." :

                $"You answered <b>{correctAnswersCount} out of {total}</b> questions correctly (<b>{percent:0}%</b>).\n\n" +
                "<color=red><b>UNFORTUNATELY FAILED.</b></color>\n\n" +
                "You need at least <b>80% (8 correct answers)</b> to pass.\n" +
                "Please consult the handbook (Handbuch.exe) to refresh your knowledge and try again.";

            actionButton.GetComponentInChildren<Text>().text = isDe ? "ERNEUT VERSUCHEN" : "RETRY";
            openCertButton.gameObject.SetActive(false);

            // Center actionButton
            actionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -160);

            // Rewire action button to restart exam directly
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(StartExam);
        }
        
        // Reset original listener for normal close when passed
        if (passed)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => DialogPopOut.Trigger(panel));
        }
    }
}
