using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElectricShockManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject shockPanel;
    public Text instructionText;
    
    [Header("Fuse Box UI")]
    public Button[] fuseButtons;
    private bool[] fuseStates;
    private int fusesDisabled = 0;
    
    private bool gameActive = false;

    
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
        if (type == GameManager.VictimType.ElectricShock)
        {
            Activate();
        }
    }

    public void Activate()
    {
        shockPanel.SetActive(true);
        gameActive = true;
        fusesDisabled = 0;
        
        instructionText.text = "Gefahr! Stromschlag! Schalte zuerst den Strom ab, bevor du das Opfer berührst!";
        
        fuseStates = new bool[fuseButtons.Length];
        
        for (int i = 0; i < fuseButtons.Length; i++)
        {
            int index = i; // local copy for closure
            fuseStates[i] = true; // true = On (Red/Dangerous), false = Off (Green/Safe)
            
            // Set initial color (Red)
            Image btnImg = fuseButtons[i].GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.8f, 0.1f, 0.1f);
            
            Text btnText = fuseButtons[i].GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = "I"; // On symbol
            
            fuseButtons[i].onClick.RemoveAllListeners();
            fuseButtons[i].onClick.AddListener(() => OnFuseClicked(index));
            fuseButtons[i].gameObject.SetActive(true);
        }
    }

    private void OnFuseClicked(int index)
    {
        if (!gameActive || !fuseStates[index]) return; // Already disabled
        
        fuseStates[index] = false;
        fusesDisabled++;
        
        ScoreManager.Instance?.RecordSuccess();

        // Change visual to safe (Green)
        Image btnImg = fuseButtons[index].GetComponent<Image>();
        if (btnImg != null) btnImg.color = new Color(0.1f, 0.8f, 0.1f);
        
        Text btnText = fuseButtons[index].GetComponentInChildren<Text>();
        if (btnText != null) btnText.text = "O"; // Off symbol
        
        CameraShake.Shake(0.1f, 0.1f); // Small shake for impact
        
        if (fusesDisabled >= fuseButtons.Length)
        {
            OnAllFusesDisabled();
        }
    }

    private void OnAllFusesDisabled()
    {
        gameActive = false;
        instructionText.text = "Strom abgestellt! Eigenschutz ist jetzt gesichert. Du kannst nun Erste Hilfe leisten.";
        StartCoroutine(FinishLevel());
    }

    private IEnumerator FinishLevel()
    {
        yield return new WaitForSeconds(2.5f);
        foreach (Button btn in fuseButtons)
        {
            btn.gameObject.SetActive(false);
        }
        
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
            
        GameEvents.OnMinigameCompleted?.Invoke(GameManager.VictimType.ElectricShock);
    }
}
