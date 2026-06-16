using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Steuert das Rhythmus-basierte CPR-Minispiel.
/// </summary>
public class CPRManager : MonoBehaviour
{
    public static CPRManager Instance { get; private set; }

    [Header("UI Referenzen")]
    public GameObject cprPanel;
    public Slider rhythmSlider;   // Der oszillierende Balken
    public Slider successSlider;  // Der Fortschrittsbalken oben
    public Text feedbackText;
    public Image handsImage; 

    [Header("Rhythmus Einstellungen")]
    public float targetBPM = 110f; 
    public float windowSize = 0.25f; // Erhöhtes Zeitfenster (einfacher)
    public int requiredCompressions = 20; // Weniger Wiederholungen nötig

    private float timer = 0f;
    private float beatInterval;
    private int successfulCompressions = 0;
    private bool isActive = false;
    private Vector3 originalHandsScale;

    private void Awake()
    {
        Instance = this;
    }

    public void SlowDownRhythm()
    {
        targetBPM *= 0.6f;
        beatInterval = 60f / targetBPM;
        Debug.Log("CPR rhythm slowed down via cheat. New BPM: " + targetBPM);
    }

    private void Start()
    {
        // Power-up check
        ShopManager sm = FindObjectOfType<ShopManager>();
        if (sm != null && sm.steadyRhythmActive)
        {
            targetBPM *= 0.7f; // 30% slower
            Debug.Log("Steady Rhythm active: BPM reduced to " + targetBPM);
        }

        beatInterval = 60f / targetBPM;
        
        if (handsImage != null)
        {
            originalHandsScale = handsImage.transform.localScale;
        }

        if (successSlider != null)
        {
            successSlider.minValue = 0;
            successSlider.maxValue = requiredCompressions;
            successSlider.value = 0;
        }
    }

    /// <summary>
    /// Startet das CPR-Spiel.
    /// </summary>
    public void Activate()
    {
        if (cprPanel != null) cprPanel.SetActive(true);
        
        isActive = true;
        successfulCompressions = 0;
        timer = 0f;
        
        if (successSlider != null) successSlider.value = 0;
        
        UpdateFeedback("DRÜCKE LEERTASTE IM RHYTHMUS!", Color.white);
    }

    private void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        
        // Berechne den Fortschritt innerhalb eines "Beats" (0.0 bis 1.0)
        float beatProgress = (timer % beatInterval) / beatInterval;
        
        if (rhythmSlider != null)
        {
            rhythmSlider.value = beatProgress;
        }

        // Subtile Animation der Hände im Takt
        AnimateHandsPulse(beatProgress);

        // Eingabe-Check
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckRhythm(beatProgress);
        }
    }

    private void AnimateHandsPulse(float progress)
    {
        if (handsImage != null)
        {
            float pulse = 1.0f + Mathf.Sin(progress * Mathf.PI * 2) * 0.05f;
            handsImage.transform.localScale = originalHandsScale * pulse;
        }
    }

    private void CheckRhythm(float beatProgress)
    {
        // Wir zielen auf die Mitte des Sliders (0.5)
        float distanceToTarget = Mathf.Abs(beatProgress - 0.5f);
        
        if (distanceToTarget < windowSize)
        {
            HandleSuccessfulCompression();
        }
        else
        {
            ScoreManager.Instance?.RecordError();
            UpdateFeedback("FALSCHER TAKT!", Color.red);
        }
    }

    private void HandleSuccessfulCompression()
    {
        successfulCompressions++;
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.cprScore++;
            ScoreManager.Instance.RecordSuccess();
        }

        if (successSlider != null)
        {
            successSlider.value = successfulCompressions;
        }

        UpdateFeedback("GUT! " + successfulCompressions + "/" + requiredCompressions, Color.green);
        
        StartCoroutine(PunchAnimationEffect());

        if (successfulCompressions >= requiredCompressions)
        {
            CompleteCPR();
        }
    }

    private IEnumerator PunchAnimationEffect()
    {
        if (handsImage != null)
        {
            handsImage.transform.localScale = originalHandsScale * 0.85f;
            yield return new WaitForSeconds(0.05f);
            handsImage.transform.localScale = originalHandsScale;
        }
    }

    private void UpdateFeedback(string msg, Color col)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = col;
        }
    }

    private void CompleteCPR()
    {
        isActive = false;
        
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.OnCPRCompleted();
        }
    }
}
