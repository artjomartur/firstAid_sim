using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the structured Career Mode, including specific accidents per day.
/// </summary>
public class CareerManager : MonoBehaviour
{
    public static CareerManager Instance { get; private set; }
    
    public bool isCareerMode = false; // Career mode is disabled by default so users can free-roam
    public int currentDay = 1;
    
    private GameManager gameManager;
    private Coroutine dayCoroutine;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        currentDay = PlayerPrefs.GetInt("Career_CurrentDay", 1);
    }
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }
    
    public void StartDay()
    {
        if (dayCoroutine != null) StopCoroutine(dayCoroutine);
        dayCoroutine = StartCoroutine(DayRoutine());
    }
    
    public void StopDay()
    {
        if (dayCoroutine != null) StopCoroutine(dayCoroutine);
    }
    
    private IEnumerator DayRoutine()
    {
        yield return new WaitForSeconds(5f);
        
        if (currentDay == 1)
        {
            gameManager.TriggerAccident(GameManager.VictimType.BleedingWound);
            yield return WaitUntilAllMissionsDone();
            gameManager.TriggerAccident(GameManager.VictimType.BurnInjury);
            yield return WaitUntilAllMissionsDone();
            CompleteDay();
        }
        else if (currentDay == 2)
        {
            gameManager.TriggerAccident(GameManager.VictimType.BikeAccident);
            yield return WaitUntilAllMissionsDone();
            gameManager.TriggerAccident(GameManager.VictimType.UnconsciousPerson);
            yield return WaitUntilAllMissionsDone();
            CompleteDay();
        }
        else if (currentDay == 3)
        {
            gameManager.TriggerAccident(GameManager.VictimType.Choking);
            yield return WaitUntilAllMissionsDone();
            gameManager.TriggerAccident(GameManager.VictimType.BoneFracture);
            yield return WaitUntilAllMissionsDone();
            CompleteDay();
        }
        else
        {
            // Beyond predefined days, fallback to random spawner
            gameManager.StartRandomMissionSpawner();
        }
    }
    
    private IEnumerator WaitUntilAllMissionsDone()
    {
        // Wait 10 seconds before next accident minimum, or wait until active victims is 0
        yield return new WaitForSeconds(10f);
        while (HasActiveMissions())
        {
            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(5f);
    }
    
    private bool HasActiveMissions()
    {
        if (gameManager == null) return false;
        foreach (var m in gameManager.availableMissions)
        {
            if (m.activeInstance != null) return true;
        }
        return false;
    }
    
    public void CompleteDay()
    {
        currentDay++;
        PlayerPrefs.SetInt("Career_CurrentDay", currentDay);
        PlayerPrefs.Save();
        
        // Show success, then return to menu
        if (gameManager.sfxSource != null && AudioManager.Instance != null && AudioManager.Instance.missionSuccessSound != null)
        {
            gameManager.sfxSource.PlayOneShot(AudioManager.Instance.missionSuccessSound);
        }
        
        MenuManager menu = FindFirstObjectByType<MenuManager>();
        if (menu != null)
        {
            // Call ReturnToMenu after a delay
            Invoke("TriggerReturn", 3f);
        }
    }
    
    private void TriggerReturn()
    {
        if (gameManager != null) gameManager.StartMenuPhase();
    }
    
    public void ResetCareer()
    {
        currentDay = 1;
        PlayerPrefs.SetInt("Career_CurrentDay", 1);
        PlayerPrefs.Save();
    }
}
