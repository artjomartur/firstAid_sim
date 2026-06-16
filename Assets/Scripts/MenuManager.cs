using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameManager gameManager;
    
    public GameObject mainView;
    public GameObject settingsView;
    public GameObject creditsView;
    public GameObject missionsView;
    public GameObject futureView; // Sub-view of creditsView

    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle nightModeToggle; // Old toggle
    
    // New Dropdown for UI
    public UnityEngine.UI.Dropdown timeModeDropdown;

    public void Initialize()
    {
        ShowMainView();
        
        if (bgmSlider != null)
        {
            bgmSlider.value = gameManager.bgmSource != null ? gameManager.bgmSource.volume : 0.5f;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.value = gameManager.sfxSource != null ? gameManager.sfxSource.volume : 0.8f;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (nightModeToggle != null)
        {
            DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
            if (dnc != null)
            {
                nightModeToggle.isOn = dnc.isTimePassing;
            }
            nightModeToggle.onValueChanged.AddListener(SetNightMode);
        }

        if (timeModeDropdown != null)
        {
            DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
            if (dnc != null)
            {
                timeModeDropdown.value = (int)dnc.timeMode;
            }
            timeModeDropdown.onValueChanged.AddListener(SetTimeMode);
        }
    }

    public void ShowMainView()
    {
        if (mainView != null) mainView.SetActive(true);
        if (settingsView != null) settingsView.SetActive(false);
        if (creditsView != null) creditsView.SetActive(false);
        if (missionsView != null) missionsView.SetActive(false);
        if (futureView != null) futureView.SetActive(false);
    }

    public void ShowSettingsView()
    {
        if (mainView != null) mainView.SetActive(false);
        if (settingsView != null) settingsView.SetActive(true);
        if (creditsView != null) creditsView.SetActive(false);
        if (missionsView != null) missionsView.SetActive(false);
    }

    public void ShowCreditsView()
    {
        if (mainView != null) mainView.SetActive(false);
        if (settingsView != null) settingsView.SetActive(false);
        if (creditsView != null) creditsView.SetActive(true);
        if (missionsView != null) missionsView.SetActive(false);
        if (futureView != null) futureView.SetActive(false);
    }

    public void ShowFutureView()
    {
        if (creditsView != null) creditsView.SetActive(false);
        if (futureView != null) futureView.SetActive(true);
    }

    public void ShowCreditsFromFuture()
    {
        if (futureView != null) futureView.SetActive(false);
        if (creditsView != null) creditsView.SetActive(true);
    }

    public void ShowMissionsView()
    {
        if (mainView != null) mainView.SetActive(false);
        if (settingsView != null) settingsView.SetActive(false);
        if (creditsView != null) creditsView.SetActive(false);
        if (missionsView != null) missionsView.SetActive(true);

        RefreshMissionsUI();
    }

    public void RefreshMissionsUI()
    {
        if (gameManager == null) return;
        gameManager.SyncMissionProgress();

        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        string statusCompleted = isDe ? "[Abgeschlossen]" : "[Completed]";
        string statusNew = isDe ? "[Neu]" : "[New]";

        foreach (var mission in gameManager.availableMissions)
        {
            if (missionsView != null)
            {
                Transform content = missionsView.transform.Find("ScrollArea/Viewport/Content");
                Transform row = content != null ? content.Find("Mission_" + mission.id) : null;
                if (row != null)
                {
                    // Localize mission name dynamically using its id (id + "_title")
                    Transform nameTrans = row.Find("Name");
                    if (nameTrans != null && LocalizationManager.Instance != null)
                    {
                        Text nameTxt = nameTrans.GetComponent<Text>();
                        if (nameTxt != null)
                        {
                            nameTxt.text = LocalizationManager.Instance.Get(mission.id + "_title");
                        }
                    }

                    Transform statusTrans = row.Find("Status");
                    if (statusTrans != null)
                    {
                        Text txt = statusTrans.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.text = mission.hasPlayed ? statusCompleted : statusNew;
                            txt.color = mission.hasPlayed ? new Color(0, 0.5f, 0) : Color.red;
                        }
                    }
                }
            }
        }
    }

    public void SetBGMVolume(float v)
    {
        if (gameManager != null && gameManager.bgmSource != null)
        {
            gameManager.bgmSource.volume = v;
        }
    }

    public void SetSFXVolume(float v)
    {
        if (gameManager != null && gameManager.sfxSource != null)
        {
            gameManager.sfxSource.volume = v;
            // Also adjust heartbeat as it's an SFX
            if (gameManager.heartbeatSource != null)
            {
                gameManager.heartbeatSource.volume = v;
            }
        }
    }

    public void SetNightMode(bool isOn)
    {
        DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
        if (dnc != null)
        {
            dnc.isTimePassing = isOn;
            if (!isOn)
            {
                dnc.timeOfDay = 0f; // Reset to day
            }
        }
    }

    public void SetTimeMode(int modeIndex)
    {
        DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
        if (dnc != null)
        {
            dnc.timeMode = (DayNightCycle.TimeMode)modeIndex;
            
            // Force immediate update
            if (modeIndex == 1) dnc.timeOfDay = 0f; // Day
            else if (modeIndex == 2) dnc.timeOfDay = 0.5f; // Night
        }
    }

    public void ToggleDayNightMode()
    {
        DayNightCycle dnc = FindObjectOfType<DayNightCycle>();
        if (dnc == null) return;

        switch (dnc.timeMode)
        {
            case DayNightCycle.TimeMode.Cycle:
                dnc.timeMode   = DayNightCycle.TimeMode.AlwaysDay;
                dnc.timeOfDay  = 0f;
                break;
            case DayNightCycle.TimeMode.AlwaysDay:
                dnc.timeMode   = DayNightCycle.TimeMode.AlwaysNight;
                dnc.timeOfDay  = 0.5f;
                break;
            default: // AlwaysNight
                dnc.timeMode   = DayNightCycle.TimeMode.Cycle;
                break;
        }
    }

    public void ResetProgress()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScores();
        }
        gameManager.bikeAccidentHelped = false;
        gameManager.bleedingWoundHelped = false;
        gameManager.unconsciousHelped = false;
        gameManager.burnInjuryHelped = false;
        gameManager.chokingHelped = false;
        gameManager.heatstrokeHelped = false;
        gameManager.triageHelped = false;
        gameManager.electricShockHelped = false;
        gameManager.poisoningHelped = false;
        
        RefreshMissionsUI();
        Debug.Log("Spielfortschritt zurückgesetzt!");
    }
}
