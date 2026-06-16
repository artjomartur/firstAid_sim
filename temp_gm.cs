using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GamePhase { Menu, Story, Call, Intro, Quiz, Bandage, CPR, AED, Burn, Choking, Heatstroke, Triage, Stabilize, Result, Shop, ElectricShock, Poisoning, EmergencyKit }
    public enum VictimType { BikeAccident, BleedingWound, UnconsciousPerson, BurnInjury, Choking, Heatstroke, TriageScene, ElectricShock, Poisoning, ParkCleanup }
    
    public GamePhase currentPhase = GamePhase.Menu;

    [System.Serializable]
    public class FirstAidMission
    {
        public string id;
        public string title;
        public VictimType type;
        public Color color;
        public bool isIndoor;
        public bool hasPlayed;
        public GameObject activeInstance;
        public System.Action<Vector2> spawnAction;
    }
    
    public System.Collections.Generic.List<FirstAidMission> availableMissions = new System.Collections.Generic.List<FirstAidMission>();
    public Transform[] outdoorSpawnPoints;
    public Transform[] indoorSpawnPoints;

    public bool bikeAccidentHelped = false;
    public bool bleedingWoundHelped = false;
    public bool unconsciousHelped = false;
    public bool burnInjuryHelped = false;
    public bool chokingHelped = false;
    public bool heatstrokeHelped = false;
    public bool triageHelped = false;
    public bool electricShockHelped = false;
    public bool poisoningHelped = false;
    public bool parkCleanupHelped = false;
    
    [Header("Inventory")]
    public bool hasMedkit = false;

    [Header("Retro UI Sprites")]
    public Sprite winBaseSprite;
    public Sprite winHeaderSprite;
    public Sprite winButtonSprite;
    public Sprite winInnerFrameSprite;

    public bool loadedFromMenu = false;

    public QuizManager quizManager;
    public BandageManager bandageManager;
    public CPRManager cprManager;
    public StabilizeManager stabilizeManager;
    public AmbulanceManager ambulanceManager;
    public GameObject menuPanel;
    public GameObject storyPanel;
    public GameObject callPanel;
    public EmergencyCallManager callManager;
    public GameObject introPanel;
    public GameObject quizPanel;
    public GameObject bandagePanel;
    public GameObject burnPanel;
    public BurnManager burnManager;
    public GameObject chokingPanel;
    public ChokingManager chokingManager;
    public GameObject heatstrokePanel;
    public HeatstrokeManager heatstrokeManager;
    public GameObject triagePanel;
    public TriageManager triageManager;
    public GameObject shockPanel;
    public ElectricShockManager shockManager;
    public GameObject poisonPanel;
    public PoisonManager poisonManager;
    public GameObject cprPanel;
    public GameObject aedPanel;
    public AEDManager aedManager;
    public GameObject stabilizePanel;
    public GameObject resultPanel;
    public GameObject shopPanel;
    public GameObject pausePanel;
    public GameObject monitorPanel;
    public MonitorManager monitorManager;
    public GameObject emergencyKitPanel;
    public EmergencyKitManager emergencyKitManager;
    public Text resultDetailsText;
    
    public Text startButtonText;
    public GamePhase phaseBeforeMenu = GamePhase.Intro;
    public bool gameStarted = false;
    private float originalOrthographicSize = 5.5f;
    private bool orthoSizeInitialized = false;

    private void InitializeOrthoSize()
    {
        if (!orthoSizeInitialized && Camera.main != null)
        {
            originalOrthographicSize = Camera.main.orthographicSize;
            orthoSizeInitialized = true;
        }
    }
    public AudioSource bgmSource;
    public AudioSource heartbeatSource;
    public AudioSource sfxSource; // For the crash sound
    public AudioClip pauseSound;
    public AudioClip unpauseSound;
    public AudioClip monitorOpenSound;
    public AudioClip monitorCloseSound;

    [Header("Music Clips")]
    public AudioClip mapMusic;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPhase == GamePhase.Menu)
            {
                if (gameStarted)
                {
                    ResumeOrStartGame();
                }
            }
            else
            {
                StartMenuPhase();
            }
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            if (currentPhase == GamePhase.Intro && monitorManager != null)
            {
                monitorManager.ToggleMonitor();
            }
        }

        UpdateHeartbeatDynamics();
    }

    private void UpdateHeartbeatDynamics()
    {
        if (heartbeatSource == null || !heartbeatSource.isPlaying) return;

        float targetPitch = 1.0f;
        float targetVolume = 0.5f;

        switch (currentPhase)
        {
            case GamePhase.Intro:
            case GamePhase.Shop:
                targetPitch = 0.8f;
                targetVolume = 0.35f;
                break;
            case GamePhase.Quiz:
            case GamePhase.Triage:
            case GamePhase.Stabilize:
            case GamePhase.Bandage:
                targetPitch = 1.2f;
                targetVolume = 0.75f;
                break;
            case GamePhase.CPR:
            case GamePhase.AED:
            case GamePhase.Choking:
            case GamePhase.Burn:
            case GamePhase.Heatstroke:
            case GamePhase.ElectricShock:
            case GamePhase.Poisoning:
                targetPitch = 1.5f;
                targetVolume = 1.0f;
                break;
            default:
                targetVolume = 0f;
                break;
        }

        // Smooth transition
        heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, targetPitch, Time.deltaTime * 2f);
        heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, targetVolume, Time.deltaTime * 2f);
    }

    public void TogglePause()
    {
        bool isPaused = !pausePanel.activeSelf;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (isPaused) 
        {
            bgmSource.Pause();
            if (pauseSound != null && sfxSource != null) sfxSource.PlayOneShot(pauseSound);
        }
        else 
        {
            bgmSource.UnPause();
            if (unpauseSound != null && sfxSource != null) sfxSource.PlayOneShot(unpauseSound);
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        bgmSource.UnPause();
        if (unpauseSound != null && sfxSource != null) sfxSource.PlayOneShot(unpauseSound);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public Transform menuCameraTarget;

    public void StartMenuPhase()
    {
        if (currentPhase != GamePhase.Menu)
        {
            phaseBeforeMenu = currentPhase;
        }

        if (menuPanel != null) menuPanel.SetActive(true);
        if (storyPanel != null) storyPanel.SetActive(false);
        if (introPanel != null) introPanel.SetActive(false);
        currentPhase = GamePhase.Menu;
        ChangeMusic(mapMusic);
        Time.timeScale = 0f;

        InitializeOrthoSize();
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = originalOrthographicSize * 1.25f; // Zoom out background slightly
        }

        CleanSkyBackgrounds();

        if (menuCameraTarget == null)
        {
            menuCameraTarget = new GameObject("MenuCameraTarget").transform;
            menuCameraTarget.position = new Vector3(106.66f, 3.63f, 0f);
        }

        CameraFollow cf = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cf != null) cf.target = menuCameraTarget;

        Cainos.PixelArtTopDown_Basic.CameraFollow ccf = Camera.main != null ? Camera.main.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>() : null;
        if (ccf != null) ccf.target = menuCameraTarget;

        UpdateMenuStartButtonText();
    }

    public void StartStoryPhase()
    {
        gameStarted = true;
        if (menuPanel != null) menuPanel.SetActive(false);
        if (storyPanel != null) storyPanel.SetActive(true);
        currentPhase = GamePhase.Story;
        ChangeMusic(null);
        Time.timeScale = 1f;

        InitializeOrthoSize();
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = originalOrthographicSize; // Restore normal zoom
        }



        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CameraFollow cf = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
            if (cf != null) cf.target = player.transform;

            Cainos.PixelArtTopDown_Basic.CameraFollow ccf = Camera.main != null ? Camera.main.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>() : null;
            if (ccf != null) ccf.target = player.transform;
        }
    }

    public void ResumeOrStartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartStoryPhase();
        }
        else
        {
            ResumeFromMenu();
        }
    }

    public void ResumeFromMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        
        currentPhase = phaseBeforeMenu;
        Time.timeScale = 1f;

        InitializeOrthoSize();
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = originalOrthographicSize; // Restore normal zoom
        }



        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CameraFollow cf = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
            if (cf != null) cf.target = player.transform;

            Cainos.PixelArtTopDown_Basic.CameraFollow ccf = Camera.main != null ? Camera.main.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>() : null;
            if (ccf != null) ccf.target = player.transform;
        }

        RestoreMusicForPhase(currentPhase);
    }

    private void RestoreMusicForPhase(GamePhase phase)
    {
        if (bgmSource != null)
        {
            if (phase == GamePhase.Intro || phase == GamePhase.Menu)
            {
                ChangeMusic(mapMusic);
            }
            else
            {
                ChangeMusic(null);
            }
        }
    }

    public void UpdateMenuStartButtonText()
    {
        if (startButtonText != null)
        {
            startButtonText.text = gameStarted ? "ZURÜCK ZUM SPIEL" : "SPIEL STARTEN";
        }
    }

    public void StartMissionDirectly(string missionId)
    {
        loadedFromMenu = true;
        FirstAidMission mission = availableMissions.Find(m => m.id == missionId);
        if (mission == null) return;

        // Clean up any menu UI
        if (menuPanel != null) menuPanel.SetActive(false);
        gameStarted = true;
        Time.timeScale = 1f;

        // Reset camera orthographic size to normal zoom
        InitializeOrthoSize();
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = originalOrthographicSize;
        }

        // Set camera target to player so it's focused correctly
        CameraFollow cf = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cf != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) cf.target = player.transform;
        }
        Cainos.PixelArtTopDown_Basic.CameraFollow ccf = Camera.main != null ? Camera.main.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>() : null;
        if (ccf != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) ccf.target = player.transform;
        }

        // Hide sky backgrounds during mission


        // Spawn the mission victim at its spawn point
        StartMission(missionId);

        // Trigger the minigame directly!
        currentPhase = GamePhase.Intro; // Temporarily set to Intro so TriggerAccident doesn't return early
        TriggerAccident(mission.type);
    }

    public void CleanSkyBackgrounds()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (var go in allObjects)
        {
            if (go.name.StartsWith("demo01_PixelSky_layer"))
            {
                if (go.name.Contains("(1)"))
                {
                    go.SetActive(false); // Hide manual duplicates
                }
                else
                {
                    go.SetActive(true); // Keep originals active always
                }
            }
        }
    }

    public void SyncMissionProgress()
    {
        foreach (var m in availableMissions)
        {
            switch (m.type)
            {
                case VictimType.BikeAccident:
                    m.hasPlayed = bikeAccidentHelped;
                    break;
                case VictimType.BleedingWound:
                    m.hasPlayed = bleedingWoundHelped;
                    break;
                case VictimType.UnconsciousPerson:
                    m.hasPlayed = unconsciousHelped;
                    break;
                case VictimType.BurnInjury:
                    m.hasPlayed = burnInjuryHelped;
                    break;
                case VictimType.Choking:
                    m.hasPlayed = chokingHelped;
                    break;
                case VictimType.Heatstroke:
                    m.hasPlayed = heatstrokeHelped;
                    break;
                case VictimType.TriageScene:
                    m.hasPlayed = triageHelped;
                    break;
                case VictimType.ElectricShock:
                    m.hasPlayed = electricShockHelped;
                    break;
                case VictimType.Poisoning:
                    m.hasPlayed = poisoningHelped;
                    break;
                case VictimType.ParkCleanup:
                    m.hasPlayed = parkCleanupHelped;
                    break;
            }
        }
    }

    public void StartCallPhase()
    {
        if (storyPanel != null) storyPanel.SetActive(false);
        if (callPanel != null) callPanel.SetActive(true);
        currentPhase = GamePhase.Call;
        ChangeMusic(null);
        if (callManager != null) callManager.Activate();
    }

    public void CleanUpActiveInstances()
    {
        foreach (var m in availableMissions)
        {
            bool helped = false;
            switch (m.type)
            {
                case VictimType.BikeAccident: helped = bikeAccidentHelped; break;
                case VictimType.BleedingWound: helped = bleedingWoundHelped; break;
                case VictimType.UnconsciousPerson: helped = unconsciousHelped; break;
                case VictimType.BurnInjury: helped = burnInjuryHelped; break;
                case VictimType.Choking: helped = chokingHelped; break;
                case VictimType.Heatstroke: helped = heatstrokeHelped; break;
                case VictimType.TriageScene: helped = triageHelped; break;
                case VictimType.ElectricShock: helped = electricShockHelped; break;
                case VictimType.Poisoning: helped = poisoningHelped; break;
            }

            if (helped)
            {
                m.hasPlayed = true;
                if (m.activeInstance != null)
                {
                    Destroy(m.activeInstance);
                    m.activeInstance = null;
                }
            }
        }
    }

    public void StartIntroPhase()
    {
        CleanUpActiveInstances();

        // Fix #8: Null out stale Transform references in GameBootstrap after cleanup
        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (bootstrap != null) bootstrap.ResetVictimTransforms();

        if (menuPanel != null) menuPanel.SetActive(false);
        if (storyPanel != null) storyPanel.SetActive(false);
        if (introPanel != null) introPanel.SetActive(true);
        currentPhase = GamePhase.Intro;
        ChangeMusic(mapMusic);
        if (heartbeatSource != null && !heartbeatSource.isPlaying) heartbeatSource.Play();

        // Spawn a random mission immediately if none is currently active
        SpawnRandomMissionIfNeeded();
    }

    public void TriggerAccident(VictimType type)
    {
        if (currentPhase != GamePhase.Intro) return; // Only trigger if in world
        
        if (sfxSource != null) sfxSource.Play(); // Play crash sound
        
        // Screen Shake
        GameBootstrap bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (bootstrap != null) bootstrap.ShakeScreen(0.5f, 20f);
        
        if (type == VictimType.BikeAccident)
        {
            currentPhase = GamePhase.Quiz;
            StartCoroutine(TransitionToQuiz());
        }
        else if (type == VictimType.BleedingWound)
        {
            currentPhase = GamePhase.Bandage;
            StartBandagePhase();
        }
        else if (type == VictimType.UnconsciousPerson)
        {
            currentPhase = GamePhase.Stabilize;
            StartStabilizePhase();
        }
        else if (type == VictimType.BurnInjury)
        {
            currentPhase = GamePhase.Burn;
            StartCoroutine(TransitionToBurn());
        }
        else if (type == VictimType.Choking)
        {
            currentPhase = GamePhase.Choking;
            StartCoroutine(TransitionToChoking());
        }
        else if (type == VictimType.Heatstroke)
        {
            currentPhase = GamePhase.Heatstroke;
            StartCoroutine(TransitionToHeatstroke());
        }
        else if (type == VictimType.TriageScene)
        {
            currentPhase = GamePhase.Triage;
            StartCoroutine(TransitionToTriage());
        }
        else if (type == VictimType.ElectricShock)
        {
            currentPhase = GamePhase.ElectricShock;
            StartCoroutine(TransitionToElectricShock());
        }
        else if (type == VictimType.Poisoning)
        {
            currentPhase = GamePhase.Poisoning;
            StartCoroutine(TransitionToPoisoning());
        }
    }

    IEnumerator TransitionToPoisoning()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartPoisonPhase();
    }

    IEnumerator TransitionToElectricShock()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartElectricShockPhase();
    }

    IEnumerator TransitionToTriage()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartTriagePhase();
    }

    IEnumerator TransitionToHeatstroke()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartHeatstrokePhase();
    }

    IEnumerator TransitionToChoking()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartChokingPhase();
    }

    IEnumerator TransitionToBurn()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartBurnPhase();
    }

    public void StartStabilizePhase()
    {
        currentPhase = GamePhase.Stabilize;
        if (stabilizePanel != null) stabilizePanel.SetActive(true);
        if (stabilizeManager != null) stabilizeManager.Activate();
    }

    public void OnStabilizeCompleted()
    {
        if (stabilizePanel != null) stabilizePanel.SetActive(false);
        unconsciousHelped = true;
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Bewusstlose Person", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
            {
                ambulanceManager.gameObject.SetActive(true);
                ambulanceManager.SpawnAmbulance(new Vector3(0, 0, 0), CheckAllHelped);
            }
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartBurnPhase()
    {
        currentPhase = GamePhase.Burn;
        if (burnPanel != null) burnPanel.SetActive(true);
        if (burnManager != null) burnManager.Activate();
    }

    public void OnBurnCompleted()
    {
        if (burnPanel != null) burnPanel.SetActive(false);
        burnInjuryHelped = true;
        CameraShake.Shake(0.3f, 0.2f);
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Verbrennung", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
                ambulanceManager.SpawnAmbulance(new Vector3(-8, -4, 0), CheckAllHelped);
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartChokingPhase()
    {
        currentPhase = GamePhase.Choking;
        if (chokingPanel != null) chokingPanel.SetActive(true);
        if (chokingManager != null) chokingManager.Activate();
    }

    public void OnChokingCompleted()
    {
        if (chokingPanel != null) chokingPanel.SetActive(false);
        chokingHelped = true;
        CameraShake.Shake(0.3f, 0.2f);
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Verschlucken", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
                ambulanceManager.SpawnAmbulance(new Vector3(10, 8, 0), CheckAllHelped);
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartHeatstrokePhase()
    {
        currentPhase = GamePhase.Heatstroke;
        if (heatstrokePanel != null) heatstrokePanel.SetActive(true);
        if (heatstrokeManager != null) heatstrokeManager.Activate();
    }

    public void OnHeatstrokeCompleted()
    {
        if (heatstrokePanel != null) heatstrokePanel.SetActive(false);
        heatstrokeHelped = true;
        CameraShake.Shake(0.3f, 0.2f);
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Hitzeschlag", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
                ambulanceManager.SpawnAmbulance(new Vector3(10, -5, 0), CheckAllHelped);
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartTriagePhase()
    {
        currentPhase = GamePhase.Triage;
        if (triagePanel != null) triagePanel.SetActive(true);
        if (triageManager != null) triageManager.Activate();
    }

    public void OnTriageCompleted()
    {
        if (triagePanel != null) triagePanel.SetActive(false);
        triageHelped = true;
        CameraShake.Shake(0.3f, 0.2f);
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Massenunfall (Triage)", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
                ambulanceManager.SpawnAmbulance(new Vector3(2, 12, 0), CheckAllHelped);
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartElectricShockPhase()
    {
        currentPhase = GamePhase.ElectricShock;
        if (shockPanel != null) shockPanel.SetActive(true);
        if (shockManager != null) shockManager.Activate();
    }

    public void OnElectricShockCompleted()
    {
        if (shockPanel != null) shockPanel.SetActive(false);
        electricShockHelped = true;
        CameraShake.Shake(0.3f, 0.2f);
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Stromschlag", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
                ambulanceManager.SpawnAmbulance(new Vector3(100, 100, 0), CheckAllHelped);
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartPoisonPhase()
    {
        currentPhase = GamePhase.Poisoning;
        if (poisonPanel != null) poisonPanel.SetActive(true);
        if (poisonManager != null) poisonManager.Activate();
    }

    public void OnPoisonCompleted()
    {
        if (poisonPanel != null) poisonPanel.SetActive(false);
        poisoningHelped = true;
        CameraShake.Shake(0.3f, 0.2f);
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Vergiftung", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
                ambulanceManager.SpawnAmbulance(new Vector3(-50, -50, 0), CheckAllHelped);
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    IEnumerator TransitionToQuiz()
    {
        yield return new WaitForSeconds(1f);
        if (introPanel != null) introPanel.SetActive(false);
        StartQuizPhase();
    }

    public void StartQuizPhase()
    {
        quizManager.gameObject.SetActive(true);
        // Start stressful audio
        ChangeMusic(null);
        if (heartbeatSource != null) heartbeatSource.Play();
    }

    public void StartShopPhase()
    {
        if (currentPhase != GamePhase.Intro) return;
        currentPhase = GamePhase.Shop;
        if (monitorPanel != null) monitorPanel.SetActive(false); // Hide tablet to avoid overlaps
        if (shopPanel != null) shopPanel.SetActive(true);
    }

    public void StartEmergencyKitPhase()
    {
        if (currentPhase != GamePhase.Intro) return;

        // Require the medkit/koffer to be picked up first!
        if (!hasMedkit)
        {
            if (missionBannerManager != null)
            {
                missionBannerManager.ShowBanner(0, "⚠️ Du brauchst zuerst den Notfallkoffer!\nSammle ihn im Park auf.");
            }
            return;
        }

        currentPhase = GamePhase.EmergencyKit;
        if (monitorPanel != null) monitorPanel.SetActive(false); // Hide tablet to avoid overlaps
        if (emergencyKitPanel != null) emergencyKitPanel.SetActive(true);
        if (emergencyKitManager != null) emergencyKitManager.Activate();
    }

    public void CloseShop()
    {
        currentPhase = GamePhase.Intro;
        if (shopPanel != null) shopPanel.SetActive(false);
        if (monitorPanel != null) monitorPanel.SetActive(true); // Restore tablet menu
    }

    public void OnMissionRestart()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScores();
        StartIntroPhase();
    }

    public bool lastMissionSuccess = true;

    private void PlayMissionResultAnimation()
    {
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            if (lastMissionSuccess) pc.TriggerWin();
            else pc.TriggerLose();
        }
        lastMissionSuccess = true; // reset
    }

    public void OnQuizCompleted()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (quizManager != null) quizManager.gameObject.SetActive(false);
        
        // Route directly to CPR and AED minigames after quiz!
        StartCPRPhase();
    }

    public void StartBandagePhase()
    {
        currentPhase = GamePhase.Bandage;
        ChangeMusic(null);
        if (bandageManager != null) {
            bandageManager.gameObject.SetActive(true);
            bandageManager.Activate();
        }
    }

    public void OnBandageCompleted()
    {
        if (bandageManager != null) bandageManager.gameObject.SetActive(false);
        bleedingWoundHelped = true;
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Stark blutende Wunde", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
            {
                ambulanceManager.gameObject.SetActive(true);
                ambulanceManager.SpawnAmbulance(new Vector2(2, 6), CheckAllHelped);
            }
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    public void StartCPRPhase()
    {
        currentPhase = GamePhase.CPR;
        if (quizPanel != null) quizPanel.SetActive(false);
        ChangeMusic(null);
        if (cprManager != null) {
            cprManager.gameObject.SetActive(true);
            cprManager.Activate();
        }
    }

    public void OnCPRCompleted()
    {
        if (cprManager != null) cprManager.gameObject.SetActive(false);
        StartAEDPhase();
    }

    public void StartAEDPhase()
    {
        currentPhase = GamePhase.AED;
        if (aedPanel != null) aedPanel.SetActive(true);
        if (aedManager != null) aedManager.Activate();
    }

    public void OnAEDCompleted()
    {
        if (aedPanel != null) aedPanel.SetActive(false);
        bikeAccidentHelped = true;
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.SaveProgress();

        ShowMissionReport("Fahrradunfall (Reanimation)", true, () => {
            StartIntroPhase();
            if (ambulanceManager != null)
            {
                ambulanceManager.gameObject.SetActive(true);
                ambulanceManager.SpawnAmbulance(new Vector3(6, 2, 0), CheckAllHelped);
            }
            else CheckAllHelped();
            PlayMissionResultAnimation();
        });
    }

    private void CheckAllHelped()
    {
        if (bikeAccidentHelped && bleedingWoundHelped && unconsciousHelped && 
            burnInjuryHelped && chokingHelped && heatstrokeHelped && triageHelped && electricShockHelped && poisoningHelped)
        {
            StartResultPhase();
        }
        else
        {
            StartIntroPhase(); 
            // We can show a banner after returning to free-roam
            if (missionBannerManager != null)
            {
                // For now, pass a generic reward or fetch from recent mission score if available
                missionBannerManager.ShowBanner(15, null);
                ScoreManager.Instance?.AddCoins(15);
            }
        }
    }

    public MissionBannerManager missionBannerManager;

    public void StartResultPhase()
    {
        FindAnyObjectByType<PlayerController>()?.TriggerWin();
        cprManager.gameObject.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
        
        // Final reward at the end of the game
        int reward = 0;
        if (ScoreManager.Instance != null)
        {
            reward += ScoreManager.Instance.quizScore * 10;
            reward += ScoreManager.Instance.bandageScore * 50;
            reward += (ScoreManager.Instance.cprScore / 10) * 15;
            ScoreManager.Instance.AddCoins(reward);
        }
        
        Debug.Log($"Mission completed. Rewarded {reward} coins.");

        if (heartbeatSource != null) heartbeatSource.Stop();
        ChangeMusic(null);

        if (ScoreManager.Instance != null && resultDetailsText != null)
        {
            string details = $"Quiz: {ScoreManager.Instance.quizScore}/{ScoreManager.Instance.maxQuizScore}\n" +
                             $"Verband: {ScoreManager.Instance.bandageScore}/{ScoreManager.Instance.maxBandageScore}\n" +
                             $"CPR: {ScoreManager.Instance.cprScore}/{ScoreManager.Instance.maxCprScore}\n\n" +
                             "Deine Badges:\n";
            foreach (string badge in ScoreManager.Instance.GetBadges())
            {
                details += badge + "\n";
            }
            resultDetailsText.text = details;
        }
    }

    private void ChangeMusic(AudioClip clip)
    {
        if (bgmSource == null) return;
        if (clip == null) 
        {
            bgmSource.Stop();
            return;
        }
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private Coroutine randomMissionCoroutine;

    public void StartRandomMissionSpawner()
    {
        if (randomMissionCoroutine != null) StopCoroutine(randomMissionCoroutine);
        
        // Spawn immediately on freeroam park entry if no active mission
        SpawnRandomMissionIfNeeded();
        
        randomMissionCoroutine = StartCoroutine(RandomMissionRoutine());
    }

    public bool IsPlayerIndoors()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position.x > 50f;
        }
        return false;
    }

    private void SpawnRandomMissionIfNeeded()
    {
        if (currentPhase == GamePhase.Intro)
        {
            int activeCount = 0;
            foreach(var m in availableMissions) if (m.activeInstance != null) activeCount++;
            
            if (activeCount == 0 && availableMissions.Count > 0)
            {
                bool playerIndoors = IsPlayerIndoors();
                var matchingMissions = availableMissions.FindAll(m => !m.hasPlayed && m.isIndoor == playerIndoors);
                
                // Fallback to played ones in this environment
                if (matchingMissions.Count == 0)
                {
                    matchingMissions = availableMissions.FindAll(m => m.isIndoor == playerIndoors);
                }

                if (matchingMissions.Count > 0)
                {
                    FirstAidMission chosen = matchingMissions[Random.Range(0, matchingMissions.Count)];
                    StartMission(chosen.id);
                    Debug.Log($"Random spawner triggered (Indoors: {playerIndoors}): {chosen.title}");
                }
            }
        }
    }

    private IEnumerator RandomMissionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(20f, 40f)); // Checks every 20-40 seconds
            SpawnRandomMissionIfNeeded();
        }
    }

    public void StartMission(string missionId)
    {
        FirstAidMission mission = availableMissions.Find(m => m.id == missionId);
        if (mission == null) return;
        
        // Clean up existing active missions
        foreach (var m in availableMissions)
        {
            if (m.activeInstance != null)
            {
                Destroy(m.activeInstance);
                m.activeInstance = null;
            }
        }

        // Pick spawn point
        Transform[] validSpawns = mission.isIndoor ? indoorSpawnPoints : outdoorSpawnPoints;
        if (validSpawns == null || validSpawns.Length == 0) return;
        
        Vector2 spawnPos = validSpawns[Random.Range(0, validSpawns.Length)].position;
        
        if (mission.spawnAction != null)
        {
            mission.spawnAction(spawnPos);
        }

        if (mission.isIndoor)
        {
            StartCoroutine(PanCameraToDoor());
        }
    }

    private System.Collections.IEnumerator PanCameraToDoor()
    {
        GameObject door = GameObject.Find("Door");
        if (door == null || Camera.main == null) yield break;

        var camFollow = Camera.main.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>();
        if (camFollow == null) yield break;

        Transform originalTarget = camFollow.target;
        camFollow.target = door.transform;

        yield return new WaitForSeconds(2.5f);

        // Only restore if the target is still the door (in case another event changed it)
        if (camFollow.target == door.transform)
        {
            camFollow.target = originalTarget;
        }
    }

    public void ShowMissionReport(string missionName, bool success, System.Action onConfirm)
    {
        // 1. Calculate stats from ScoreManager
        int correct = 0;
        int errors = 0;
        if (ScoreManager.Instance != null)
        {
            correct = ScoreManager.Instance.currentMissionCorrect;
            errors = ScoreManager.Instance.currentMissionErrors;
        }

        // 2. Base coin reward
        int rewardCoins = 15;
        if (success)
        {
            int total = correct + errors;
            if (total > 0)
            {
                float accuracy = (float)correct / total;
                rewardCoins += Mathf.RoundToInt(accuracy * 35);
            }
            else
            {
                rewardCoins = 50;
            }
        }
        else
        {
            rewardCoins = 5;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(rewardCoins);
            ScoreManager.Instance.SaveProgress();
        }

        // Play Mission Report Sound
        if (AudioManager.Instance != null)
        {
            AudioClip soundToPlay = success ? AudioManager.Instance.missionSuccessSound : AudioManager.Instance.missionFailSound;
            if (soundToPlay != null) AudioManager.Instance.PlaySFX(soundToPlay);
        }

        // 3. Create Premium Report Dialog
        GameObject gameCanvasObj = GameObject.Find("GameCanvas");
        RectTransform canvasRT = null;
        if (gameCanvasObj != null) canvasRT = gameCanvasObj.GetComponent<RectTransform>();
        else canvasRT = FindFirstObjectByType<Canvas>()?.GetComponent<RectTransform>();

        if (canvasRT == null)
        {
            onConfirm?.Invoke();
            return;
        }

        int totalActions = correct + errors;
        float accPercent = totalActions > 0 ? ((float)correct / totalActions) * 100f : 100f;

        // ── Dimmed background overlay ──
        GameObject overlay = UIFactory.CreateFullscreenPanel(canvasRT, "ReportOverlay", new Color(0, 0, 0, 0.7f));
        overlay.GetComponent<Image>().raycastTarget = false; // Fix #6: Don't block clicks on the WEITER button

        // ── Main Report Container ──
        GameObject reportObj = UIFactory.CreateUIElement(canvasRT, "MissionReportDialog", Vector2.zero, new Vector2(720, 580));
        UIFactory.SetupImage(reportObj, winBaseSprite, false);
        reportObj.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.97f, 1f);
        RectTransform reportRT = reportObj.GetComponent<RectTransform>();

        // ── Drop shadow ──
        GameObject shadow = UIFactory.CreateUIElement(canvasRT, "ReportShadow", new Vector2(5, -5), new Vector2(730, 590));
        shadow.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
        shadow.GetComponent<Image>().raycastTarget = false;
        shadow.transform.SetSiblingIndex(reportObj.transform.GetSiblingIndex());

        // Pop-in animation
        reportObj.AddComponent<DialogPopIn>();

        // ── Header Bar ──
        Color headerColor = success ? new Color(0.15f, 0.55f, 0.25f) : new Color(0.65f, 0.15f, 0.15f);
        GameObject header = UIFactory.CreateUIElement(reportRT, "Header", new Vector2(0, 271), new Vector2(714, 38));
        UIFactory.SetupImage(header, winHeaderSprite, false);

        // ── Accent Line ──
        GameObject accentLine = UIFactory.CreateUIElement(reportRT, "AccentLine", new Vector2(0, 250), new Vector2(714, 4));
        Image accentImg = accentLine.GetComponent<Image>();
        accentImg.color = success ? new Color(0.2f, 0.8f, 0.4f) : new Color(1f, 0.3f, 0.3f);
        accentImg.raycastTarget = false;

        Text headerTxt = UIFactory.CreateText(header.transform, "Title", "📊 Einsatz-Bericht", new Vector2(10, 0), 22, TextAnchor.MiddleLeft);
        headerTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 38);
        headerTxt.color = Color.white;

        // ── Status Badge (large centered) ──
        string statusEmoji = success ? "🏅" : "❌";
        string statusLabel = success ? "MISSION ERFOLGREICH" : "MISSION FEHLGESCHLAGEN";
        Color statusColor = success ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.7f, 0.15f, 0.15f);

        // Status badge background
        Color badgeBgColor = success ? new Color(0.85f, 0.95f, 0.87f) : new Color(0.95f, 0.85f, 0.85f);
        GameObject badgeBg = UIFactory.CreateUIElement(reportRT, "BadgeBg", new Vector2(0, 195), new Vector2(680, 65));
        badgeBg.GetComponent<Image>().color = badgeBgColor;

        Text statusTxt = UIFactory.CreateText(reportRT, "StatusText", $"{statusEmoji}  {statusLabel}", new Vector2(0, 195), 30, TextAnchor.MiddleCenter);
        statusTxt.color = statusColor;
        statusTxt.fontStyle = FontStyle.Bold;
        statusTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 65);

        // ── Mission Name ──
        Text mNameTxt = UIFactory.CreateText(reportRT, "MissionName", $"Mission: {missionName}", new Vector2(0, 145), 20, TextAnchor.MiddleCenter);
        mNameTxt.color = new Color(0.3f, 0.3f, 0.35f);
        mNameTxt.fontStyle = FontStyle.Normal;
        mNameTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 30);
        Shadow mNameShadow = mNameTxt.GetComponent<Shadow>();
        if (mNameShadow != null) mNameShadow.effectColor = Color.clear;

        // ── Stats Cards Row ──
        float cardY = 55f;
        float cardWidth = 200f;
        float cardHeight = 120f;
        float spacing = 20f;
        float startX = -(cardWidth + spacing);

        // Card 1: Richtige Aktionen
        CreateStatCard(reportRT, "CorrectCard", "✓ RICHTIG", correct.ToString(), 
            new Vector2(startX, cardY), new Vector2(cardWidth, cardHeight),
            new Color(0.15f, 0.6f, 0.3f), new Color(0.88f, 0.96f, 0.9f));

        // Card 2: Fehler
        CreateStatCard(reportRT, "ErrorCard", "✕ FEHLER", errors.ToString(),
            new Vector2(startX + cardWidth + spacing, cardY), new Vector2(cardWidth, cardHeight),
            new Color(0.7f, 0.2f, 0.2f), new Color(0.96f, 0.88f, 0.88f));

        // Card 3: Genauigkeit
        string accText = $"{accPercent:F0}%";
        Color accColor = accPercent >= 80 ? new Color(0.15f, 0.6f, 0.3f) :
                         accPercent >= 50 ? new Color(0.8f, 0.6f, 0.1f) :
                                            new Color(0.7f, 0.2f, 0.2f);
        Color accBgColor = accPercent >= 80 ? new Color(0.88f, 0.96f, 0.9f) :
                           accPercent >= 50 ? new Color(0.96f, 0.94f, 0.85f) :
                                              new Color(0.96f, 0.88f, 0.88f);
        CreateStatCard(reportRT, "AccuracyCard", "◎ GENAUIGKEIT", accText,
            new Vector2(startX + 2 * (cardWidth + spacing), cardY), new Vector2(cardWidth, cardHeight),
            accColor, accBgColor);

        // ── Accuracy Progress Bar ──
        float barY = -25f;
        GameObject barBg = UIFactory.CreateUIElement(reportRT, "AccBarBg", new Vector2(0, barY), new Vector2(640, 18));
        barBg.GetComponent<Image>().color = new Color(0.82f, 0.82f, 0.85f);

        float barFillWidth = Mathf.Max(6, 640f * (accPercent / 100f));
        GameObject barFill = UIFactory.CreateUIElement(reportRT, "AccBarFill", 
            new Vector2(-(640f - barFillWidth) / 2f, barY), new Vector2(barFillWidth, 18));
        Color barColor = accPercent >= 80 ? new Color(0.2f, 0.75f, 0.4f) :
                         accPercent >= 50 ? new Color(0.9f, 0.7f, 0.15f) :
                                            new Color(0.85f, 0.25f, 0.25f);
        barFill.GetComponent<Image>().color = barColor;
        barFill.GetComponent<Image>().raycastTarget = false;

        // Bar label
        Text barLabel = UIFactory.CreateText(reportRT, "AccLabel", $"Erfolgsquote: {accPercent:F0}%", new Vector2(0, barY - 20f), 16, TextAnchor.MiddleCenter);
        barLabel.color = new Color(0.4f, 0.4f, 0.45f);
        barLabel.fontStyle = FontStyle.Normal;
        barLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 24);
        Shadow barShadow = barLabel.GetComponent<Shadow>();
        if (barShadow != null) barShadow.effectColor = Color.clear;

        // ── Reward Section ──
        float rewardY = -90f;
        GameObject rewardBg = UIFactory.CreateUIElement(reportRT, "RewardBg", new Vector2(0, rewardY), new Vector2(640, 50));
        rewardBg.GetComponent<Image>().color = new Color(0.92f, 0.9f, 0.82f);

        Text rewardTxt = UIFactory.CreateText(reportRT, "RewardText", $"💰 Belohnung: +{rewardCoins} Erste-Hilfe-Münzen", new Vector2(0, rewardY), 22, TextAnchor.MiddleCenter);
        rewardTxt.color = new Color(0.55f, 0.4f, 0.05f);
        rewardTxt.fontStyle = FontStyle.Bold;
        rewardTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 50);

        // ── Medal Line ──
        string medalText;
        if (accPercent >= 95 && success) medalText = "⭐⭐⭐ Goldmedaille – Perfekte Leistung!";
        else if (accPercent >= 75 && success) medalText = "⭐⭐ Silbermedaille – Sehr gut!";
        else if (success) medalText = "⭐ Bronzemedaille – Geschafft!";
        else medalText = "Versuche es noch einmal. Du schaffst das!";

        Text medalTxt = UIFactory.CreateText(reportRT, "MedalText", medalText, new Vector2(0, -140f), 18, TextAnchor.MiddleCenter);
        medalTxt.color = new Color(0.35f, 0.35f, 0.4f);
        medalTxt.fontStyle = FontStyle.Italic;
        medalTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 30);
        Shadow medalShadow = medalTxt.GetComponent<Shadow>();
        if (medalShadow != null) medalShadow.effectColor = Color.clear;

        // ── OK Button (premium styled) ──
        Color okBtnColor = success ? new Color(0.2f, 0.65f, 0.35f) : new Color(0.3f, 0.3f, 0.35f);
        GameObject okBtnObj = UIFactory.CreateButton(reportRT, "OKButton", "WEITER  →", new Vector2(0, -210), new Vector2(260, 60), winButtonSprite);
        Button okBtn = okBtnObj.GetComponent<Button>();
        // Style the button text
        Text okTxt = okBtnObj.GetComponentInChildren<Text>();
        if (okTxt != null) { okTxt.fontSize = 24; okTxt.color = Color.black; }
        UIFactory.AddHoverEffect(okBtnObj);
        
        okBtn.onClick.AddListener(() => {
            Destroy(overlay);
            Destroy(shadow);
            Destroy(reportObj);

            onConfirm?.Invoke();
            ScoreManager.Instance?.ResetMissionStats();

            if (loadedFromMenu)
            {
                loadedFromMenu = false;
                QuitToMenu();
            }
        });
    }

    /// <summary>
    /// Creates a premium stat card for the mission report
    /// </summary>
    private void CreateStatCard(RectTransform parent, string name, string label, string value, Vector2 pos, Vector2 size, Color accentColor, Color bgColor)
    {
        // Card background
        GameObject card = UIFactory.CreateUIElement(parent, name, pos, size);
        card.GetComponent<Image>().color = bgColor;

        // Top accent stripe
        GameObject stripe = UIFactory.CreateUIElement(card.GetComponent<RectTransform>(), "Stripe", 
            new Vector2(0, size.y / 2f - 3f), new Vector2(size.x, 6));
        stripe.GetComponent<Image>().color = accentColor;
        stripe.GetComponent<Image>().raycastTarget = false;

        // Label
        Text labelTxt = UIFactory.CreateText(card.transform, "Label", label, new Vector2(0, 18), 16, TextAnchor.MiddleCenter);
        labelTxt.color = new Color(0.4f, 0.4f, 0.45f);
        labelTxt.fontStyle = FontStyle.Normal;
        labelTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x - 10, 24);
        Shadow labelShadow = labelTxt.GetComponent<Shadow>();
        if (labelShadow != null) labelShadow.effectColor = Color.clear;

        // Value (large)
        Text valueTxt = UIFactory.CreateText(card.transform, "Value", value, new Vector2(0, -15), 36, TextAnchor.MiddleCenter);
        valueTxt.color = accentColor;
        valueTxt.fontStyle = FontStyle.Bold;
        valueTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x - 10, 50);
    }

    private IEnumerator BouncyScaleAnimation(RectTransform rt)
    {
        rt.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Elastic out curve
            float scale = Mathf.Sin(t * Mathf.PI * 0.7f) * 1.08f;
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}
