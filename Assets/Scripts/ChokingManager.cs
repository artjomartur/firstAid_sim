using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChokingManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject chokingPanel;
    public Slider forceSlider;
    public Text instructionText;
    public Image fillImage;
    
    private float currentForce = 0f;
    private int successes = 0;
    private int required = 5;
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
        if (type == GameManager.VictimType.Choking)
        {
            Activate();
        }
    }

    public void Activate()
    {
        chokingPanel.SetActive(true);
        successes = 0;
        currentForce = 0f;
        isActive = true;
        instructionText.text = "DRÜCKE LEERTASTE WENN DER BALKEN IM GRÜNEN IST!";
    }

    private void Update()
    {
        if (!isActive) return;

        // Oscillate force
        currentForce = Mathf.PingPong(Time.time * 2f, 1f);
        forceSlider.value = currentForce;

        // Color logic
        if (currentForce > 0.7f && currentForce < 0.9f) fillImage.color = Color.green;
        else fillImage.color = Color.red;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentForce > 0.7f && currentForce < 0.9f)
            {
                successes++;
                instructionText.text = "GUT! " + successes + "/" + required;
                if (successes >= required) CompleteChoking();
            }
            else
            {
                instructionText.text = "ZU SCHWACH ODER ZU STARK!";
            }
        }
    }

    private void CompleteChoking()
    {
        isActive = false;
        instructionText.text = "FREMDKÖRPER GELÖST!";
        StartCoroutine(FinishSequence());
    }

    private IEnumerator FinishSequence()
    {
        yield return new WaitForSeconds(2f);
        chokingPanel.SetActive(false);
        GameEvents.OnMinigameCompleted?.Invoke(GameManager.VictimType.Choking);
    }
}
