using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BurnManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject burnPanel;
    public Slider temperatureSlider;
    public Image temperatureFill;
    public Text instructionText;
    public Slider progressSlider;
    public Text temperatureLabelText;
    
    private float currentProgress = 0f;
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
        if (type == GameManager.VictimType.BurnInjury)
        {
            Activate();
        }
    }

    public void Activate()
    {
        burnPanel.SetActive(true);
        currentProgress = 0f;
        isActive = true;
        progressSlider.value = 0;
        instructionText.text = "HALTE DIE TEMPERATUR IM BLAUEN BEREICH!";
    }

    private void Update()
    {
        if (!isActive) return;

        float val = temperatureSlider.value;
        
        // Calculate temperature in Celsius: 5°C to 40°C
        float tempCelsius = 5f + val * 35f;

        // Update the label dynamically
        if (temperatureLabelText != null)
        {
            temperatureLabelText.text = string.Format("Wasser-Temperatur regeln ({0:F1}°C)", tempCelsius);
        }
        
        // Update color based on temperature
        temperatureFill.color = Color.Lerp(Color.blue, Color.red, val);

        // Check if in "cool" zone (around 0.2 - 0.4)
        if (val > 0.15f && val < 0.45f)
        {
            currentProgress += Time.deltaTime * 0.15f;
            instructionText.text = "GUT! SO WEITER KÜHLEN.";
        }
        else if (val > 0.6f)
        {
            instructionText.text = "ZU HEISS! GEFAHR!";
        }
        else
        {
            instructionText.text = "ZU KALT! GEWEBEGEFAHR!";
        }

        progressSlider.value = currentProgress;

        if (currentProgress >= 1f)
        {
            CompleteBurn();
        }
    }

    private void CompleteBurn()
    {
        isActive = false;
        instructionText.text = "VERBRENNUNG ERSTVERSORGT!";
        StartCoroutine(FinishSequence());
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(2f);
        burnPanel.SetActive(false);
        GameEvents.OnMinigameCompleted?.Invoke(GameManager.VictimType.BurnInjury);
    }
}
