using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Zentrale Einstiegsinstanz für UI-Setup und Spiellogik.
/// Die Welt (Map, Player) wird direkt in der Szene platziert.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Victim & Arrow Sprites")]
    public Sprite victimSprite;
    public Sprite arrowSprite;
    public Sprite unconsciousVictimSprite;
    public Sprite ambulanceSprite;

    [Header("Windows GUI Assets")]
    public Sprite winBaseSprite;
    public Sprite winHeaderSprite;
    public Sprite winInnerFrameSprite;
    public Sprite winButtonSprite;
    public Sprite winButtonPressedSprite;

    [Header("Mission Sprites")]
    public Sprite menuBackgroundPremium;
    public Sprite aedBodySprite;
    public Sprite aedPadSprite;
    public Sprite burnArmSprite;
    public Sprite chokingPersonSprite;
    public Sprite heatstrokePersonSprite;
    public Sprite triageGroupSprite;
    public Sprite dispatcherSprite;

    [Header("Stabilize Assets")]
    public Sprite stabilizeBG;
    public Sprite victimArmUp;
    public Sprite victimLegBent;
    public Sprite victimSidePos;
    
    [Header("CPR Assets")]
    public Sprite cprBackground;
    public Sprite cprHands;
    
    [Header("Menu Assets")]
    public Sprite menuBackground;
    public Sprite menuLogo;
    public Sprite menuBGBeautiful;
    public Sprite bandageBackground;

    [Header("Sleep System")]
    public Sprite bedSprite;

    [Header("Audio Assets")]
    public AudioClip mapMusic;
    public AudioClip heartbeatClip;
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    public AudioClip pauseSound;
    public AudioClip unpauseSound;
    public AudioClip monitorOpenSound;
    public AudioClip monitorCloseSound;
    public Sprite shopIcon;
    public Sprite coinIcon;

    [Header("Environment Sounds")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    [Header("Mission Sounds")]
    public AudioClip missionSuccessSound;
    public AudioClip missionFailSound;

    [Header("Story Video")]
    public UnityEngine.Video.VideoClip[] storyVideoClips;
    public AudioClip telephoneSound;
    public AudioClip muffledTalkSound;

    [Header("Spiel-Einstellungen")]
    public float triggerDistance = 2.5f;

    [Header("Developer Options")]
    public bool devSkipToPark = true; // Skips menu, story, and gives koffer

    [Header("Modular Content")]
    public QuizData mainQuiz; // Senior Way: Assign questions via ScriptableObject

    private Transform victimBikeTransform;
    private Transform victimBleedTransform;
    private Transform victimUnconsciousTransform;
    private Transform burnVictimTransform;
    private Transform chokingVictimTransform;
    private Transform heatstrokeVictimTransform;
    private Transform triageVictimTransform;
    private Transform shockVictimTransform;
    private Transform poisonVictimTransform;
    private Transform boneFractureTransform;
    private Transform allergicShockTransform;
    private Transform drowningVictimTransform;
    private Transform diabeticShockTransform;
    private Transform panicAttackTransform;
    private Transform victimStrokeTransform;
    private Transform victimHeartAttackTransform;
    private Transform victimSnakebiteTransform;
    private RectTransform arrowRect;
    private GameManager gameManager;
    private ShopManager shopManager;
    private bool hasExitedShopProximity = true;

    // --- NEW ---
    private RectTransform interactionPromptRect;
    private Text interactionPromptText;
    private GameManager.VictimType? currentNearbyVictim = null;
    private Transform medkitTransform;
    private bool isNearMedkit = false;
    private GameObject gameCanvas;

    private void Awake()
    {
        InitializeCoreSystems();
        
        gameCanvas = CreateGameCanvas();
        
        new GameObject("VignetteEffect").AddComponent<VignetteEffect>();
        new GameObject("TerminalManager").AddComponent<TerminalManager>();
        new GameObject("StrokeManager").AddComponent<StrokeManager>();
        new GameObject("CertificateManager").AddComponent<CertificateManager>();
        new GameObject("FirstAidHandbook").AddComponent<FirstAidHandbook>();
        new GameObject("LearningTracker").AddComponent<LearningTracker>();
        new GameObject("TipSystem").AddComponent<TipSystem>();
        new GameObject("ExamManager").AddComponent<ExamManager>();
        new GameObject("MinesweeperManager").AddComponent<MinesweeperManager>();
        new GameObject("BootSequenceManager").AddComponent<BootSequenceManager>();
        new GameObject("SnakeManager").AddComponent<SnakeManager>();
        new GameObject("BadgeManager").AddComponent<BadgeManager>();
        new GameObject("ScreensaverManager").AddComponent<ScreensaverManager>();
        new GameObject("BSODManager").AddComponent<BSODManager>();
        new GameObject("ContextMenuManager").AddComponent<ContextMenuManager>();
        new GameObject("WallpaperManager").AddComponent<WallpaperManager>();
        new GameObject("CareerManager").AddComponent<CareerManager>();
        new GameObject("BossMailManager").AddComponent<BossMailManager>();
        new GameObject("WeatherManager").AddComponent<WeatherManager>();
        new GameObject("HighscoreManager").AddComponent<HighscoreManager>();
        new GameObject("GafferManager").AddComponent<GafferManager>();

        // Clear any old garbage park data saved from ParkArchitect
        PlayerPrefs.DeleteKey("CustomParkData");

        SetupAudio();
        
        InitMissions();
        SetupHouses();
        SetupArrow(gameCanvas);
        SetupInteractionPrompt(gameCanvas);
        SetupAmbulance();
        SetupPlayerHUD(gameCanvas);

        // Add Camera Shake
        if (Camera.main != null && Camera.main.gameObject.GetComponent<CameraShake>() == null)
        {
            Camera.main.gameObject.AddComponent<CameraShake>();
        }

        InitializeGameplayPanels(gameCanvas);
        SetupAtmosphere();
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.LoadProgress();
        }
        
        if (devSkipToPark)
        {
            gameManager.gameStarted = true;
            gameManager.hasMedkit = true; // Skip koffer mission
            gameManager.loadedFromMenu = true;
            Time.timeScale = 1f;

            // Set camera target to player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var cf = Camera.main?.GetComponent<CameraFollow>();
                if (cf != null) cf.target = player.transform;
                var ccf = Camera.main?.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>();
                if (ccf != null) ccf.target = player.transform;
            }

            gameManager.StartIntroPhase();
        }
        else
        {
            // Launch retro boot sequence; menu starts after boot completes
            gameManager.StartMenuPhase();

            if (BootSequenceManager.Instance != null)
            {
                RectTransform canvasRT = gameCanvas.GetComponent<RectTransform>();
                BootSequenceManager.Instance.OnBootComplete = () =>
                {
                    // Boot is done — chime already played inside BootSequenceManager
                };
                BootSequenceManager.Instance.StartBoot(canvasRT);
            }
            else
            {
                // Fallback: play chime immediately if boot manager unavailable
                if (AudioManager.Instance != null && AudioManager.Instance.bootChimeSound != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.bootChimeSound, 0.7f);
                }
            }
        }

        if (!gameManager.parkCleanupHelped)
        {
            GameObject missionObj = new GameObject("CleanParkMissionManager");
            CleanParkMission cpm = missionObj.AddComponent<CleanParkMission>();
            cpm.StartMission(gameManager, gameManager.outdoorSpawnPoints);
        }

        // Init mode
        if (CareerManager.Instance != null && CareerManager.Instance.isCareerMode)
        {
            CareerManager.Instance.StartDay();
        }
        else
        {
            gameManager.StartRandomMissionSpawner();
        }
    }

    private void SetupAtmosphere()
    {
        if (Camera.main != null && Camera.main.GetComponent<DayNightCycle>() == null)
        {
            Camera.main.gameObject.AddComponent<DayNightCycle>();
        }
        
        GameObject envAnim = new GameObject("EnvironmentAnimator");
        envAnim.AddComponent<EnvironmentAnimator>();

        if (FindFirstObjectByType<ScreenFader>() == null)
        {
            new GameObject("ScreenFader").AddComponent<ScreenFader>();
        }
    }

    private void InitializeCoreSystems()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        if (FindFirstObjectByType<ScoreManager>() == null)
        {
            new GameObject("ScoreManager").AddComponent<ScoreManager>();
        }

        GameObject gmObj = new GameObject("GameManager");
        gameManager = gmObj.AddComponent<GameManager>();
        
        gameManager.mapMusic = mapMusic; 
        gameManager.pauseSound = pauseSound;
        gameManager.unpauseSound = unpauseSound;
        gameManager.monitorOpenSound = monitorOpenSound;
        gameManager.monitorCloseSound = monitorCloseSound;

        // Copy retro UI sprites
        gameManager.winBaseSprite = winBaseSprite;
        gameManager.winHeaderSprite = winHeaderSprite;
        gameManager.winButtonSprite = winButtonSprite;
        gameManager.winInnerFrameSprite = winInnerFrameSprite;

        // Instantiate new polish managers procedurally at runtime
        GameObject lmObj = new GameObject("LocalizationManager");
        LocalizationManager lm = lmObj.AddComponent<LocalizationManager>();
        lm.LanguageChangedEvent += LocalizeMenuUI;
    }

    private GameObject CreateGameCanvas()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private void SetupAudio()
    {
        GameObject audioObj = new GameObject("AudioSources");
        audioObj.transform.SetParent(gameManager.transform);
        
        gameManager.bgmSource = audioObj.AddComponent<AudioSource>();
        gameManager.heartbeatSource = audioObj.AddComponent<AudioSource>();
        gameManager.heartbeatSource.clip = heartbeatClip;
        gameManager.heartbeatSource.loop = true;
        gameManager.heartbeatSource.playOnAwake = false;
        gameManager.sfxSource = audioObj.AddComponent<AudioSource>();

        // Automatisch den AudioManager anlegen, damit er nicht mehr manuell in die Szene gezogen werden muss
        if (FindFirstObjectByType<AudioManager>() == null)
        {
            AudioManager am = audioObj.AddComponent<AudioManager>();
            am.sfxSource = gameManager.sfxSource;
            am.musicSource = gameManager.bgmSource;
            am.doorOpenSound = doorOpenSound;
            am.doorCloseSound = doorCloseSound;
            am.missionSuccessSound = missionSuccessSound;
            am.missionFailSound = missionFailSound;
        }
    }

    private GameObject victimsContainer;

    private void InitMissions()
    {
        float entityScale = 0.08f;
        
        victimsContainer = new GameObject("Victims");
        // We don't set it to inactive anymore, as individual victims will be spawned directly into it and should be active
        // victimsContainer.SetActive(false); 

        // Define generic spawn points
        GameObject indoorSpawn1 = new GameObject("IndoorSpawn1");
        indoorSpawn1.transform.position = new Vector3(100, 102, 0); // Inside the house
        gameManager.indoorSpawnPoints = new Transform[] { indoorSpawn1.transform };

        GameObject out1 = new GameObject("OutdoorSpawn1"); out1.transform.position = new Vector3(3, -3, 0);
        GameObject out2 = new GameObject("OutdoorSpawn2"); out2.transform.position = new Vector3(-1, 0, 0);
        GameObject out3 = new GameObject("OutdoorSpawn3"); out3.transform.position = new Vector3(9, 3, 0);
        GameObject out4 = new GameObject("OutdoorSpawn4"); out4.transform.position = new Vector3(-58, 0.7f, 0);
        GameObject out5 = new GameObject("OutdoorSpawn5"); out5.transform.position = new Vector3(3, 3, 0);
        gameManager.outdoorSpawnPoints = new Transform[] { out1.transform, out2.transform, out3.transform, out4.transform, out5.transform };

        // Register missions
        gameManager.availableMissions.Add(CreateMission("m1", "Fahrradunfall", GameManager.VictimType.BikeAccident, Color.white, false, (pos, m) => {
            GameObject v = CreateWorldObject("Victim_Bike", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimBikeTransform = v.transform;
            SetupSprite(v, victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));
        gameManager.availableMissions.Add(CreateMission("m16", "Vergiftung", GameManager.VictimType.Poisoning, new Color(0.6f, 0.2f, 0.8f), false, (pos, m) => {
            GameObject v = CreateWorldObject("Victim_Poison", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            poisonVictimTransform = v.transform;
            SetupSprite(v, unconsciousVictimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m17", "Hundevergiftung", GameManager.VictimType.DogPoisoning, new Color(0.8f, 0.5f, 0.2f), false, (pos, m) => {
            GameObject v = CreateWorldObject("Victim_Dog", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            // We use the victim sprite and color it brown for now
            SetupSprite(v, victimSprite);
            v.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.3f, 0.1f);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // (Park Aufräumen was moved to run parallel in Start())

        gameManager.availableMissions.Add(CreateMission("m2", "Schnittwunde", GameManager.VictimType.BleedingWound, Color.red, false, (pos, m) => {
            GameObject v = CreateWorldObject("Victim_Bleeding", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimBleedTransform = v.transform;
            SetupSprite(v, victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m3", "Bewusstlose Person", GameManager.VictimType.UnconsciousPerson, Color.gray, true, (pos, m) => {
            GameObject v = CreateWorldObject("Victim_Unconscious", pos, new Vector3(entityScale * 1.2f, entityScale * 1.2f, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimUnconsciousTransform = v.transform;
            SetupSprite(v, unconsciousVictimSprite != null ? unconsciousVictimSprite : victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m4", "Verbrennung", GameManager.VictimType.BurnInjury, Color.magenta, true, (pos, m) => {
            GameObject v = SpawnVictim(pos, "BurnInjury", "Verbrennung am Grill", GameManager.VictimType.BurnInjury, Color.magenta);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m5", "Verschlucken", GameManager.VictimType.Choking, Color.yellow, true, (pos, m) => {
            GameObject v = SpawnVictim(pos, "ChokingVictim", "Person verschluckt sich", GameManager.VictimType.Choking, Color.yellow);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m6", "Hitzschlag", GameManager.VictimType.Heatstroke, Color.red, false, (pos, m) => {
            GameObject v = SpawnVictim(pos, "HeatstrokeVictim", "Person bricht in Hitze zusammen", GameManager.VictimType.Heatstroke, Color.red);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m7", "Stromschlag", GameManager.VictimType.ElectricShock, Color.cyan, true, (pos, m) => {
            GameObject v = SpawnVictim(pos, "ElectricShock", "Person am Stromkabel", GameManager.VictimType.ElectricShock, Color.cyan);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m8", "Vergiftung", GameManager.VictimType.Poisoning, new Color(0.5f, 0, 0.5f), true, (pos, m) => {
            GameObject v = SpawnVictim(pos, "Poisoning", "Giftige Pilze gegessen", GameManager.VictimType.Poisoning, new Color(0.5f, 0, 0.5f));
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m9", "Massenunfall (Triage)", GameManager.VictimType.TriageScene, Color.blue, false, (pos, m) => {
            GameObject v = SpawnVictim(pos, "TriageGroup", "Massenunfall mit mehreren Verletzten", GameManager.VictimType.TriageScene, Color.blue);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // --- 5 NEW MISSIONS ---

        // m10: Knochenbruch am Spielplatz (outdoor)
        gameManager.availableMissions.Add(CreateMission("m10", "Knochenbruch am Spielplatz", GameManager.VictimType.BoneFracture, new Color(1f, 0.6f, 0.1f), false, (pos, m) =>
        {
            GameObject v = SpawnVictim(pos, "BoneFractureVictim", "Kind vom Klettergerüst gefallen", GameManager.VictimType.BoneFracture, new Color(1f, 0.6f, 0.1f));
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m11: Allergischer Schock (indoor – Bienenstich)
        gameManager.availableMissions.Add(CreateMission("m11", "Allergischer Schock", GameManager.VictimType.AllergicShock, new Color(1f, 0.85f, 0f), true, (pos, m) =>
        {
            GameObject v = SpawnVictim(pos, "AllergicShockVictim", "Bienenstich – anaphylaktischer Schock", GameManager.VictimType.AllergicShock, new Color(1f, 0.85f, 0f));
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m12: Ertrinkungsunfall (outdoor – am Teich)
        gameManager.availableMissions.Add(CreateMission("m12", "Ertrinkungsunfall", GameManager.VictimType.DrowningVictim, new Color(0.1f, 0.5f, 1f), false, (pos, m) =>
        {
            GameObject v = CreateWorldObject("Victim_Drowning", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            drowningVictimTransform = v.transform;
            SetupSprite(v, unconsciousVictimSprite != null ? unconsciousVictimSprite : victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m13: Diabetischer Schock (indoor)
        gameManager.availableMissions.Add(CreateMission("m13", "Diabetischer Schock", GameManager.VictimType.DiabeticShock, new Color(0.2f, 0.8f, 0.4f), true, (pos, m) =>
        {
            GameObject v = SpawnVictim(pos, "DiabeticShockVictim", "Diabetiker – zu niedriger Blutzucker", GameManager.VictimType.DiabeticShock, new Color(0.2f, 0.8f, 0.4f));
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m14: Panikattacke / Hyperventilation (outdoor – auf der Bank)
        gameManager.availableMissions.Add(CreateMission("m14", "Panikattacke", GameManager.VictimType.PanicAttack, new Color(0.9f, 0.4f, 0.6f), false, (pos, m) =>
        {
            GameObject v = CreateWorldObject("Victim_Panic", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            panicAttackTransform = v.transform;
            SetupSprite(v, victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m15: Verdacht auf Schlaganfall (Stroke) (outdoor - auf der Bank)
        gameManager.availableMissions.Add(CreateMission("m15", "Verdacht auf Schlaganfall", GameManager.VictimType.Stroke, new Color(0.85f, 0.45f, 0f), false, (pos, m) =>
        {
            GameObject v = CreateWorldObject("Victim_Stroke", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimStrokeTransform = null;
            victimHeartAttackTransform = null;
            victimSnakebiteTransform = null;
            victimStrokeTransform = v.transform;
            SetupSprite(v, victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m18: Herzinfarkt (Heart Attack)
        gameManager.availableMissions.Add(CreateMission("m18", "Herzinfarkt", GameManager.VictimType.HeartAttack, new Color(0.9f, 0.1f, 0.1f), false, (pos, m) =>
        {
            GameObject v = CreateWorldObject("Victim_HeartAttack", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimHeartAttackTransform = v.transform;
            SetupSprite(v, victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // m19: Schlangenbiss (Snakebite)
        gameManager.availableMissions.Add(CreateMission("m19", "Schlangenbiss", GameManager.VictimType.Snakebite, new Color(0.2f, 0.8f, 0.2f), false, (pos, m) =>
        {
            GameObject v = CreateWorldObject("Victim_Snakebite", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimSnakebiteTransform = v.transform;
            SetupSprite(v, victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // Spawn background NPCs
        SpawnNPCs(); 
    }

    private GameManager.FirstAidMission CreateMission(string id, string title, GameManager.VictimType type, Color color, bool isIndoor, System.Action<Vector2, GameManager.FirstAidMission> spawner)
    {
        return new GameManager.FirstAidMission {
            id = id,
            title = title,
            type = type,
            color = color,
            isIndoor = isIndoor,
            hasPlayed = false,
            spawnAction = (pos) => spawner(pos, gameManager.availableMissions.Find(m => m.id == id))
        };
    }

    private Transform activeArrowTarget;

    private void SetPlayerArrow(Transform target)
    {
        activeArrowTarget = target;
    }

    private GameObject SpawnVictim(Vector2 pos, string name, string hint, GameManager.VictimType type, Color col)
    {
        // Check if the user placed a placeholder object in the Unity Editor
        GameObject placeholder = GameObject.Find(name);
        if (placeholder != null)
        {
            pos = placeholder.transform.position;
            Object.Destroy(placeholder);
        }

        GameObject v = new GameObject(name);
        v.transform.SetParent(victimsContainer.transform);
        v.transform.position = pos;
        
        SpriteRenderer sr = v.AddComponent<SpriteRenderer>();
        sr.sprite = unconsciousVictimSprite;
        sr.color = col;
        v.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
        // Fix: Set correct sorting layer so victims render above map tiles
        sr.sortingLayerName = "Layer 3";
        sr.sortingOrder = 100;

        CircleCollider2D col2d = v.AddComponent<CircleCollider2D>();
        col2d.radius = 1f;
        col2d.isTrigger = true;

        if (name == "BikeAccident") victimBikeTransform = v.transform;
        else if (name == "BleedingWound") victimBleedTransform = v.transform;
        else if (name == "UnconsciousPerson") victimUnconsciousTransform = v.transform;
        else if (name == "BurnInjury") burnVictimTransform = v.transform;
        else if (name == "ChokingVictim") chokingVictimTransform = v.transform;
        else if (name == "HeatstrokeVictim") heatstrokeVictimTransform = v.transform;
        else if (name == "TriageGroup") triageVictimTransform = v.transform;
        else if (name == "ElectricShock") shockVictimTransform = v.transform;
        else if (name == "Poisoning") poisonVictimTransform = v.transform;
        else if (name == "BoneFractureVictim") boneFractureTransform = v.transform;
        else if (name == "AllergicShockVictim") allergicShockTransform = v.transform;
        else if (name == "DiabeticShockVictim") diabeticShockTransform = v.transform;
        else if (name == "Victim_HeartAttack") victimHeartAttackTransform = v.transform;
        else if (name == "Victim_Snakebite") victimSnakebiteTransform = v.transform;

        return v;
    }

    private void SetupHouses()
    {
        // Procedurally generate a solid white texture to use for doors and floors
        Texture2D solidTex = new Texture2D(1, 1);
        solidTex.SetPixel(0, 0, Color.white);
        solidTex.Apply();
        Sprite solidSprite = Sprite.Create(solidTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // 1. Create the Interior Room (Far away from the main map)
        Vector3 interiorPos = new Vector3(100f, 100f, 0f);
        GameObject interiorRoom = new GameObject("InteriorRoom");
        interiorRoom.transform.position = interiorPos;

        // Simple interior floor using a solid dark color
        GameObject floor = new GameObject("Floor");
        floor.transform.SetParent(interiorRoom.transform);
        floor.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        sr.sprite = solidSprite;
        sr.color = new Color(0.12f, 0.12f, 0.12f);
        floor.transform.localScale = new Vector3(12f, 12f, 1f);

        // 2. Create the Exterior Door (at the bottom boundary wall of the park, near path)
        Vector3 exteriorPos = new Vector3(1.5f, -6.0f, 0f);
        GameObject exteriorDoor = new GameObject("HouseExteriorDoor");
        exteriorDoor.transform.position = exteriorPos;
        BoxCollider2D extCol = exteriorDoor.AddComponent<BoxCollider2D>();
        extCol.isTrigger = true;
        extCol.size = new Vector2(1.5f, 1.5f);
        
        DoorTransition extDoor = exteriorDoor.AddComponent<DoorTransition>();
        
        // 3. Create the Interior Door (inside the room to exit)
        GameObject interiorDoor = new GameObject("HouseInteriorDoor");
        interiorDoor.transform.position = interiorPos + new Vector3(0f, -3f, 0f);
        BoxCollider2D intCol = interiorDoor.AddComponent<BoxCollider2D>();
        intCol.isTrigger = true;
        intCol.size = new Vector2(1.5f, 1.5f);
        
        DoorTransition intDoor = interiorDoor.AddComponent<DoorTransition>();

        // Link them up
        extDoor.targetPosition = interiorDoor.transform;
        intDoor.targetPosition = exteriorDoor.transform;

        // Visual indicator for doors (Solid wooden-colored panels)
        SpriteRenderer extSr = exteriorDoor.AddComponent<SpriteRenderer>();
        extSr.sprite = solidSprite;
        extSr.color = new Color(0.42f, 0.22f, 0.08f); // Beautiful dark brown wooden door
        extSr.sortingOrder = 10;
        exteriorDoor.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

        SpriteRenderer intSr = interiorDoor.AddComponent<SpriteRenderer>();
        intSr.sprite = solidSprite;
        intSr.color = new Color(0.42f, 0.22f, 0.08f); // Beautiful dark brown wooden door
        intSr.sortingOrder = 10;
        interiorDoor.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
    }

    private void SpawnNPCs()
    {
        GameObject mihoPrefab = Resources.Load<GameObject>("Miho");
        if (mihoPrefab == null)
        {
            Debug.LogWarning("Miho prefab not found in Resources folder. NPCs will not be spawned.");
            return;
        }

        Vector3[] npcPositions = new Vector3[]
        {
            new Vector3(4, 6, 0),
            new Vector3(7, -1, 0),
            new Vector3(2, 2, 0),
            new Vector3(10, 5, 0),
            new Vector3(-4, 3, 0) // Extra NPC
        };

        for (int i = 0; i < npcPositions.Length; i++)
        {
            GameObject npc = Instantiate(mihoPrefab, npcPositions[i], Quaternion.identity);
            npc.name = "MihoNPC_" + i;
            npc.transform.SetParent(victimsContainer.transform);
            
            // Adjust scale because Miho.prefab might have a different default scale
            npc.transform.localScale = new Vector3(0.12f, 0.12f, 1f);
            
            // Add custom scripts
            npc.AddComponent<MihoNPC>();
            npc.AddComponent<NPCDialogue>();
            npc.AddComponent<NPCWander>(); // Fix: NPCs wander around instead of standing still

            // Fix visibility: Do NOT overwrite individual SpriteRenderer sorting orders,
            // as Miho is made of multiple parts! Instead, use a SortingGroup just like PlayerController.
            UnityEngine.Rendering.SortingGroup sg = npc.AddComponent<UnityEngine.Rendering.SortingGroup>();
            sg.sortingLayerName = "Layer 1"; // Must match PlayerController!
            sg.sortingOrder = 2; // Match the sorting order of the props so Y-sorting works!

            // Also update all children to be on Layer 1 so they render properly within the group
            SpriteRenderer[] srs = npc.GetComponentsInChildren<SpriteRenderer>(true);
            
            // Generate a random tint for this NPC to vary hair/clothes
            Color randomTint = Color.HSVToRGB(Random.value, Random.Range(0.2f, 0.6f), Random.Range(0.7f, 1f));
            
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = "Layer 1";
                if (sr.gameObject.name != "shadow")
                {
                    sr.color = randomTint;
                }
            }
        }
    }

    private void SetupArrow(GameObject canvas)
    {
        GameObject arrow = UIFactory.CreateUIElement(canvas.GetComponent<RectTransform>(), "DirectionArrow", Vector2.zero, new Vector2(60, 60));
        arrowRect = arrow.GetComponent<RectTransform>();
        UIFactory.SetupImage(arrow, arrowSprite, true);
    }

    private void SetupInteractionPrompt(GameObject canvas)
    {
        GameObject prompt = UIFactory.CreateUIElement(canvas.GetComponent<RectTransform>(), "InteractionPrompt", new Vector2(0, 100), new Vector2(350, 60));
        interactionPromptRect = prompt.GetComponent<RectTransform>();
        UIFactory.SetupImage(prompt, winInnerFrameSprite, false);
        Image img = prompt.GetComponent<Image>();
        img.type = Image.Type.Sliced; // Ensures nice borders if sprite supports it
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark elegant box

        interactionPromptText = UIFactory.CreateText(prompt.transform, "PromptText", "[E] Untersuchen", Vector2.zero, 28, TextAnchor.MiddleCenter);
        interactionPromptText.color = new Color(1f, 0.8f, 0.2f); // Golden yellow font
        interactionPromptText.fontStyle = FontStyle.Bold;

        // Add float animation
        FloatingUI fUI = prompt.AddComponent<FloatingUI>();
        fUI.amplitude = 8f;
        fUI.speed = 4f;

        prompt.SetActive(false);
    }

    private void SetupPlayerHUD(GameObject canvasObj)
    {
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();

        // ── Stamina bar ──────────────────────────────────────────────────────
        GameObject staminaRoot = UIFactory.CreateUIElement(canvasRT, "StaminaBar", new Vector2(-820, -490), new Vector2(200, 16));
        RectTransform staminaRT = staminaRoot.GetComponent<RectTransform>();
        staminaRT.anchorMin = new Vector2(0, 0);
        staminaRT.anchorMax = new Vector2(0, 0);
        staminaRT.pivot     = new Vector2(0, 0);
        staminaRT.anchoredPosition = new Vector2(16, 90);

        // background (CreateUIElement already adds Image)
        Image staminaBg = staminaRoot.GetComponent<Image>();
        staminaBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        // label
        Text staminaLabel = UIFactory.CreateText(staminaRoot.transform, "Label", "⚡ Ausdauer", new Vector2(0, 12), 12, TextAnchor.MiddleLeft);
        staminaLabel.color = Color.white;
        staminaLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(4, 10);

        // fill
        GameObject staminaFillObj = UIFactory.CreateUIElement(staminaRoot.GetComponent<RectTransform>(), "Fill", Vector2.zero, new Vector2(200, 16));
        Image staminaFillImg = staminaFillObj.GetComponent<Image>();
        staminaFillImg.color = new Color(0.2f, 0.8f, 0.9f);
        RectTransform staminaFillRT = staminaFillObj.GetComponent<RectTransform>();
        staminaFillRT.anchorMin = new Vector2(0, 0);
        staminaFillRT.anchorMax = new Vector2(1, 1);
        staminaFillRT.offsetMin = Vector2.zero;
        staminaFillRT.offsetMax = Vector2.zero;
        staminaFillRT.pivot     = new Vector2(0, 0.5f);

        // ── Tiredness bar ────────────────────────────────────────────────────
        GameObject tiredRoot = UIFactory.CreateUIElement(canvasRT, "TirednessBar", new Vector2(16, 60), new Vector2(200, 16));
        RectTransform tiredRT = tiredRoot.GetComponent<RectTransform>();
        tiredRT.anchorMin = new Vector2(0, 0);
        tiredRT.anchorMax = new Vector2(0, 0);
        tiredRT.pivot     = new Vector2(0, 0);
        tiredRT.anchoredPosition = new Vector2(16, 60);

        Image tiredBg = tiredRoot.GetComponent<Image>();
        tiredBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        Text tiredLabel = UIFactory.CreateText(tiredRoot.transform, "Label", "🌙 Müdigkeit", new Vector2(4, 10), 12, TextAnchor.MiddleLeft);
        tiredLabel.color = Color.white;
        tiredLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(4, 10);

        GameObject tiredFillObj = UIFactory.CreateUIElement(tiredRoot.GetComponent<RectTransform>(), "Fill", Vector2.zero, new Vector2(200, 16));
        Image tiredFillImg = tiredFillObj.GetComponent<Image>();
        tiredFillImg.color = new Color(0.1f, 0.8f, 0.2f);
        RectTransform tiredFillRT = tiredFillObj.GetComponent<RectTransform>();
        tiredFillRT.anchorMin = new Vector2(0, 0);
        tiredFillRT.anchorMax = new Vector2(1, 1);
        tiredFillRT.offsetMin = Vector2.zero;
        tiredFillRT.offsetMax = Vector2.zero;
        tiredFillRT.pivot     = new Vector2(0, 0.5f);

        // ── Wire up PlayerController stamina → HUD ───────────────────────────
        StartCoroutine(WireHUDRoutine(staminaFillRT, tiredFillRT, tiredFillImg));

        // ── Spawn SleepManager ───────────────────────────────────────────────
        GameObject smObj = new GameObject("SleepManager");
        SleepManager sm  = smObj.AddComponent<SleepManager>();
        sm.bedSprite              = bedSprite;
        sm.tirednessBarFill       = tiredFillRT;
    }

    IEnumerator WireHUDRoutine(RectTransform staminaFill, RectTransform tiredFill, Image tiredFillImg)
    {
        PlayerController pc = null;
        while (pc == null) { pc = FindFirstObjectByType<PlayerController>(); yield return null; }

        while (true)
        {
            if (pc == null) yield break;

            // Stamina bar
            float sPct = pc.currentStamina / pc.maxStamina;
            staminaFill.anchorMax = new Vector2(sPct, 1);
            Image sImg = staminaFill.GetComponent<Image>();
            if (sImg != null) sImg.color = sPct < 0.25f ? new Color(0.9f, 0.2f, 0.1f) : new Color(0.2f, 0.8f, 0.9f);

            yield return null;
        }
    }

    private void SetupAmbulance()
    {
        GameObject ambObj = CreateWorldObject("Ambulance", Vector3.zero, new Vector3(0.1f, 0.1f, 1));
        ambObj.SetActive(false);
        
        AmbulanceManager am = ambObj.AddComponent<AmbulanceManager>();
        am.ambulanceTransform = ambObj.transform;
        SetupSprite(ambObj, ambulanceSprite);
        am.ambulanceRenderer = ambObj.GetComponent<SpriteRenderer>();
        am.ambulanceSprite = ambulanceSprite;
        
        gameManager.ambulanceManager = am;

        // Spawn Medkit near ambulance spawn point (0, 0)
        GameObject medkit = CreateWorldObject("Medkit", new Vector3(0, -1.5f, 0), Vector3.one);
        Sprite medkitSprite = Resources.Load<Sprite>("emergency_kit");
        if (medkitSprite != null)
        {
            SetupSprite(medkit, medkitSprite);
            float maxBound = Mathf.Max(medkitSprite.bounds.size.x, medkitSprite.bounds.size.y);
            if (maxBound > 0f)
            {
                float scale = 0.8f / maxBound;
                medkit.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            SetupSprite(medkit, coinIcon);
            medkit.GetComponent<SpriteRenderer>().color = Color.red;
            medkit.transform.localScale = new Vector3(0.06f, 0.06f, 1f);
        }
        medkitTransform = medkit.transform;


    }

    private void InitializeGameplayPanels(GameObject canvas)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        SetupMenuPanel(canvasRect);
        SetupQuizPanel(canvasRect);
        SetupBandagePanel(canvasRect);
        SetupCPRPanel(canvasRect);
        SetupStabilizePanel(canvasRect);
        SetupResultPanel(canvasRect);
        SetupShopPanel(canvasRect);
        SetupStoryPanel(canvasRect);
        SetupCallPanel(canvasRect);
        SetupAEDPanel(canvasRect);
        SetupBurnPanel(canvasRect);
        SetupChokingPanel(canvasRect);
        SetupHeatstrokePanel(canvasRect);
        SetupTriagePanel(canvasRect);
        SetupElectricShockPanel(canvasRect);
        SetupPoisonPanel(canvasRect);
        SetupPausePanel(canvasRect);
        SetupMonitorPanel(canvasRect);
        SetupEmergencyKitPanel(canvasRect);
        SetupMissionBannerPanel(canvasRect);
    }

    private void SetupMissionBannerPanel(RectTransform parent)
    {
        // Container that is completely transparent, holding the moving banner
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "MissionBannerPanel", new Color(0, 0, 0, 0));
        // We do NOT set panel inactive, because the panel is always there but the banner itself will move in and out.
        // Wait, it's cleaner to just toggle the banner object itself.
        panel.SetActive(false); // We can toggle this entire panel on/off
        
        MissionBannerManager mbm = panel.AddComponent<MissionBannerManager>();
        gameManager.missionBannerManager = mbm; // Need to add this reference to GameManager

        // The actual banner object
        GameObject banner = UIFactory.CreateUIElement(panel.transform as RectTransform, "Banner", new Vector2(0, 200), new Vector2(500, 100));
        
        // Anchor to top center
        RectTransform rt = banner.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, 200); // Start off-screen

        UIFactory.SetupImage(banner, winBaseSprite, false); // Use solid Windows base styling
        banner.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f); // Solid retro light-gray

        // Add an inner frame for aesthetics
        GameObject innerFrame = UIFactory.CreateUIElement(banner.transform as RectTransform, "InnerFrame", Vector2.zero, new Vector2(480, 80));
        UIFactory.SetupImage(innerFrame, winInnerFrameSprite, false);
        innerFrame.GetComponent<Image>().color = Color.white;

        mbm.bannerRect = rt;
        
        // Banner Text
        Text t = UIFactory.CreateText(innerFrame.transform, "Text", "MISSION ERFOLGREICH!", Vector2.zero, 28, TextAnchor.MiddleCenter);
        t.color = Color.black;
        t.fontStyle = FontStyle.Bold;
        mbm.bannerText = t;
    }

    private void SetupMonitorPanel(RectTransform parent)
    {
        // Dark Overlay Background
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "MonitorPanel", new Color(0, 0, 0, 0.85f));
        panel.SetActive(false);
        gameManager.monitorPanel = panel;

        MonitorManager mm = panel.AddComponent<MonitorManager>();
        mm.gameManager = gameManager;
        mm.monitorPanel = panel;
        gameManager.monitorManager = mm;

        // Windows Dialog
        GameObject dialog = CreateWindowsDialog(panel.transform, "MonitorDialog", "System-Monitor.exe", Vector2.zero, new Vector2(800, 700));

        // Close Button (Top Right)
        GameObject closeBtn = CreateWindowsButton(dialog.transform, "CloseBtn", "X", new Vector2(360, 315), new Vector2(40, 40));
        closeBtn.GetComponent<Button>().onClick.AddListener(() => mm.CloseMonitor());

        // Finances / Coins section
        GameObject coinFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "CoinFrame", new Vector2(0, 200), new Vector2(700, 80));
        UIFactory.SetupImage(coinFrame, winInnerFrameSprite, false);
        mm.coinsText = UIFactory.CreateText(coinFrame.transform, "Coins", "Guthaben: 0 Coins", Vector2.zero, 32, TextAnchor.MiddleCenter);
        mm.coinsText.color = Color.black;
        mm.coinsText.fontStyle = FontStyle.Bold;

        // Statistics / Badges section
        GameObject statsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "StatsFrame", new Vector2(0, 40), new Vector2(700, 200));
        UIFactory.SetupImage(statsFrame, winInnerFrameSprite, false);
        mm.statsText = UIFactory.CreateText(statsFrame.transform, "Stats", "Aktive Abzeichen:\n- Keine -", Vector2.zero, 28, TextAnchor.MiddleCenter);
        mm.statsText.color = new Color(0.2f, 0.2f, 0.6f); // Dark blue for retro feel
        mm.statsText.fontStyle = FontStyle.Bold;

        // Missions section
        GameObject missionsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "MissionsFrame", new Vector2(0, -180), new Vector2(700, 200));
        UIFactory.SetupImage(missionsFrame, winInnerFrameSprite, false);
        mm.missionsText = UIFactory.CreateText(missionsFrame.transform, "Missions", "Einsatz-Protokoll:\nNoch keine Einsätze absolviert.", Vector2.zero, 24, TextAnchor.MiddleCenter);
        mm.missionsText.color = new Color(0.1f, 0.4f, 0.1f); // Dark green
        mm.missionsText.fontStyle = FontStyle.Bold;

        // Start Quiz Button
        GameObject quizBtn = CreateWindowsButton(dialog.transform, "QuizBtn", "WISSENS-QUIZ", new Vector2(-250, -310), new Vector2(220, 60));
        quizBtn.GetComponent<Button>().onClick.AddListener(() => {
            mm.CloseMonitor();
            gameManager.StartQuizPhase();
        });

        // Start Koffer Training Button
        GameObject kitBtn = CreateWindowsButton(dialog.transform, "KitBtn", "KOFFER-TRAINING", new Vector2(0, -310), new Vector2(220, 60));
        kitBtn.GetComponent<Button>().onClick.AddListener(() => {
            mm.CloseMonitor();
            gameManager.StartEmergencyKitPhase();
        });

        // Open Shop Button
        GameObject shopBtn = CreateWindowsButton(dialog.transform, "ShopBtn", "AUSRÜSTUNG SHOP", new Vector2(250, -310), new Vector2(220, 60));
        shopBtn.GetComponent<Button>().onClick.AddListener(() => {
            mm.CloseMonitor();
            gameManager.StartShopPhase();
        });
    }

    private void SetupEmergencyKitPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "EmergencyKitPanel", new Color(0, 0, 0, 0.85f));
        panel.SetActive(false);
        gameManager.emergencyKitPanel = panel;
        
        EmergencyKitManager ekm = panel.AddComponent<EmergencyKitManager>();
        gameManager.emergencyKitManager = ekm;
        ekm.gameManager = gameManager;
        ekm.emergencyKitPanel = panel;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "EmergencyKitDialog", "Notfallkoffer-Planer.exe", Vector2.zero, new Vector2(850, 750));

        // Shelf Frame / Item Container (where items spawn)
        GameObject shelfFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "ShelfFrame", new Vector2(0, 140), new Vector2(750, 320));
        UIFactory.SetupImage(shelfFrame, winInnerFrameSprite, false);
        
        // Inside the shelf, create a container for grid spawning
        GameObject itemsGrid = UIFactory.CreateUIElement(shelfFrame.transform as RectTransform, "ItemsGrid", Vector2.zero, new Vector2(750, 320));
        ekm.itemContainer = itemsGrid.GetComponent<RectTransform>();

        // Left Target: Emergency Kit Box (Drop Zone)
        GameObject kitFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "KitFrame", new Vector2(-190, -70), new Vector2(340, 140));
        UIFactory.SetupImage(kitFrame, winInnerFrameSprite, false);
        Text kitLabel = UIFactory.CreateText(kitFrame.transform, "KitLabel", "💼 IN DEN KOFFER", Vector2.zero, 24, TextAnchor.MiddleCenter);
        kitLabel.color = new Color(0.1f, 0.4f, 0.1f);
        kitLabel.fontStyle = FontStyle.Bold;
        DropZone kitDZ = kitFrame.AddComponent<DropZone>();
        ekm.kitDropZone = kitDZ;

        // Right Target: Trash Bin (Drop Zone)
        GameObject binFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "BinFrame", new Vector2(190, -70), new Vector2(340, 140));
        UIFactory.SetupImage(binFrame, winInnerFrameSprite, false);
        Text binLabel = UIFactory.CreateText(binFrame.transform, "BinLabel", "🗑️ MÜLLEIMER", Vector2.zero, 24, TextAnchor.MiddleCenter);
        binLabel.color = new Color(0.6f, 0.1f, 0.1f);
        binLabel.fontStyle = FontStyle.Bold;
        DropZone binDZ = binFrame.AddComponent<DropZone>();
        ekm.binDropZone = binDZ;

        // Progress Text Label
        Text progText = UIFactory.CreateText(dialog.transform, "ProgressLabel", "Einsortiert: 0 / 10", new Vector2(0, -160), 22, TextAnchor.MiddleCenter);
        progText.color = Color.black;
        progText.fontStyle = FontStyle.Bold;
        ekm.progressText = progText;

        // Explanation / Feedback Frame
        GameObject feedbackFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "FeedbackFrame", new Vector2(0, -225), new Vector2(800, 140));
        UIFactory.SetupImage(feedbackFrame, winInnerFrameSprite, false);

        Text fbTitle = UIFactory.CreateText(feedbackFrame.transform, "FBTitle", "NOTFALLKOFFER PLANER", new Vector2(0, 45), 22, TextAnchor.MiddleCenter);
        fbTitle.color = Color.black;
        fbTitle.fontStyle = FontStyle.Bold;
        fbTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(780, 30);
        ekm.feedbackTitleText = fbTitle;

        Text fbBody = UIFactory.CreateText(feedbackFrame.transform, "FBBody", "...", new Vector2(0, -15), 16, TextAnchor.MiddleCenter);
        fbBody.color = Color.black;
        fbBody.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 90);
        ekm.feedbackBodyText = fbBody;

        // Finish button at the bottom
        GameObject finishBtnObj = CreateWindowsButton(dialog.transform, "FinishBtn", "BEWERTUNG ABSCHLIESSEN", new Vector2(0, -315), new Vector2(350, 50));
        Button finishBtn = finishBtnObj.GetComponent<Button>();
        finishBtn.onClick.AddListener(() => ekm.OnFinishButtonPressed());
        ekm.finishButton = finishBtn;
    }

    private void SetupTriagePanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "TriagePanel", new Color(0, 0, 0, 0.8f));
        panel.SetActive(false);
        gameManager.triagePanel = panel;
        
        TriageManager tm = panel.AddComponent<TriageManager>();
        gameManager.triageManager = tm;
        tm.gameManager = gameManager;
        tm.triagePanel = panel;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "TriageDialog", "Triage-System.exe", Vector2.zero, new Vector2(1000, 850));

        // Illustration in Inner Frame
        GameObject imgFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "ImageFrame", new Vector2(0, 180), new Vector2(900, 400));
        UIFactory.SetupImage(imgFrame, winInnerFrameSprite, false);

        GameObject groupObj = UIFactory.CreateUIElement(imgFrame.transform as RectTransform, "TriageIllustration", Vector2.zero, new Vector2(880, 380));
        groupObj.GetComponent<Image>().sprite = triageGroupSprite;
        groupObj.GetComponent<Image>().preserveAspect = true;

        // Info Text in Inner Frame
        GameObject infoFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InfoBox", new Vector2(0, -100), new Vector2(900, 120));
        UIFactory.SetupImage(infoFrame, winInnerFrameSprite, false);
        tm.victimInfoText = UIFactory.CreateText(infoFrame.transform, "InfoText", "...", Vector2.zero, 30, TextAnchor.MiddleCenter);
        tm.victimInfoText.color = Color.black;
        tm.victimInfoText.fontStyle = FontStyle.Bold;

        // Windows Buttons Container
        GameObject btnContainer = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Buttons", new Vector2(0, -250), new Vector2(900, 100));
        btnContainer.GetComponent<Image>().color = new Color(0, 0, 0, 0); // Transparent container
        
        GameObject blackBtn = CreateWindowsButton(btnContainer.transform, "BlackBtn", "SCHWARZ", new Vector2(-345, 0), new Vector2(210, 70));
        blackBtn.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f); // Dark gray tint
        blackBtn.GetComponentInChildren<Text>().color = Color.white; // White text for contrast
        blackBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(3));

        GameObject redBtn = CreateWindowsButton(btnContainer.transform, "RedBtn", "ROT (Akut)", new Vector2(-115, 0), new Vector2(210, 70));
        redBtn.GetComponent<Image>().color = new Color(1f, 0.6f, 0.6f); // Light red tint
        redBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(0));

        GameObject yellowBtn = CreateWindowsButton(btnContainer.transform, "YellowBtn", "GELB (Schwer)", new Vector2(115, 0), new Vector2(210, 70));
        yellowBtn.GetComponent<Image>().color = new Color(1f, 1f, 0.6f); // Light yellow tint
        yellowBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(1));

        GameObject greenBtn = CreateWindowsButton(btnContainer.transform, "GreenBtn", "GRÜN (Leicht)", new Vector2(345, 0), new Vector2(210, 70));
        greenBtn.GetComponent<Image>().color = new Color(0.6f, 1f, 0.6f); // Light green tint
        greenBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(2));
    }

    private void SetupHeatstrokePanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "HeatstrokePanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.heatstrokePanel = panel;
        
        HeatstrokeManager hm = panel.AddComponent<HeatstrokeManager>();
        gameManager.heatstrokeManager = hm;
        hm.gameManager = gameManager;
        hm.heatstrokePanel = panel;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "HeatstrokeDialog", "Hitzeschlag-Assistent.exe", Vector2.zero, new Vector2(1000, 750));

        // Inner Frame for Instructions & Avatar
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InnerFrame", new Vector2(0, 160), new Vector2(900, 240));
        UIFactory.SetupImage(innerFrame, winInnerFrameSprite, false);

        GameObject avatarObj = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Avatar", new Vector2(-320, 0), new Vector2(200, 200));
        Image avImg = avatarObj.GetComponent<Image>();
        avImg.sprite = heatstrokePersonSprite;
        avImg.preserveAspect = true;
        avImg.raycastTarget = false;

        hm.instructionText = UIFactory.CreateText(innerFrame.transform, "Instructions", "...", new Vector2(100, 0), 24, TextAnchor.MiddleLeft);
        hm.instructionText.color = Color.black;
        hm.instructionText.fontStyle = FontStyle.Bold;
        hm.instructionText.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 220);
        hm.instructionText.raycastTarget = false;

        // Windows Style Buttons
        GameObject b1 = CreateWindowsButton(dialog.transform, "ShadowBtn", "IN DEN SCHATTEN BRINGEN", new Vector2(0, -60), new Vector2(600, 60));
        hm.shadowButton = b1.GetComponent<Button>();
        hm.shadowButton.onClick.AddListener(() => hm.MoveToShadow());

        GameObject b2 = CreateWindowsButton(dialog.transform, "LegsBtn", "BEINE HOCHLAGERN", new Vector2(0, -140), new Vector2(600, 60));
        hm.legsButton = b2.GetComponent<Button>();
        hm.legsButton.onClick.AddListener(() => hm.RaiseLegs());

        GameObject b3 = CreateWindowsButton(dialog.transform, "WaterBtn", "WASSER GEBEN (SCHLÜCKCHEN)", new Vector2(0, -220), new Vector2(600, 60));
        hm.waterButton = b3.GetComponent<Button>();
        hm.waterButton.onClick.AddListener(() => hm.GiveWater());
    }

    private void SetupChokingPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "ChokingPanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.chokingPanel = panel;
        
        ChokingManager cm = panel.AddComponent<ChokingManager>();
        gameManager.chokingManager = cm;
        cm.gameManager = gameManager;
        cm.chokingPanel = panel;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "ChokingDialog", "Heimlich-Assistent.exe", Vector2.zero, new Vector2(1000, 750));

        // Inner Frame for Instructions & Avatar
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InnerFrame", new Vector2(0, 160), new Vector2(900, 260));
        UIFactory.SetupImage(innerFrame, winInnerFrameSprite, false);

        GameObject avatarObj = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Avatar", new Vector2(-320, 0), new Vector2(220, 220));
        avatarObj.GetComponent<Image>().sprite = chokingPersonSprite;
        avatarObj.GetComponent<Image>().preserveAspect = true;

        cm.instructionText = UIFactory.CreateText(innerFrame.transform, "Instructions", "...", new Vector2(100, 0), 28, TextAnchor.MiddleLeft);
        cm.instructionText.color = Color.black;
        cm.instructionText.fontStyle = FontStyle.Bold;

        // Slider in retro frame
        GameObject sliderFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "SliderFrame", new Vector2(0, -110), new Vector2(700, 160));
        UIFactory.SetupImage(sliderFrame, winInnerFrameSprite, false);

        Text sliderTxt = UIFactory.CreateText(sliderFrame.transform, "Label", "Stoßstärke (LEERTASTE im grünen Bereich drücken)", new Vector2(0, 50), 24, TextAnchor.MiddleCenter);
        sliderTxt.color = Color.black;
        sliderTxt.fontStyle = FontStyle.Bold;

        Slider fSlider = UIFactory.CreateSlider(sliderFrame.transform, "ForceSlider", new Vector2(0, -20), new Vector2(600, 45), Color.red, null, null);
        cm.forceSlider = fSlider;
        cm.fillImage = fSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
    }

    private void SetupPoisonPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "PoisonPanel", new Color(0, 0.08f, 0.02f, 0.92f));
        panel.SetActive(false);
        gameManager.poisonPanel = panel;

        PoisonManager pm = panel.AddComponent<PoisonManager>();
        gameManager.poisonManager = pm;
        pm.gameManager = gameManager;
        pm.poisonPanel = panel;

        // Main dialog window
        GameObject dialog = CreateWindowsDialog(panel.transform, "PoisonDialog",
            "Vergiftungs-Assistent.exe", Vector2.zero, new Vector2(900, 660));

        // Shared instruction text (top)
        GameObject infoFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform,
            "InfoFrame", new Vector2(0, 235), new Vector2(840, 80));
        UIFactory.SetupImage(infoFrame, winInnerFrameSprite, false);
        Text instTxt = UIFactory.CreateText(infoFrame.transform, "InstructionText",
            "...", Vector2.zero, 20, TextAnchor.MiddleCenter);
        instTxt.color = Color.black;
        instTxt.fontStyle = FontStyle.Bold;
        instTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(810, 72);
        pm.instructionText = instTxt;

        // === PHASE 1: Examine button ===
        GameObject examineObj = CreateWindowsButton(dialog.transform, "ExamineBtn",
            "PILZE UNTERSUCHEN", new Vector2(0, 130), new Vector2(500, 65));
        pm.examineButton = examineObj.GetComponent<Button>();
        pm.examineButton.onClick.AddListener(() => pm.OnExamineClicked());

        // === PHASE 2: Phone dialer ===
        GameObject phonePanel = UIFactory.CreateUIElement(dialog.transform as RectTransform,
            "PhonePanel", new Vector2(0, 20), new Vector2(820, 430));
        UIFactory.SetupImage(phonePanel, winInnerFrameSprite, false);
        phonePanel.SetActive(false);
        pm.phonePanel = phonePanel;

        // Status hint
        Text phoneStat = UIFactory.CreateText(phonePanel.transform, "PhoneStatus",
            "Waehle den Giftnotruf: 1 9 2 4 0", new Vector2(0, 175), 18, TextAnchor.MiddleCenter);
        phoneStat.color = new Color(0.1f, 0.35f, 0.1f);
        phoneStat.fontStyle = FontStyle.Bold;
        phoneStat.supportRichText = true;
        phoneStat.GetComponent<RectTransform>().sizeDelta = new Vector2(780, 30);
        pm.phoneStatusText = phoneStat;

        // Dial display
        GameObject displayFrame = UIFactory.CreateUIElement(phonePanel.GetComponent<RectTransform>(),
            "DialFrame", new Vector2(0, 115), new Vector2(500, 60));
        UIFactory.SetupImage(displayFrame, winInnerFrameSprite, false);
        displayFrame.GetComponent<Image>().color = new Color(0.85f, 0.95f, 0.85f);
        Text dialDisp = UIFactory.CreateText(displayFrame.transform, "DialDisplay",
            "_ _ _ _ _", Vector2.zero, 28, TextAnchor.MiddleCenter);
        dialDisp.color = new Color(0.05f, 0.25f, 0.05f);
        dialDisp.fontStyle = FontStyle.Bold;
        dialDisp.GetComponent<RectTransform>().sizeDelta = new Vector2(470, 52);
        pm.dialDisplay = dialDisp;

        // Digit keypad 3x3 + 0
        pm.digitButtons = new Button[10];
        string[] digits = { "1","2","3","4","5","6","7","8","9","0" };
        Vector2[] keyPos = {
            new Vector2(-150, 40), new Vector2(0, 40),   new Vector2(150, 40),
            new Vector2(-150,-40), new Vector2(0,-40),   new Vector2(150,-40),
            new Vector2(-150,-120),new Vector2(0,-120),  new Vector2(150,-120),
            new Vector2(-75,-200)
        };
        for (int i = 0; i < 10; i++)
        {
            string d = digits[i];
            GameObject keyObj = UIFactory.CreateButton(phonePanel.GetComponent<RectTransform>(),
                "Key_" + d, d, keyPos[i], new Vector2(110, 62), winButtonSprite);
            Text kt = keyObj.GetComponentInChildren<Text>();
            if (kt != null) { kt.fontSize = 26; kt.fontStyle = FontStyle.Bold; kt.color = Color.black; }
            Button kb = keyObj.GetComponent<Button>();
            pm.digitButtons[i] = kb;
            kb.onClick.AddListener(() => pm.OnDigitPressed(d));
        }

        // Delete button
        GameObject delObj = UIFactory.CreateButton(phonePanel.GetComponent<RectTransform>(),
            "DeleteBtn", "<", new Vector2(75, -200), new Vector2(110, 62), winButtonSprite);
        delObj.GetComponent<Image>().color = new Color(1f, 0.88f, 0.88f);
        Text delT = delObj.GetComponentInChildren<Text>();
        if (delT != null) { delT.fontSize = 22; delT.color = new Color(0.6f, 0.1f, 0.1f); }
        pm.deleteButton = delObj.GetComponent<Button>();
        pm.deleteButton.onClick.AddListener(() => pm.OnDeletePressed());

        // Call button (green)
        GameObject callObj = UIFactory.CreateButton(phonePanel.GetComponent<RectTransform>(),
            "CallDialBtn", "ANRUFEN", new Vector2(220, -200), new Vector2(200, 62), winButtonSprite);
        callObj.GetComponent<Image>().color = new Color(0.7f, 1f, 0.7f);
        Text callT = callObj.GetComponentInChildren<Text>();
        if (callT != null) { callT.fontSize = 18; callT.fontStyle = FontStyle.Bold; callT.color = new Color(0f, 0.35f, 0f); }
        pm.callDialButton = callObj.GetComponent<Button>();
        pm.callDialButton.onClick.AddListener(() => pm.OnCallPressed());

        // === PHASE 3: Water panel ===
        GameObject waterPanel = UIFactory.CreateUIElement(dialog.transform as RectTransform,
            "WaterPanel", new Vector2(0, 60), new Vector2(820, 280));
        UIFactory.SetupImage(waterPanel, winInnerFrameSprite, false);
        waterPanel.SetActive(false);
        pm.waterPanel = waterPanel;

        Text waterInfo = UIFactory.CreateText(waterPanel.transform, "WaterInfo",
            "Der Giftnotruf raet:\nStilles Wasser oder Tee in kleinen Schlueckchen geben.\nKEIN Erbrechen ausloesen!",
            new Vector2(0, 55), 19, TextAnchor.MiddleCenter);
        waterInfo.color = new Color(0.05f, 0.2f, 0.45f);
        waterInfo.fontStyle = FontStyle.Bold;
        waterInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 120);

        GameObject waterObj = CreateWindowsButton(waterPanel.transform, "WaterBtn",
            "WASSER GEBEN", new Vector2(0, -80), new Vector2(400, 65));
        waterObj.GetComponent<Image>().color = new Color(0.85f, 0.93f, 1f);
        pm.waterButton = waterObj.GetComponent<Button>();
        pm.waterButton.onClick.AddListener(() => pm.OnWaterClicked());
    }

    private void SetupElectricShockPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "ElectricShockPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        panel.SetActive(false);
        gameManager.shockPanel = panel;
        
        ElectricShockManager em = panel.AddComponent<ElectricShockManager>();
        gameManager.shockManager = em;
        em.shockPanel = panel;
        em.gameManager = gameManager;

        GameObject dialog = CreateWindowsDialog(panel.transform, "ShockDialog", "Sicherungskasten", Vector2.zero, new Vector2(800, 600));

        Text instTxt = UIFactory.CreateText(dialog.transform, "InstructionText", "Folge den Anweisungen!", new Vector2(0, 200), 28, TextAnchor.MiddleCenter);
        instTxt.color = Color.black;
        em.instructionText = instTxt;

        // Visual Layout: Fuse Box Container
        GameObject fuseContainer = UIFactory.CreateUIElement(dialog.transform as RectTransform, "FuseBox", new Vector2(0, -50), new Vector2(600, 250));
        UIFactory.SetupImage(fuseContainer, winInnerFrameSprite, false);

        // Create 3 Fuses
        em.fuseButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = new Vector2(-180 + (i * 180), 0);
            GameObject fuseBtnObj = UIFactory.CreateButton(fuseContainer.GetComponent<RectTransform>(), "Fuse_" + i, "I", pos, new Vector2(100, 180), null);
            Button fuseBtn = fuseBtnObj.GetComponent<Button>();
            
            Text btnText = fuseBtnObj.GetComponentInChildren<Text>();
            btnText.fontSize = 50; // Big text for the switch state
            
            em.fuseButtons[i] = fuseBtn;
        }
    }

    private void SetupBurnPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "BurnPanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.burnPanel = panel;
        
        BurnManager bm = panel.AddComponent<BurnManager>();
        gameManager.burnManager = bm;
        bm.gameManager = gameManager;
        bm.burnPanel = panel;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "BurnDialog", "Kühlungs-Assistent.exe", Vector2.zero, new Vector2(1000, 750));

        // Left Column: Arm Illustration in Window Frame
        GameObject leftFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "LeftFrame", new Vector2(-220, 20), new Vector2(420, 520));
        UIFactory.SetupImage(leftFrame, winInnerFrameSprite, false);

        GameObject arm = UIFactory.CreateUIElement(leftFrame.transform as RectTransform, "BurnArm", Vector2.zero, new Vector2(380, 480));
        arm.GetComponent<Image>().sprite = burnArmSprite;
        arm.GetComponent<Image>().preserveAspect = true;

        // Right Column: Instructions
        GameObject instructionsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InstructionsFrame", new Vector2(230, 210), new Vector2(420, 140));
        UIFactory.SetupImage(instructionsFrame, winInnerFrameSprite, false);
        bm.instructionText = UIFactory.CreateText(instructionsFrame.transform, "Instructions", "...", Vector2.zero, 26, TextAnchor.MiddleCenter);
        bm.instructionText.color = Color.black;
        bm.instructionText.fontStyle = FontStyle.Bold;

        // Temperature Control Frame
        GameObject tempFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "TempFrame", new Vector2(230, 30), new Vector2(420, 140));
        UIFactory.SetupImage(tempFrame, winInnerFrameSprite, false);

        Text tempTxt = UIFactory.CreateText(tempFrame.transform, "Label", "Wasser-Temperatur regeln", new Vector2(0, 40), 22, TextAnchor.MiddleCenter);
        tempTxt.color = Color.black;
        tempTxt.fontStyle = FontStyle.Bold;

        Slider tSlider = UIFactory.CreateSlider(tempFrame.transform, "TempSlider", new Vector2(0, -20), new Vector2(360, 35), Color.blue, null, null);
        bm.temperatureSlider = tSlider;
        bm.temperatureLabelText = tempTxt;
        bm.temperatureFill = tSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();

        // Progress Control Frame
        GameObject progressFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "ProgressFrame", new Vector2(230, -150), new Vector2(420, 140));
        UIFactory.SetupImage(progressFrame, winInnerFrameSprite, false);

        Text progressTxt = UIFactory.CreateText(progressFrame.transform, "Label", "Fortschritt Kühlung (100% Ziel)", new Vector2(0, 40), 22, TextAnchor.MiddleCenter);
        progressTxt.color = Color.black;
        progressTxt.fontStyle = FontStyle.Bold;

        Slider pSlider = UIFactory.CreateSlider(progressFrame.transform, "ProgressSlider", new Vector2(0, -20), new Vector2(360, 30), Color.green, null, null);
        bm.progressSlider = pSlider;
    }

    private void SetupAEDPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "AEDPanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.aedPanel = panel;
        
        AEDManager am = panel.AddComponent<AEDManager>();
        gameManager.aedManager = am;
        am.gameManager = gameManager;
        am.aedPanel = panel;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "AEDDialog", "Defibrillator-Assistent.exe", Vector2.zero, new Vector2(1000, 850));

        // Instructions inside an inner frame at the top
        GameObject infoFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InfoBox", new Vector2(0, 320), new Vector2(900, 100));
        UIFactory.SetupImage(infoFrame, winInnerFrameSprite, false);
        am.instructionText = UIFactory.CreateText(infoFrame.transform, "Instructions", "...", Vector2.zero, 28, TextAnchor.MiddleCenter);
        am.instructionText.color = Color.black;
        am.instructionText.fontStyle = FontStyle.Bold;

        // Victim body visualization
        GameObject victim = UIFactory.CreateUIElement(dialog.transform as RectTransform, "VictimBody", new Vector2(120, -50), new Vector2(450, 600));
        victim.GetComponent<Image>().sprite = aedBodySprite;
        victim.GetComponent<Image>().preserveAspect = true;
        victim.GetComponent<Image>().raycastTarget = false; // Prevents blocking pads
        
        // Target Zones (aligned inside dialog coordinates)
        GameObject t1 = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Target1", new Vector2(120 + 108, -50 + 154), new Vector2(90, 90));
        t1.GetComponent<Image>().color = new Color(0, 1, 0, 0.3f);
        am.target1 = t1.GetComponent<RectTransform>();

        GameObject t2 = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Target2", new Vector2(120 - 108, -50 - 86), new Vector2(90, 90));
        t2.GetComponent<Image>().color = new Color(0, 1, 0, 0.3f);
        am.target2 = t2.GetComponent<RectTransform>();

        // Pads on the left column inside the dialog
        GameObject p1 = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Pad1", new Vector2(-320, 50), new Vector2(120, 120));
        p1.GetComponent<Image>().sprite = aedPadSprite;
        am.pad1 = p1.GetComponent<RectTransform>();

        GameObject p2 = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Pad2", new Vector2(-320, -150), new Vector2(120, 120));
        p2.GetComponent<Image>().sprite = aedPadSprite;
        am.pad2 = p2.GetComponent<RectTransform>();

        // Shock Button (Classic Windows Button styled in red)
        GameObject shockBtn = CreateWindowsButton(dialog.transform, "ShockButton", "SCHOCK AUSLÖSEN", new Vector2(-320, -320), new Vector2(240, 60));
        shockBtn.GetComponent<Image>().color = new Color(1f, 0.6f, 0.6f); // Light red tint
        am.shockButton = shockBtn.GetComponent<Button>();
        am.shockButton.onClick.AddListener(() => am.OnShockPressed());
    }

    private void SetupCallPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "CallPanel", new Color(0, 0, 0, 0.95f));
        panel.SetActive(false);
        gameManager.callPanel = panel;
        
        EmergencyCallManager ecm = panel.AddComponent<EmergencyCallManager>();
        gameManager.callManager = ecm;
        ecm.gameManager = gameManager;
        ecm.callPanel = panel;
        ecm.telephoneSound = telephoneSound;
        ecm.muffledTalkSound = muffledTalkSound;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(panel.transform, "CallDialog", "112 - Leitstelle.exe", Vector2.zero, new Vector2(1000, 750));

        // Inner Frame for Dispatcher Text
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InnerFrame", new Vector2(0, 150), new Vector2(900, 200));
        UIFactory.SetupImage(innerFrame, winInnerFrameSprite, false);

        // Dispatcher Avatar inside Frame (moved to the right)
        GameObject avatarObj = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Avatar", new Vector2(350, 0), new Vector2(160, 160));
        avatarObj.GetComponent<Image>().sprite = dispatcherSprite;
        avatarObj.GetComponent<Image>().preserveAspect = true;
        
        ecm.dispatcherText = UIFactory.CreateText(innerFrame.transform, "Text", "Verbindung...", new Vector2(-60, 0), 32, TextAnchor.MiddleCenter);
        ecm.dispatcherText.color = Color.black;
        ecm.dispatcherText.fontStyle = FontStyle.Bold;

        // LIVE indicator inside Inner Frame (moved to the left)
        GameObject live = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Live", new Vector2(-380, 70), new Vector2(60, 25));
        live.GetComponent<Image>().color = Color.red;
        UIFactory.CreateText(live.transform, "T", "LIVE", Vector2.zero, 14, TextAnchor.MiddleCenter).color = Color.white;
        
        ecm.answerButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = CreateWindowsButton(dialog.transform, "Answer_" + i, "Antwort", new Vector2(0, -60 - (i * 90)), new Vector2(800, 70));
            ecm.answerButtons[i] = btnObj.GetComponent<Button>();
            int idx = i;
            ecm.answerButtons[idx].onClick.AddListener(() => ecm.OnAnswerSelected(idx));
        }
    }

    private void SetupStoryPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "StoryPanel", Color.black);
        panel.SetActive(false);
        gameManager.storyPanel = panel;
        
        IntroStoryManager ism = panel.AddComponent<IntroStoryManager>();
        ism.gameManager = gameManager;
        
        // Fullscreen Story Image
        GameObject imgObj = UIFactory.CreateUIElement(panel.transform as RectTransform, "StoryImage", Vector2.zero, new Vector2(1920, 1080));
        ism.storyImage = imgObj.GetComponent<Image>();
        ism.storyImage.preserveAspect = true;

        // "Moderate Windows" Bottom Dialog Box
        GameObject textBox = UIFactory.CreateUIElement(panel.transform as RectTransform, "TextBox", new Vector2(0, -380), new Vector2(1600, 250));
        UIFactory.SetupImage(textBox, winBaseSprite, false);
        ism.storyTextBox = textBox;
        
        // Inner Frame for the Text
        GameObject innerFrame = UIFactory.CreateUIElement(textBox.transform as RectTransform, "InnerFrame", Vector2.zero, new Vector2(1560, 210));
        UIFactory.SetupImage(innerFrame, winInnerFrameSprite, false);

        ism.storyText = UIFactory.CreateText(innerFrame.transform, "Text", "Ein neuer Tag beginnt...", Vector2.zero, 38, TextAnchor.MiddleCenter);
        ism.storyText.color = Color.black;
        ism.storyText.fontStyle = FontStyle.Bold;

        ism.storyVideoClips = storyVideoClips;

        // Skip Button as Windows Button (for static slideshow)
        GameObject skipBtn = CreateWindowsButton(textBox.transform, "SkipBtn", "ÜBERSPRINGEN", new Vector2(600, -80), new Vector2(220, 60));
        skipBtn.GetComponent<Button>().onClick.AddListener(() => ism.SkipStory());

        // Dedicated Corner Skip Button specifically for the Fullscreen Video
        GameObject videoSkipBtn = CreateWindowsButton(panel.transform, "VideoSkipBtn", "ÜBERSPRINGEN", new Vector2(800, -460), new Vector2(240, 70));
        videoSkipBtn.SetActive(false); // Hide by default, will be activated only when video starts
        videoSkipBtn.GetComponent<Button>().onClick.AddListener(() => ism.SkipStory());
        ism.videoSkipButton = videoSkipBtn;
    }

    private void SetupShopPanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "ShopPanel", new Color(0, 0, 0, 0.8f));
        panel.SetActive(false);
        gameManager.shopPanel = panel;
        shopManager = panel.AddComponent<ShopManager>();
        shopManager.gameManager = gameManager;

        GameObject dialog = CreateWindowsDialog(panel.transform, "ShopDialog", "Ausrüstung-Shop.exe", Vector2.zero, new Vector2(900, 800));

        GameObject coinsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "CoinsFrame", new Vector2(0, 300), new Vector2(300, 60));
        UIFactory.SetupImage(coinsFrame, winInnerFrameSprite, false);
        shopManager.coinsText = UIFactory.CreateText(coinsFrame.transform, "Text", "Coins: 0", Vector2.zero, 30, TextAnchor.MiddleCenter);
        shopManager.coinsText.color = Color.black;
        shopManager.coinsText.fontStyle = FontStyle.Bold;

        // Item Grid
        float startY = 160;
        CreateShopItem(dialog.transform, "Steady Rhythm", "CPR-Takt verlangsamen", "50", new Vector2(0, startY), () => shopManager.BuySteadyRhythm());
        CreateShopItem(dialog.transform, "Sharp Eye", "Quiz-Hilfe aktivieren", "30", new Vector2(0, startY - 120), () => shopManager.BuySharpEye());
        CreateShopItem(dialog.transform, "Pro Bandage", "Verband-Fehlertoleranz+", "40", new Vector2(0, startY - 240), () => shopManager.BuyProBandage());
        
        // Skin Colors (Color Pickers wrapped in Windows Buttons)
        GameObject skinRow = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Skins", new Vector2(0, -220), new Vector2(600, 100));
        CreateColorButton(skinRow.transform, Color.red, "#FF0000", new Vector2(-150, 0));
        CreateColorButton(skinRow.transform, Color.blue, "#0000FF", new Vector2(-50, 0));
        CreateColorButton(skinRow.transform, Color.green, "#00FF00", new Vector2(50, 0));
        CreateColorButton(skinRow.transform, Color.yellow, "#FFFF00", new Vector2(150, 0));

        GameObject closeBtn = CreateWindowsButton(dialog.transform, "CloseBtn", "ZURÜCK", new Vector2(0, -340), new Vector2(300, 60));
        closeBtn.GetComponent<Button>().onClick.AddListener(() => gameManager.CloseShop());
    }

    public void CreateShopItem(Transform parent, string title, string desc, string price, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject item = UIFactory.CreateUIElement(parent as RectTransform, title, pos, new Vector2(700, 100));
        UIFactory.SetupImage(item, winInnerFrameSprite, false);
        
        Text titleTxt = UIFactory.CreateText(item.transform, "Title", title, new Vector2(-150, 20), 28, TextAnchor.MiddleLeft);
        if (titleTxt != null) { titleTxt.color = Color.black; titleTxt.fontStyle = FontStyle.Bold; titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40); }
        
        Text descTxt = UIFactory.CreateText(item.transform, "Desc", desc, new Vector2(-150, -20), 18, TextAnchor.MiddleLeft);
        if (descTxt != null) { descTxt.color = new Color(0.2f, 0.2f, 0.2f); descTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40); }
        
        GameObject buyBtn = CreateWindowsButton(item.transform, "BuyBtn", price + " C", new Vector2(250, 0), new Vector2(140, 60));
        buyBtn.GetComponent<Button>().onClick.AddListener(action);
    }

    public void CreateColorButton(Transform parent, Color col, string hex, Vector2 pos)
    {
        GameObject btn = CreateWindowsButton(parent, "Color_" + hex, "", pos, new Vector2(70, 70));
        GameObject colorBlock = UIFactory.CreateUIElement(btn.transform as RectTransform, "Color", Vector2.zero, new Vector2(46, 46));
        colorBlock.GetComponent<Image>().color = col;
        btn.GetComponent<Button>().onClick.AddListener(() => shopManager.BuyColor(hex));
    }

    private void SetupPausePanel(RectTransform parent)
    {
        GameObject pause = UIFactory.CreateFullscreenPanel(parent, "PausePanel", new Color(0, 0, 0, 0.7f));
        pause.SetActive(false);
        gameManager.pausePanel = pause;

        // Windows Dialog Container for main pause controls
        GameObject mainContainer = CreateWindowsDialog(pause.transform, "PauseDialog", "Pause.exe", Vector2.zero, new Vector2(500, 420));

        GameObject resumeBtn = CreateWindowsButton(mainContainer.transform, "ResumeBtn", "FORTSETZEN", new Vector2(0, 70), new Vector2(350, 65));
        resumeBtn.GetComponent<Button>().onClick.AddListener(() => gameManager.TogglePause());

        GameObject settingsBtn = CreateWindowsButton(mainContainer.transform, "SettingsBtn", "EINSTELLUNGEN", new Vector2(0, -20), new Vector2(350, 65));

        GameObject quitBtn = CreateWindowsButton(mainContainer.transform, "QuitBtn", "HAUPTMENÜ", new Vector2(0, -110), new Vector2(350, 65));
        quitBtn.GetComponent<Button>().onClick.AddListener(() => gameManager.QuitToMenu());

        // --- RETRO SETTINGS DIALOG IN PAUSE ---
        GameObject settingsDialog = CreateWindowsDialog(pause.transform, "PauseSettingsDialog", "System-Einstellungen.exe", Vector2.zero, new Vector2(850, 680));
        settingsDialog.SetActive(false); // Hidden by default

        UIFactory.CreateText(settingsDialog.transform, "SettingsHeader", "SYSTEM-EINSTELLUNGEN", new Vector2(0, 260), 32, TextAnchor.MiddleCenter).color = Color.black;

        // BGM Slider inside Dialog (Left side-by-side)
        GameObject bgmFrame = UIFactory.CreateUIElement(settingsDialog.transform as RectTransform, "BGMFrame", new Vector2(-190, 160), new Vector2(360, 95));
        UIFactory.SetupImage(bgmFrame, winInnerFrameSprite, false);
        Text bgmTxt = UIFactory.CreateText(bgmFrame.transform, "Label", "Musik Lautstärke", new Vector2(0, 18), 24, TextAnchor.MiddleCenter);
        bgmTxt.color = Color.black;
        bgmTxt.fontStyle = FontStyle.Bold;
        
        Slider bgmS = UIFactory.CreateSlider(bgmFrame.transform, "BGMSlider", new Vector2(0, -18), new Vector2(320, 25), new Color(0, 0, 0.8f));

        // SFX Slider inside Dialog (Right side-by-side)
        GameObject sfxFrame = UIFactory.CreateUIElement(settingsDialog.transform as RectTransform, "SFXFrame", new Vector2(190, 160), new Vector2(360, 95));
        UIFactory.SetupImage(sfxFrame, winInnerFrameSprite, false);
        Text sfxTxt = UIFactory.CreateText(sfxFrame.transform, "Label", "Effekt Lautstärke", new Vector2(0, 18), 24, TextAnchor.MiddleCenter);
        sfxTxt.color = Color.black;
        sfxTxt.fontStyle = FontStyle.Bold;
        
        Slider sfxS = UIFactory.CreateSlider(sfxFrame.transform, "SFXSlider", new Vector2(0, -18), new Vector2(320, 25), new Color(0, 0.6f, 0));

        // Textual Tutorial Frame inside Dialog
        GameObject tutorialFrame = UIFactory.CreateUIElement(settingsDialog.transform as RectTransform, "TutorialFrame", new Vector2(0, -25), new Vector2(740, 210));
        UIFactory.SetupImage(tutorialFrame, winInnerFrameSprite, false);

        string pauseTutorialString = "<b>SPIEL-ANLEITUNG & STEUERUNG</b>\n" +
                                     "• <b>WASD / Pfeiltasten:</b> Bewege deine Spielfigur durch den Park.\n" +
                                     "• <b>E / Enter:</b> Interagiere mit verletzten Personen oder Gegenständen.\n" +
                                     "• <b>M-Taste:</b> Öffne dein Tablet für Badges, Checkliste und den Shop.\n" +
                                     "• <b>ESC / P-Taste:</b> Pausiere das Spiel und öffne das Hauptmenü.\n" +
                                     "• <b>Medizin-Taschen (+):</b> Sammle sie im Park, um Erste-Hilfe-Ausrüstung aufzufüllen.\n" +
                                     "• <b>Das Haus:</b> Gehe zur Haustür unten im Park und drücke <b>E</b>, um das Haus zu betreten. Dort warten weitere Indoor-Szenarien auf dich!";

        Text tutTxt = UIFactory.CreateText(tutorialFrame.transform, "TutorialContent", pauseTutorialString, new Vector2(0, 0), 16, TextAnchor.UpperLeft);
        tutTxt.color = Color.black;
        tutTxt.supportRichText = true;
        tutTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 180);

        // Reset Button (Left side-by-side)
        GameObject resetBtn = CreateWindowsButton(settingsDialog.transform, "ResetButton", "FORTSCHRITT ZURÜCKSETZEN", new Vector2(-190, -180), new Vector2(360, 55));
        
        // Back Button to return to pause menu (Right side-by-side)
        GameObject backBtn = CreateWindowsButton(settingsDialog.transform, "BackButton", "ZURÜCK", new Vector2(190, -180), new Vector2(360, 55));

        // Button Actions
        settingsBtn.GetComponent<Button>().onClick.AddListener(() => {
            bgmS.value = gameManager.bgmSource != null ? gameManager.bgmSource.volume : 0.5f;
            sfxS.value = gameManager.sfxSource != null ? gameManager.sfxSource.volume : 0.8f;
            mainContainer.SetActive(false);
            settingsDialog.SetActive(true);
        });

        backBtn.GetComponent<Button>().onClick.AddListener(() => {
            mainContainer.SetActive(true);
            settingsDialog.SetActive(false);
        });

        resetBtn.GetComponent<Button>().onClick.AddListener(() => {
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScores();
            gameManager.bikeAccidentHelped = false;
            gameManager.bleedingWoundHelped = false;
            gameManager.unconsciousHelped = false;
            gameManager.burnInjuryHelped = false;
            gameManager.chokingHelped = false;
            gameManager.heatstrokeHelped = false;
            gameManager.triageHelped = false;
            gameManager.electricShockHelped = false;
            gameManager.poisoningHelped = false;
            Debug.Log("Spielfortschritt zurückgesetzt!");
        });

        bgmS.onValueChanged.AddListener((v) => {
            if (gameManager.bgmSource != null) gameManager.bgmSource.volume = v;
        });

        sfxS.onValueChanged.AddListener((v) => {
            if (gameManager.sfxSource != null) {
                gameManager.sfxSource.volume = v;
                if (gameManager.heartbeatSource != null) gameManager.heartbeatSource.volume = v;
            }
        });
    }

    private Sprite LoadSprite(string relativePath)
    {
        string fullPath = System.IO.Path.Combine(Application.dataPath, relativePath);
        if (System.IO.File.Exists(fullPath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            Debug.LogWarning("Sprite nicht gefunden unter: " + fullPath);
        }
        return null;
    }

    private void SetupMenuSkyBackground()
    {
        // Set a nice sky-blue camera background
        Camera.main.backgroundColor = new Color(0.45f, 0.72f, 1f);

        // Find existing demo01_PixelSky layers in the scene and activate parallax on them
        string[] layerNames = { "demo01_PixelSky_layer01", "demo01_PixelSky_layer02", "demo01_PixelSky_layer03" };
        float[] scrollSpeeds = { 0.04f, 0.1f, 0.2f };
        float[] yParallaxFactors = { 0f, 0f, 0f };

        for (int i = 0; i < layerNames.Length; i++)
        {
            GameObject layer = GameObject.Find(layerNames[i]);
            if (layer != null)
            {
                // Ensure the layer is at a sensible Z so it renders behind the UI
                Vector3 pos = layer.transform.position;
                pos.z = 5f + i; // Push behind
                layer.transform.position = pos;

                // Add or configure ParallaxBackground for gentle cloud drift
                ParallaxBackground pb = layer.GetComponent<ParallaxBackground>();
                if (pb == null) pb = layer.AddComponent<ParallaxBackground>();

                pb.parallaxEffectX = scrollSpeeds[i];
                pb.parallaxEffectY = yParallaxFactors[i];
                pb.repeatX = true;
                pb.repeatY = false;
            }
        }
    }

    private void SetupMenuPanel(RectTransform parent)
    {
        // Transparent panel so the parallax sky is visible behind it
        GameObject menu = UIFactory.CreateFullscreenPanel(parent, "MenuPanel", Color.clear); 
        gameManager.menuPanel = menu;
        
        // Activate animated sky layers behind the UI
        SetupMenuSkyBackground();

        MenuManager menuManager = menu.AddComponent<MenuManager>();
        menuManager.gameManager = gameManager;

        // The Main Windows Dialog
        GameObject dialog = CreateWindowsDialog(menu.transform, "FirstAidOS", "First-Aid-OS.exe", Vector2.zero, new Vector2(1000, 800));

        // Create Views filling the entire dialog
        GameObject mainView = UIFactory.CreateUIElement(dialog.transform as RectTransform, "MainView", Vector2.zero, new Vector2(950, 740));
        GameObject settingsView = UIFactory.CreateUIElement(dialog.transform as RectTransform, "SettingsView", Vector2.zero, new Vector2(950, 740));
        GameObject creditsView = UIFactory.CreateUIElement(dialog.transform as RectTransform, "CreditsView", Vector2.zero, new Vector2(950, 740));
        
        menuManager.mainView = mainView;
        menuManager.settingsView = settingsView;
        menuManager.creditsView = creditsView;

        // --- MAIN VIEW CONTENT ---
        // Glowing Title Logo inside the retro window
        GameObject logoObj = UIFactory.CreateUIElement(mainView.transform as RectTransform, "Logo", new Vector2(0, 220), new Vector2(600, 260));
        if (menuLogo != null) logoObj.GetComponent<Image>().sprite = menuLogo;
        logoObj.GetComponent<Image>().preserveAspect = true;
        logoObj.AddComponent<Outline>().effectColor = Color.cyan;
        logoObj.AddComponent<Shadow>().effectColor = new Color(0, 1, 1, 0.5f);
        StartCoroutine(AnimateLogo(logoObj.GetComponent<RectTransform>()));

        // Five buttons stacked vertically under each other
        int currentDay = PlayerPrefs.GetInt("Career_CurrentDay", 1);
        string startText = $"SCHICHT STARTEN (Tag {currentDay})";
        GameObject startBtn = CreateWindowsButton(mainView.transform, "StartButton", startText, new Vector2(0, 40), new Vector2(400, 65));
        startBtn.GetComponent<Button>().onClick.AddListener(() => gameManager.ResumeOrStartGame());
        gameManager.startButtonText = startBtn.GetComponentInChildren<Text>();

        GameObject missionsBtn = CreateWindowsButton(mainView.transform, "MissionsButton", "MISSIONEN", new Vector2(0, -30), new Vector2(400, 65));
        missionsBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowMissionsView());

        GameObject settingsBtn = CreateWindowsButton(mainView.transform, "SettingsButton", "EINSTELLUNGEN", new Vector2(0, -100), new Vector2(400, 65));
        settingsBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowSettingsView());

        GameObject creditsBtn = CreateWindowsButton(mainView.transform, "CreditsButton", "MITWIRKENDE", new Vector2(0, -170), new Vector2(400, 65));
        creditsBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowCreditsView());

        GameObject quitBtn = CreateWindowsButton(mainView.transform, "QuitButton", "BEENDEN", new Vector2(0, -240), new Vector2(400, 65));
        quitBtn.GetComponent<Button>().onClick.AddListener(() => Application.Quit());

        UIFactory.CreateText(mainView.transform, "Copyright", "© 2026 First Aid Sim - Retro Edition", new Vector2(0, -320), 18, TextAnchor.MiddleCenter).color = Color.black;

        // --- MISSIONS VIEW CONTENT ---
        GameObject missionsView = UIFactory.CreateUIElement(dialog.transform as RectTransform, "MissionsView", Vector2.zero, new Vector2(950, 740));
        menuManager.missionsView = missionsView;
        UIFactory.CreateText(missionsView.transform, "MissionsHeader", "VERFÜGBARE MISSIONEN", new Vector2(0, 310), 32, TextAnchor.MiddleCenter).color = Color.black;

        // Scrollable mission list (viewport + scrollbar — required for WebGL / itch.io)
        GameObject scrollArea = UIFactory.CreateUIElement(missionsView.transform as RectTransform, "ScrollArea", new Vector2(0, -10), new Vector2(820, 520));
        UIFactory.SetupImage(scrollArea, winInnerFrameSprite, false);

        UnityEngine.UI.ScrollRect scrollRect = scrollArea.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 80f;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollArea.transform, false);
        RectTransform viewportRT = viewport.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = new Vector2(8, 8);
        viewportRT.offsetMax = new Vector2(-32, -8);
        viewport.AddComponent<UnityEngine.UI.RectMask2D>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        viewportImage.raycastTarget = true;

        GameObject contentArea = new GameObject("Content");
        contentArea.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = contentArea.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 0);

        var contentFitter = contentArea.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        contentFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        var layout = contentArea.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 8;
        layout.padding = new RectOffset(6, 6, 6, 6);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        UnityEngine.UI.Scrollbar scrollbar = CreateMissionListScrollbar(scrollArea.transform, scrollRect);
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.Permanent;

        foreach (var mission in gameManager.availableMissions)
        {
            GameObject missionRow = UIFactory.CreateUIElement(contentRT, "Mission_" + mission.id, Vector2.zero, new Vector2(0, 60));
            var rowLayout = missionRow.AddComponent<UnityEngine.UI.LayoutElement>();
            rowLayout.preferredHeight = 60;
            rowLayout.minHeight = 60;
            
            // Name label
            Text nameTxt = UIFactory.CreateText(missionRow.transform, "Name", mission.title, new Vector2(-155, 0), 24, TextAnchor.MiddleLeft);
            nameTxt.color = Color.black;
            nameTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(370, 50);
            
            // Status label
            Text statusTxt = UIFactory.CreateText(missionRow.transform, "Status", mission.hasPlayed ? "[Abgeschlossen]" : "[Neu]", new Vector2(100, 0), 20, TextAnchor.MiddleCenter);
            statusTxt.color = mission.hasPlayed ? new Color(0, 0.5f, 0) : Color.red;
            
            // Start button
            GameObject mStartBtn = CreateWindowsButton(missionRow.transform, "Btn", "Start", new Vector2(280, 0), new Vector2(120, 50));
            var captureMission = mission;
            mStartBtn.GetComponent<Button>().onClick.AddListener(() => {
                gameManager.StartMissionDirectly(captureMission.id);
            });
        }

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

        GameObject mBackBtn = CreateWindowsButton(missionsView.transform, "BackButton", "ZURÜCK", new Vector2(0, -320), new Vector2(200, 60));
        mBackBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowMainView());
        missionsView.SetActive(false);

        // --- SETTINGS VIEW CONTENT ---
        UIFactory.CreateText(settingsView.transform, "SettingsHeader", "SYSTEM-EINSTELLUNGEN", new Vector2(0, 260), 32, TextAnchor.MiddleCenter).color = Color.black;

        // BGM Slider (Left side-by-side)
        GameObject bgmFrame = UIFactory.CreateUIElement(settingsView.transform as RectTransform, "BGMFrame", new Vector2(-210, 160), new Vector2(380, 100));
        UIFactory.SetupImage(bgmFrame, winInnerFrameSprite, false);
        Text bgmTxt = UIFactory.CreateText(bgmFrame.transform, "Label", "Musik Lautstärke", new Vector2(0, 20), 24, TextAnchor.MiddleCenter);
        bgmTxt.color = Color.black;
        bgmTxt.fontStyle = FontStyle.Bold;
        
        Slider bgmS = UIFactory.CreateSlider(bgmFrame.transform, "BGMSlider", new Vector2(0, -15), new Vector2(320, 30), new Color(0, 0, 0.8f));
        menuManager.bgmSlider = bgmS;

        // SFX Slider (Right side-by-side)
        GameObject sfxFrame = UIFactory.CreateUIElement(settingsView.transform as RectTransform, "SFXFrame", new Vector2(210, 160), new Vector2(380, 100));
        UIFactory.SetupImage(sfxFrame, winInnerFrameSprite, false);
        Text sfxTxt = UIFactory.CreateText(sfxFrame.transform, "Label", "Effekt Lautstärke", new Vector2(0, 20), 24, TextAnchor.MiddleCenter);
        sfxTxt.color = Color.black;
        sfxTxt.fontStyle = FontStyle.Bold;
        
        Slider sfxS = UIFactory.CreateSlider(sfxFrame.transform, "SFXSlider", new Vector2(0, -15), new Vector2(320, 30), new Color(0, 0.6f, 0));
        menuManager.sfxSlider = sfxS;

        // Textual Tutorial Frame
        GameObject settingsTutorialFrame = UIFactory.CreateUIElement(settingsView.transform as RectTransform, "SettingsTutorialFrame", new Vector2(0, -35), new Vector2(800, 240));
        UIFactory.SetupImage(settingsTutorialFrame, winInnerFrameSprite, false);

        string mainTutorialString = "<b>SPIEL-ANLEITUNG & STEUERUNG</b>\n" +
                                    "• <b>WASD / Pfeiltasten:</b> Bewege deine Spielfigur durch den Park.\n" +
                                    "• <b>E / Enter:</b> Interagiere mit verletzten Personen oder Gegenständen.\n" +
                                    "• <b>M-Taste:</b> Öffne dein Tablet für Badges, Checkliste und den Shop.\n" +
                                    "• <b>ESC / P-Taste:</b> Pausiere das Spiel und öffne das Hauptmenü.\n" +
                                    "• <b>Medizin-Taschen (+):</b> Sammle sie im Park, um Erste-Hilfe-Ausrüstung aufzufüllen.\n" +
                                    "• <b>Das Haus (Innenbereich):</b> Gehe zur Haustür unten im Park und drücke <b>E</b>, um das Haus zu betreten. Dort warten weitere Indoor-Szenarien (wie z.B. Verbrennung oder Vergiftung) auf dich!";

        Text mainTutTxt = UIFactory.CreateText(settingsTutorialFrame.transform, "TutorialContent", mainTutorialString, new Vector2(0, 0), 16, TextAnchor.UpperLeft);
        mainTutTxt.color = Color.black;
        mainTutTxt.supportRichText = true;
        mainTutTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 210);

        // Language Toggle (Upper Row Left)
        GameObject langBtn = CreateWindowsButton(settingsView.transform, "LanguageToggle", "SPRACHE: DE", new Vector2(-190, -180), new Vector2(360, 55));
        langBtn.GetComponent<Button>().onClick.AddListener(() => {
            if (LocalizationManager.Instance != null)
            {
                var nextLang = LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE ? 
                    LocalizationManager.Language.EN : LocalizationManager.Language.DE;
                LocalizationManager.Instance.SetLanguage(nextLang);
            }
        });

        // Day/Night Toggle Button (Upper Row Right)
        GameObject dayNightBtn = CreateWindowsButton(settingsView.transform, "DayNightBtn", "TAG/NACHT", new Vector2(190, -180), new Vector2(360, 55));
        dayNightBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ToggleDayNightMode());

        // Reset Button (Lower Row Left)
        GameObject resetBtn = CreateWindowsButton(settingsView.transform, "ResetButton", "ZURÜCKSETZEN", new Vector2(-190, -250), new Vector2(360, 55));
        resetBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ResetProgress());

        // Back Button to return to main menu (Lower Row Right)
        GameObject backBtn = CreateWindowsButton(settingsView.transform, "BackButton", "ZURÜCK", new Vector2(190, -250), new Vector2(360, 55));
        backBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowMainView());

        // --- CREDITS VIEW CONTENT ---
        UIFactory.CreateText(creditsView.transform, "CreditsHeader", "SYSTEM-INFORMATION (README)", new Vector2(0, 260), 32, TextAnchor.MiddleCenter).color = Color.black;

        // Credits Frame (single-column)
        GameObject credsFrame = UIFactory.CreateUIElement(creditsView.transform as RectTransform, "CreditsFrame", new Vector2(0, 55), new Vector2(860, 360));
        UIFactory.SetupImage(credsFrame, winInnerFrameSprite, false);

        // Left Content: Creators & Assets
        string creditsString = "<b>FIRST AID SIMULATOR - PARK EDITION</b>\n" +
                               "Ein interaktiver Simulator für Erste Hilfe in einer Parkumgebung.\n\n" +
                               "<b>SCHÖPFER & MITWIRKENDE:</b>\n" +
                               "• <b>Artjom Becker</b> (Projektleitung & Programmierung)\n" +
                               "• <b>AG Serious Games</b> (TU Darmstadt)\n\n" +
                               "<b>ASSET-CREDITS:</b>\n" +
                               "• <b>Spieler & Map:</b> Cainos (cainos.itch.io)\n" +
                               "• <b>Windows UI:</b> Comp-3 Interactive (comp3.itch.io)\n" +
                               "• <b>Sounds UI:</b> Noah Kühne (Menu Sounds V2)\n" +
                               "• <b>Musik & Medien:</b> Generiert von Gemini AI\n\n" +
                               "<i>Erstellt für Bildungszwecke. Jede Sekunde zählt!</i>";

        Text credsTxt = UIFactory.CreateText(credsFrame.transform, "CreditsContent", creditsString, Vector2.zero, 17, TextAnchor.UpperLeft);
        credsTxt.color = Color.black;
        credsTxt.supportRichText = true;
        credsTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 320);

        // ── "Zukunftspotenzial" Sub-Menu Button ──
        GameObject futureOpenBtn = CreateWindowsButton(creditsView.transform, "FutureButton",
            "\U0001f680  ZUKUNFTSPOTENZIAL  \u2192", new Vector2(0, -185), new Vector2(440, 58));
        futureOpenBtn.GetComponent<Image>().color = new Color(0.88f, 0.86f, 1f);
        futureOpenBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowFutureView());

        // TU Darmstadt logo
        Sprite logoSprite = LoadSprite("Sprites/TU_Darmstadt_Logo.png");
        if (logoSprite != null)
        {
            GameObject tudLogoObj = UIFactory.CreateUIElement(creditsView.transform as RectTransform, "TUDarmstadtLogo", new Vector2(-310, -265), new Vector2(155, 62));
            UIFactory.SetupImage(tudLogoObj, logoSprite, true);
        }

        // AG Serious Games logo
        Sprite agLogoSprite = LoadSprite("Sprites/AG_Serious_Games_Logo.png");
        if (agLogoSprite != null)
        {
            GameObject agLogoObj = UIFactory.CreateUIElement(creditsView.transform as RectTransform, "AGSeriousGamesLogo", new Vector2(310, -265), new Vector2(65, 65));
            UIFactory.SetupImage(agLogoObj, agLogoSprite, true);
        }

        // Back to main menu
        GameObject credsBackBtn = CreateWindowsButton(creditsView.transform, "CreditsBackButton", "ZURÜCK", new Vector2(0, -295), new Vector2(300, 55));
        credsBackBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowMainView());

        // ═══════════════════════════════════════════════════════════════════
        // FUTURE VIEW — Sub-view inside the same dialog
        // ═══════════════════════════════════════════════════════════════════
        GameObject futureView = UIFactory.CreateUIElement(dialog.transform as RectTransform, "FutureView", Vector2.zero, new Vector2(950, 740));
        futureView.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        futureView.SetActive(false);
        menuManager.futureView = futureView;

        // Title
        Text futureTitleTxt = UIFactory.CreateText(futureView.transform, "FutureTitle",
            "\U0001f680  ZUKUNFTSPOTENZIAL", new Vector2(0, 300), 30, TextAnchor.MiddleCenter);
        futureTitleTxt.color = new Color(0.2f, 0.1f, 0.5f);
        futureTitleTxt.fontStyle = FontStyle.Bold;
        futureTitleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 45);

        Text futureSub = UIFactory.CreateText(futureView.transform, "FutureSub",
            "Geplante Erweiterungen des First Aid Simulators", new Vector2(0, 248), 16, TextAnchor.MiddleCenter);
        futureSub.color = new Color(0.45f, 0.35f, 0.6f);
        futureSub.fontStyle = FontStyle.Italic;
        futureSub.GetComponent<RectTransform>().sizeDelta = new Vector2(860, 28);

        // 6 Feature Cards in a 3x2 grid
        string[] fCardTitles = { 
            "\U0001f9d1\u200d\U0001f9bc  VR-Training", 
            "\U0001f465  Koop-Multiplayer", 
            "\U0001f4dc  Echte Zertifikate", 
            "\u2709\ufe0f  Story-Modus", 
            "\U0001f3d7\ufe0f  Park-Architekt", 
            "\U0001f916  KI-Szenarien" 
        };
        string[] fCardBodies = {
            "Erste-Hilfe hautnah erleben.\nVR-Brillen ermöglichen realistisches\nStress- & Bewegungstraining.",
            "Koordinierte Rettung im Team:\nEin Spieler sichert die Unfallstelle,\nwährend ein anderer CPR macht.",
            "Theorie-Stunden anrechenbar\nfür Führerschein-Bewerber durch\nKooperation mit Rettungsdiensten.",
            "Kampagnen-Modus als Sanitäter-Azubi.\nErhalte E-Mails von Dr. Sauer\n(BossMail.exe) & meistere Tagesaufgaben.",
            "Baue und gestalte deinen eigenen Park\nim Editor und platziere dort\nUnfallgefahren selbst.",
            "Dialog-Feedback per LLM\n(Gemini AI) basierend auf der\nemotionalen Ansprache des Spielers."
        };
        Color[] fCardColors = {
            new Color(0.18f, 0.3f, 0.7f),
            new Color(0.15f, 0.5f, 0.35f),
            new Color(0.6f, 0.38f, 0.08f),
            new Color(0.7f, 0.15f, 0.15f),
            new Color(0.2f, 0.55f, 0.65f),
            new Color(0.45f, 0.15f, 0.65f)
        };
        Vector2[] fCardPositions = {
            new Vector2(-300, 85),  new Vector2(0, 85),  new Vector2(300, 85),
            new Vector2(-300, -110), new Vector2(0, -110), new Vector2(300, -110)
        };

        for (int i = 0; i < 6; i++)
        {
            GameObject fCard = UIFactory.CreateUIElement(futureView.transform as RectTransform,
                "FCard_" + i, fCardPositions[i], new Vector2(280, 160));
            UIFactory.SetupImage(fCard, winInnerFrameSprite, false);
            fCard.GetComponent<Image>().color = new Color(0.97f, 0.96f, 1f);

            // Colored header strip
            GameObject fHdr = UIFactory.CreateUIElement(fCard.GetComponent<RectTransform>(),
                "Hdr", new Vector2(0, 57), new Vector2(280, 46));
            fHdr.GetComponent<Image>().color = fCardColors[i];

            Text fHdrTxt = UIFactory.CreateText(fHdr.transform, "T", fCardTitles[i], Vector2.zero, 18, TextAnchor.MiddleCenter);
            fHdrTxt.color = Color.white;
            fHdrTxt.fontStyle = FontStyle.Bold;
            fHdrTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 42);

            Text fBodyTxt = UIFactory.CreateText(fCard.transform, "Body", fCardBodies[i], new Vector2(0, -22), 13, TextAnchor.MiddleCenter);
            fBodyTxt.color = new Color(0.15f, 0.1f, 0.25f);
            fBodyTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 95);
        }

        // Back button → Credits
        GameObject futureBackBtn = CreateWindowsButton(futureView.transform, "FutureBackButton",
            "\u2190  ZURÜCK ZU MITWIRKENDE", new Vector2(0, -310), new Vector2(380, 55));
        futureBackBtn.GetComponent<Button>().onClick.AddListener(() => menuManager.ShowCreditsFromFuture());

        // ── OS DESKTOP SHORTCUTS (Left sidebar on desktop) ──
        // (Empty, as all icons are now in the Start Menu)

        // ── TASKBAR & START MENU ──
        // Taskbar Panel (bottom horizontal stretch)
        GameObject taskbar = UIFactory.CreateUIElement(menu.transform as RectTransform, "Taskbar", new Vector2(0, 0), new Vector2(0, 48));
        RectTransform taskbarRT = taskbar.GetComponent<RectTransform>();
        taskbarRT.anchorMin = new Vector2(0, 0);
        taskbarRT.anchorMax = new Vector2(1, 0);
        taskbarRT.pivot = new Vector2(0.5f, 0);
        taskbarRT.anchoredPosition = Vector2.zero;
        
        UIFactory.SetupImage(taskbar, winBaseSprite, false);
        taskbar.GetComponent<Image>().type = Image.Type.Sliced;
        taskbar.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 1f);

        // Start Menu Popup (initially hidden)
        GameObject startMenu = UIFactory.CreateUIElement(menu.transform as RectTransform, "StartMenu", new Vector2(5, 52), new Vector2(240, 620));
        RectTransform startMenuRT = startMenu.GetComponent<RectTransform>();
        startMenuRT.anchorMin = new Vector2(0, 0);
        startMenuRT.anchorMax = new Vector2(0, 0);
        startMenuRT.pivot = new Vector2(0, 0);
        startMenuRT.anchoredPosition = new Vector2(5, 52);

        UIFactory.SetupImage(startMenu, winBaseSprite, false);
        startMenu.GetComponent<Image>().type = Image.Type.Sliced;
        startMenu.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f, 1f);

        GameObject smShadow = UIFactory.CreateUIElement(startMenuRT, "StartMenuShadow", new Vector2(4, -4), new Vector2(240, 620));
        Image smShadowImg = smShadow.GetComponent<Image>();
        smShadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        smShadowImg.raycastTarget = false;
        smShadow.transform.SetAsFirstSibling();
        
        startMenu.SetActive(false);

        // Start Menu Sidebar Banner
        GameObject smSidebar = UIFactory.CreateUIElement(startMenuRT, "Sidebar", new Vector2(3, 0), new Vector2(32, 614));
        RectTransform sidebarRT = smSidebar.GetComponent<RectTransform>();
        sidebarRT.anchorMin = new Vector2(0, 0.5f);
        sidebarRT.anchorMax = new Vector2(0, 0.5f);
        sidebarRT.pivot = new Vector2(0, 0.5f);
        sidebarRT.anchoredPosition = new Vector2(3, 0);

        Image sbImg = smSidebar.GetComponent<Image>();
        sbImg.color = new Color(0.05f, 0.05f, 0.45f, 1f);

        GameObject sbTextObj = new GameObject("SidebarText");
        sbTextObj.transform.SetParent(smSidebar.transform, false);
        RectTransform sbTextRT = sbTextObj.AddComponent<RectTransform>();
        sbTextRT.anchoredPosition = Vector2.zero;
        sbTextRT.sizeDelta = new Vector2(300, 30);
        sbTextRT.localEulerAngles = new Vector3(0, 0, 90f);
        
        Text sbTxt = sbTextObj.AddComponent<Text>();
        sbTxt.text = "FirstAid 98";
        sbTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sbTxt.fontSize = 18;
        sbTxt.fontStyle = FontStyle.Bold;
        sbTxt.color = Color.white;
        sbTxt.alignment = TextAnchor.MiddleCenter;

        // Start Button
        GameObject startBtnObj = CreateWindowsButton(taskbarRT, "StartBtn", "🏁 Start", new Vector2(6, 0), new Vector2(100, 36));
        RectTransform startBtnRT = startBtnObj.GetComponent<RectTransform>();
        startBtnRT.anchorMin = new Vector2(0, 0.5f);
        startBtnRT.anchorMax = new Vector2(0, 0.5f);
        startBtnRT.pivot = new Vector2(0, 0.5f);
        startBtnRT.anchoredPosition = new Vector2(6, 0);
        
        startBtnObj.GetComponent<Button>().onClick.AddListener(() => {
            startMenu.SetActive(!startMenu.activeSelf);
            if (startMenu.activeSelf) startMenu.transform.SetAsLastSibling();
        });

        // Tray Container (recessed box)
        GameObject systemTray = UIFactory.CreateUIElement(taskbarRT, "SystemTray", new Vector2(-6, 0), new Vector2(210, 36));
        RectTransform trayRT = systemTray.GetComponent<RectTransform>();
        trayRT.anchorMin = new Vector2(1, 0.5f);
        trayRT.anchorMax = new Vector2(1, 0.5f);
        trayRT.pivot = new Vector2(1, 0.5f);
        trayRT.anchoredPosition = new Vector2(-6, 0);

        UIFactory.SetupImage(systemTray, winInnerFrameSprite, false);
        systemTray.GetComponent<Image>().type = Image.Type.Sliced;
        systemTray.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1f);

        // Language Switcher Button in Tray
        GameObject trayLangBtnObj = CreateWindowsButton(trayRT, "TaskbarLanguageToggle", "🌐 DE", new Vector2(6, 0), new Vector2(70, 24));
        RectTransform trayLangRT = trayLangBtnObj.GetComponent<RectTransform>();
        trayLangRT.anchorMin = new Vector2(0, 0.5f);
        trayLangRT.anchorMax = new Vector2(0, 0.5f);
        trayLangRT.pivot = new Vector2(0, 0.5f);
        trayLangRT.anchoredPosition = new Vector2(6, 0);
        
        trayLangBtnObj.GetComponent<Button>().onClick.AddListener(() => {
            if (LocalizationManager.Instance != null)
            {
                var next = LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE ? 
                    LocalizationManager.Language.EN : LocalizationManager.Language.DE;
                LocalizationManager.Instance.SetLanguage(next);
            }
        });

        // Digital Clock text in Tray
        GameObject clockObj = new GameObject("Clock");
        clockObj.transform.SetParent(trayRT, false);
        RectTransform clockRT = clockObj.AddComponent<RectTransform>();
        clockRT.anchorMin = new Vector2(1, 0.5f);
        clockRT.anchorMax = new Vector2(1, 0.5f);
        clockRT.pivot = new Vector2(1, 0.5f);
        clockRT.anchoredPosition = new Vector2(-10, 0);
        clockRT.sizeDelta = new Vector2(110, 24);

        Text clockTxt = clockObj.AddComponent<Text>();
        clockTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        clockTxt.fontSize = 14;
        clockTxt.fontStyle = FontStyle.Bold;
        clockTxt.color = Color.black;
        clockTxt.alignment = TextAnchor.MiddleCenter;
        
        StartCoroutine(ClockRoutine(clockTxt));

        // Start Menu vertical items setup
        string[] smItemsDe = { "📖 Handbuch.exe", "📈 Lernfortschritt.exe", "🏆 Erfolge.exe", "🏆 Bestenliste.exe", "✍️ Prüfung.exe", "📜 Zertifikat.exe", "🖼️ Darstellung.exe", "🐍 Snake.exe", "💣 Minesweeper.exe", "📧 BossMail.exe", "🏗️ ParkArchitect.exe", "💻 cmd.exe", "🗑️ Papierkorb", "🚪 Beenden" };
        string[] smItemsEn = { "📖 Handbook.exe", "📈 Progress.exe", "🏆 Achievements.exe", "🏆 Highscores.exe", "✍️ Exam.exe", "📜 Certificate.exe", "🖼️ Display.exe", "🐍 Snake.exe", "💣 Minesweeper.exe", "📧 BossMail.exe", "🏗️ ParkArchitect.exe", "💻 cmd.exe", "🗑️ Recycle Bin", "🚪 Quit Game" };
        System.Action[] smActions = {
            () => { if (FirstAidHandbook.Instance != null) FirstAidHandbook.Instance.ToggleWindow(); },
            () => { if (LearningTracker.Instance != null) LearningTracker.Instance.ToggleWindow(); },
            () => { if (BadgeManager.Instance != null) BadgeManager.Instance.ToggleWindow(); },
            () => { if (HighscoreManager.Instance != null) HighscoreManager.Instance.ToggleWindow(); },
            () => { if (ExamManager.Instance != null) ExamManager.Instance.ToggleWindow(); },
            () => { if (CertificateManager.Instance != null) CertificateManager.Instance.ToggleWindow(); },
            () => { if (WallpaperManager.Instance != null) WallpaperManager.Instance.ToggleWindow(); },
            () => { if (SnakeManager.Instance != null) SnakeManager.Instance.ToggleWindow(); },
            () => { if (MinesweeperManager.Instance != null) MinesweeperManager.Instance.ToggleWindow(); },
            () => { if (BossMailManager.Instance != null) BossMailManager.Instance.ToggleWindow(); },
            () => { if (ParkArchitectManager.Instance != null) ParkArchitectManager.Instance.ToggleWindow(); },
            () => { if (TerminalManager.Instance != null) TerminalManager.Instance.ToggleTerminal(); },
            () => { 
                if (menuManager != null) menuManager.ResetProgress(); 
                if (gameManager != null && gameManager.sfxSource != null && telephoneSound != null)
                {
                    gameManager.sfxSource.PlayOneShot(telephoneSound);
                }
            },
            () => { Application.Quit(); }
        };

        float smItemYStart = 575f;
        float smItemSpacing = 44f;
        for (int i = 0; i < smActions.Length; i++)
        {
            int index = i;
            GameObject itemBtnObj = CreateWindowsButton(startMenuRT, "StartMenuItem_" + i, smItemsDe[i], new Vector2(138, smItemYStart - i * smItemSpacing), new Vector2(195, 42));
            RectTransform itemRT = itemBtnObj.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0);
            itemRT.anchorMax = new Vector2(0, 0);
            itemRT.pivot = new Vector2(0, 0);
            itemRT.anchoredPosition = new Vector2(38, smItemYStart - i * smItemSpacing);
            
            itemBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                startMenu.SetActive(false);
                smActions[index]();
            });
        }

        menuManager.Initialize();
        LocalizeMenuUI();
    }

    private System.Collections.IEnumerator ClockRoutine(Text clockText)
    {
        while (true)
        {
            if (clockText != null)
            {
                clockText.text = System.DateTime.Now.ToString("HH:mm");
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public GameObject CreateWindowsDialog(Transform parent, string name, string title, Vector2 pos, Vector2 size)
    {
        // ── Background Base (Dark elegant) ──
        GameObject dialog = UIFactory.CreateUIElement(parent as RectTransform, name, pos, size);
        UIFactory.SetupImage(dialog, winBaseSprite, false);
        Image baseImg = dialog.GetComponent<Image>();
        // Tint the base sprite slightly darker for a premium feel
        baseImg.color = new Color(0.92f, 0.92f, 0.95f, 1f);

        // ── Outer Glow / Shadow Layer (Parented to dialog at local offset) ──
        GameObject shadowObj = UIFactory.CreateUIElement(dialog.transform as RectTransform, name + "_Shadow", new Vector2(4, -4), size);
        Image shadowImg = shadowObj.GetComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.45f);
        shadowImg.raycastTarget = false;
        shadowObj.transform.SetAsFirstSibling();

        // ── Header Bar (Gradient look via layered elements) ──
        float headerHeight = 38f;
        GameObject header = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Header", new Vector2(0, size.y / 2f - headerHeight / 2f), new Vector2(size.x - 4, headerHeight));
        UIFactory.SetupImage(header, winHeaderSprite, false);
        header.AddComponent<WindowDragger>();

        // ── Accent Line (vibrant gradient stripe under header) ──
        GameObject accentLine = UIFactory.CreateUIElement(dialog.transform as RectTransform, "AccentLine", 
            new Vector2(0, size.y / 2f - headerHeight - 2f), new Vector2(size.x - 4, 4));
        Image accentImg = accentLine.GetComponent<Image>();
        accentImg.color = new Color(1f, 0.35f, 0.35f, 1f); // Vibrant red accent
        accentImg.raycastTarget = false;
        // Animate the accent line color
        AccentPulse pulse = accentLine.AddComponent<AccentPulse>();

        // ── Title icon (emoji-style marker) ──
        string iconPrefix = "⚕ "; // Medical cross
        if (title.Contains("Quiz")) iconPrefix = "🧠 ";
        else if (title.Contains("Triage")) iconPrefix = "🚨 ";
        else if (title.Contains("Hitze")) iconPrefix = "☀ ";
        else if (title.Contains("Heimlich") || title.Contains("Chok")) iconPrefix = "💨 ";
        else if (title.Contains("Kühl") || title.Contains("Burn")) iconPrefix = "🔥 ";
        else if (title.Contains("Defibrill") || title.Contains("AED")) iconPrefix = "⚡ ";
        else if (title.Contains("112") || title.Contains("Leitstelle")) iconPrefix = "📞 ";
        else if (title.Contains("Shop") || title.Contains("Ausrüstung")) iconPrefix = "🛒 ";
        else if (title.Contains("Pause")) iconPrefix = "⏸ ";
        else if (title.Contains("Monitor")) iconPrefix = "📋 ";
        else if (title.Contains("Notfallkoffer")) iconPrefix = "🧳 ";
        else if (title.Contains("Vergiftung") || title.Contains("Poison")) iconPrefix = "☠ ";
        else if (title.Contains("Sicherungs") || title.Contains("Shock")) iconPrefix = "⚡ ";
        else if (title.Contains("Einstellung")) iconPrefix = "⚙ ";

        // Title Text (bigger, bolder)
        Text t = UIFactory.CreateText(header.transform, "Title", iconPrefix + title.Replace(".exe", ""), new Vector2(10, 0), 22, TextAnchor.MiddleLeft);
        t.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x - 30, headerHeight);
        t.color = Color.white;
        t.fontStyle = FontStyle.Bold;
        Shadow titleShadow = t.GetComponent<Shadow>();
        if (titleShadow != null) { titleShadow.effectColor = new Color(0, 0, 0, 0.7f); titleShadow.effectDistance = new Vector2(1, -1); }

        // ── Close button indicator (cosmetic "X") ──
        Text closeX = UIFactory.CreateText(header.transform, "CloseX", "✕", new Vector2(size.x / 2f - 25, 0), 20, TextAnchor.MiddleCenter);
        closeX.GetComponent<RectTransform>().sizeDelta = new Vector2(30, headerHeight);
        closeX.color = new Color(1f, 1f, 1f, 0.5f);

        // ── Entry animation ──
        DialogPopIn popIn = dialog.AddComponent<DialogPopIn>();
        
        // Reset button step counter for this new dialog
        ResetButtonCounter();
        
        return dialog;
    }
    
    private int _buttonCounter = 0;
    
    private UnityEngine.UI.Scrollbar CreateMissionListScrollbar(Transform parent, UnityEngine.UI.ScrollRect scrollRect)
    {
        GameObject scrollbarObj = new GameObject("Scrollbar Vertical");
        scrollbarObj.transform.SetParent(parent, false);

        RectTransform sbRT = scrollbarObj.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 0.5f);
        sbRT.anchoredPosition = Vector2.zero;
        sbRT.sizeDelta = new Vector2(22, 0);
        sbRT.offsetMin = new Vector2(-26, 12);
        sbRT.offsetMax = new Vector2(-6, -12);

        Image trackImg = scrollbarObj.AddComponent<Image>();
        trackImg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        UnityEngine.UI.Scrollbar scrollbar = scrollbarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollbar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;

        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbarObj.transform, false);
        RectTransform slideRT = slidingArea.AddComponent<RectTransform>();
        slideRT.anchorMin = Vector2.zero;
        slideRT.anchorMax = Vector2.one;
        slideRT.offsetMin = new Vector2(4, 4);
        slideRT.offsetMax = new Vector2(-4, -4);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(14, 40);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.75f, 0.2f, 0.2f, 1f);

        scrollbar.handleRect = handleRT;
        scrollbar.targetGraphic = handleImg;

        return scrollbar;
    }

    public GameObject CreateWindowsButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        _buttonCounter++;
        
        GameObject btnObj = UIFactory.CreateUIElement(parent as RectTransform, name, pos, size);
        UIFactory.SetupImage(btnObj, winButtonSprite, false);
        
        // ── Colored left accent strip (no number, just color) ──
        GameObject accentStrip = UIFactory.CreateUIElement(btnObj.GetComponent<RectTransform>(), "LeftAccent",
            new Vector2(-size.x / 2f + 3, 0), new Vector2(6, size.y - 8));
        Image stripImg = accentStrip.GetComponent<Image>();
        Color[] accentColors = { 
            new Color(0.2f, 0.6f, 1f),
            new Color(0.2f, 0.8f, 0.4f),
            new Color(1f, 0.6f, 0.1f),
            new Color(0.8f, 0.3f, 0.9f),
            new Color(1f, 0.3f, 0.3f),
            new Color(0f, 0.8f, 0.8f)
        };
        stripImg.color = accentColors[(_buttonCounter - 1) % accentColors.Length];
        stripImg.raycastTarget = false;
        
        Button btn = btnObj.AddComponent<Button>();
        if (winButtonSprite != null && winButtonPressedSprite != null)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState st = new SpriteState();
            st.pressedSprite = winButtonPressedSprite;
            btn.spriteState = st;
        }
        
        // Label fills the full button width (no step-number gap needed)
        int actualFontSize = size.x < 180 ? 17 : 22;
        Text t = UIFactory.CreateText(btnObj.transform, "Label", label, Vector2.zero, actualFontSize, TextAnchor.MiddleCenter);
        RectTransform labelRT = t.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(size.x - 20, size.y);
        labelRT.anchoredPosition = Vector2.zero;
        t.color = Color.black;
        t.fontStyle = FontStyle.Bold;
        Shadow btnShadow = t.GetComponent<Shadow>();
        if (btnShadow != null) { btnShadow.effectColor = new Color(0, 0, 0, 0.15f); btnShadow.effectDistance = new Vector2(1, -1); }
        
        if (buttonClickSound != null && gameManager != null && gameManager.sfxSource != null)
        {
            btn.onClick.AddListener(() => gameManager.sfxSource.PlayOneShot(buttonClickSound));
        }
        UIFactory.AddHoverEffect(btnObj, buttonHoverSound, gameManager?.sfxSource);
        
        return btnObj;
    }
    
    /// <summary>
    /// Resets the button counter for each new dialog to start numbering from 1.
    /// Call this before creating buttons for a new dialog.
    /// </summary>
    private void ResetButtonCounter() { _buttonCounter = 0; }

    private GameObject CreateStyledButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color bgColor)
    {
        GameObject btnObj = UIFactory.CreateUIElement(parent.GetComponent<RectTransform>(), name, pos, size);
        Image img = btnObj.GetComponent<Image>();
        img.color = bgColor;
        
        Button btn = btnObj.AddComponent<Button>();
        
        Text t = UIFactory.CreateText(btnObj.transform, "Label", label, Vector2.zero, 30, TextAnchor.MiddleCenter);
        t.color = Color.white;
        
        if (buttonClickSound != null && gameManager != null && gameManager.sfxSource != null)
        {
            btn.onClick.AddListener(() => gameManager.sfxSource.PlayOneShot(buttonClickSound));
        }
        
        UIFactory.AddHoverEffect(btnObj, buttonHoverSound, gameManager?.sfxSource);
        
        return btnObj;
    }

    private IEnumerator AnimateLogo(RectTransform logo)
    {
        Vector3 startPos = logo.anchoredPosition;
        while (true)
        {
            float y = Mathf.Sin(Time.unscaledTime * 2f) * 20f;
            logo.anchoredPosition = startPos + new Vector3(0, y, 0);
            yield return null;
        }
    }



    private void SetupQuizPanel(RectTransform parent)
    {
        GameObject quiz = UIFactory.CreateFullscreenPanel(parent, "QuizPanel", new Color(0, 0, 0, 0.8f)); // Dark overlay
        quiz.SetActive(false);
        
        QuizManager qm = quiz.AddComponent<QuizManager>();
        gameManager.quizManager = qm;
        qm.quizPanel = quiz;

        // Windows Dialog Container
        GameObject dialog = CreateWindowsDialog(quiz.transform, "QuizDialog", "Medizinisches Wissens-Quiz.exe", Vector2.zero, new Vector2(1000, 750));

        // Question Inner Frame
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "QuestionBox", new Vector2(0, 150), new Vector2(900, 200));
        UIFactory.SetupImage(innerFrame, winInnerFrameSprite, false);
        
        qm.questionText = UIFactory.CreateText(innerFrame.transform, "QuestionText", "...", Vector2.zero, 34, TextAnchor.MiddleCenter);
        qm.questionText.color = Color.black;
        qm.questionText.fontStyle = FontStyle.Bold;
        
        // Load from ScriptableObject or fallback
        if (mainQuiz != null)
        {
            qm.allQuestions = new List<QuestionData>(mainQuiz.questions);
        }
        else
        {
            qm.allQuestions = new List<QuestionData>
            { 
                CreateQuestion("Was tust du zuerst bei einem Unfall?", new string[]{"Sicherheit prüfen", "Sofort beatmen", "Handy herausholen"}, 0),
                CreateQuestion("Prüfung des Bewusstseins: Wie?", new string[]{"Ansprechen & Anfassen", "Anschreien", "In den Arm kneifen"}, 0),
                CreateQuestion("Notrufnummer in Europa?", new string[]{"112", "911", "110"}, 0)
            };
        }
        
        qm.answerButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = CreateWindowsButton(dialog.transform, "Answer_" + i, "Antwort", new Vector2(0, -60 - (i * 90)), new Vector2(800, 70));
            qm.answerButtons[i] = btnObj.GetComponent<Button>();
        }
    }

    private void SetupBandagePanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "BandagePanel", Color.black);
        UIFactory.SetupImage(panel, bandageBackground, false);
        panel.SetActive(false);

        BandageManager bm = panel.AddComponent<BandageManager>();
        gameManager.bandageManager = bm;
        gameManager.bandagePanel = panel;
        bm.bandagePanel = panel;

        bm.timerSlider = UIFactory.CreateSlider(panel.transform, "TimerBar", new Vector2(0, -50), new Vector2(800, 30), Color.red, null, null);
        RectTransform timerRT = bm.timerSlider.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0.5f, 1f);
        timerRT.anchorMax = new Vector2(0.5f, 1f);

        bm.instructionText = UIFactory.CreateText(panel.transform, "Instructions", "...", new Vector2(0, 420), 32, TextAnchor.MiddleCenter);

        GameObject wound = UIFactory.CreateUIElement(panel.GetComponent<RectTransform>(), "WoundArea", Vector2.zero, new Vector2(400, 400));
        wound.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        bm.woundArea = wound.GetComponent<RectTransform>();
    }

    private void SetupStabilizePanel(RectTransform parent)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "StabilizePanel", Color.black);
        UIFactory.SetupImage(panel, stabilizeBG != null ? stabilizeBG : bandageBackground, false);
        panel.SetActive(false);

        StabilizeManager sm = panel.AddComponent<StabilizeManager>();
        gameManager.stabilizeManager = sm;
        gameManager.stabilizePanel = panel;
        sm.stabilizePanel = panel;
        
        sm.originalPose = unconsciousVictimSprite;
        sm.armUpPose = victimArmUp;
        sm.legBentPose = victimLegBent;
        sm.finalSidePose = victimSidePos;

        sm.instructionText = UIFactory.CreateText(panel.transform, "Instructions", "...", new Vector2(0, 420), 36, TextAnchor.MiddleCenter);

        GameObject victimView = UIFactory.CreateUIElement(panel.transform as RectTransform, "VictimView", Vector2.zero, new Vector2(900, 900));
        UIFactory.SetupImage(victimView, unconsciousVictimSprite, true);
        sm.victimBodyImage = victimView.GetComponent<Image>();

        GameObject handle = UIFactory.CreateUIElement(panel.transform as RectTransform, "DragHandle", Vector2.zero, new Vector2(100, 100));
        handle.GetComponent<Image>().color = new Color(1, 1, 0, 0.6f);
        sm.dragHandle = handle.GetComponent<RectTransform>();
        
        GameObject target = UIFactory.CreateUIElement(panel.transform as RectTransform, "TargetZone", Vector2.zero, new Vector2(120, 120));
        target.GetComponent<Image>().color = new Color(0, 1, 0, 0.3f);
        sm.targetZone = target.GetComponent<RectTransform>();
    }



    private void SetupCPRPanel(RectTransform parent)
    {
        GameObject cpr = UIFactory.CreateFullscreenPanel(parent, "CPRPanel", Color.black);
        UIFactory.SetupImage(cpr, cprBackground, false);
        cpr.SetActive(false);
        
        CPRManager cprm = cpr.AddComponent<CPRManager>();
        gameManager.cprPanel = cpr;
        gameManager.cprManager = cprm;
        cprm.cprPanel = cpr;

        cprm.successSlider = UIFactory.CreateSlider(cpr.transform, "SuccessBar", new Vector2(0, -100), new Vector2(800, 40), Color.green, null, null);
        RectTransform successRT = cprm.successSlider.GetComponent<RectTransform>();
        successRT.anchorMin = new Vector2(0.5f, 1f);
        successRT.anchorMax = new Vector2(0.5f, 1f);
        UIFactory.CreateText(cprm.successSlider.transform, "Label", "FORTSCHRITT", new Vector2(0, 50), 24, TextAnchor.MiddleCenter);

        GameObject hands = UIFactory.CreateUIElement(cpr.GetComponent<RectTransform>(), "Hands", new Vector2(0, -50), new Vector2(600, 600));
        cprm.handsImage = hands.GetComponent<Image>();
        UIFactory.SetupImage(hands, cprHands, true);

        cprm.feedbackText = UIFactory.CreateText(cpr.transform, "Feedback", "RHYTHMUS HALTEN!", new Vector2(0, 0), 32, TextAnchor.MiddleCenter);

        cprm.rhythmSlider = UIFactory.CreateSlider(cpr.transform, "RhythmSlider", new Vector2(0, 100), new Vector2(600, 60), Color.red, null, null);
        RectTransform rhythmRT = cprm.rhythmSlider.GetComponent<RectTransform>();
        rhythmRT.anchorMin = new Vector2(0.5f, 0f);
        rhythmRT.anchorMax = new Vector2(0.5f, 0f);
        
        GameObject rhythmTarget = UIFactory.CreateUIElement(rhythmRT, "TargetZone", Vector2.zero, new Vector2(150, 60));
        rhythmTarget.GetComponent<Image>().color = new Color(0, 1, 0, 0.4f);
    }

    private void SetupResultPanel(RectTransform parent)
    {
        GameObject res = UIFactory.CreateFullscreenPanel(parent, "ResultPanel", Color.black);
        res.SetActive(false);
        
        gameManager.resultPanel = res;
        gameManager.resultDetailsText = UIFactory.CreateText(res.transform, "Details", "Ergebnis...", Vector2.zero, 32, TextAnchor.MiddleCenter);
    }

    // --- UI HELPER METHODEN ---

    // UI helpers removed in favor of UIFactory static methods.


    // Removed redundant Text, Slider, and CreateButton logic.


    // --- WORLD HELPERS ---

    public GameObject CreateWorldObject(string name, Vector3 pos, Vector3 scale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        return obj;
    }

    public void SetupSprite(GameObject obj, Sprite s)
    {
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        if (s == null) s = unconsciousVictimSprite; // Fallback to unconscious sprite if possible

        if (s != null)
        {
            sr.sprite = s;
        }
        else
        {
            Debug.LogWarning("Sprite für " + obj.name + " ist NULL!");
            // Create a visible red square as fallback
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.red;
            tex.SetPixels(colors);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            // Reset scale so the fallback is visible (overriding 0.08 scale)
            obj.transform.localScale = Vector3.one;
        }
        // Use the same sorting layer as the top Cainos map layer
        sr.sortingLayerName = "Layer 1";
        sr.sortingOrder = 100;
    }

    private QuestionData CreateQuestion(string text, string[] options, int correctIdx)
    {
        var data = ScriptableObject.CreateInstance<QuestionData>();
        data.questionText = text;
        data.answers = options;
        data.correctAnswerIndex = correctIdx;
        return data;
    }

    // --- UPDATE LOGIK ---

    private void Update()
    {
        if (gameManager == null || gameManager.currentPhase != GameManager.GamePhase.Intro)
        {
            if (arrowRect != null) arrowRect.gameObject.SetActive(false);
            if (victimStrokeTransform != null) victimStrokeTransform.gameObject.SetActive(false);
            if (victimHeartAttackTransform != null) victimHeartAttackTransform.gameObject.SetActive(false);
            if (victimSnakebiteTransform != null) victimSnakebiteTransform.gameObject.SetActive(false);
            if (victimsContainer != null) victimsContainer.SetActive(false);
            return;
        }

        // Show victims when in intro
        if (victimsContainer != null && !victimsContainer.activeSelf)
            victimsContainer.SetActive(true);

        UpdateDirectionIndicator();
        CheckVictimProximity();
    }

    
    /// <summary>
    /// Fix #8: Reset local transform references after active instances are destroyed,
    /// so CheckVictimProximity doesn't compute distances to destroyed GameObjects.
    /// </summary>
    public void ResetVictimTransforms()
    {
        if (gameManager.bikeAccidentHelped) victimBikeTransform = null;
        if (gameManager.bleedingWoundHelped) victimBleedTransform = null;
        if (gameManager.unconsciousHelped) victimUnconsciousTransform = null;
        if (gameManager.burnInjuryHelped) burnVictimTransform = null;
        if (gameManager.chokingHelped) chokingVictimTransform = null;
        if (gameManager.heatstrokeHelped) heatstrokeVictimTransform = null;
        if (gameManager.triageHelped) triageVictimTransform = null;
        if (gameManager.electricShockHelped) shockVictimTransform = null;
        if (gameManager.poisoningHelped) poisonVictimTransform = null;
        if (gameManager.boneFractureHelped) boneFractureTransform = null;
        if (gameManager.allergicShockHelped) allergicShockTransform = null;
        if (gameManager.drowningHelped) drowningVictimTransform = null;
        if (gameManager.diabeticShockHelped) diabeticShockTransform = null;
        if (gameManager.panicAttackHelped) panicAttackTransform = null;
        if (gameManager.strokeHelped) victimStrokeTransform = null;
    }

    private void UpdateDirectionIndicator()
    {
        PlayerController pCtrl = Object.FindAnyObjectByType<PlayerController>();
        GameObject player = pCtrl != null ? pCtrl.gameObject : null;
        if (player == null || arrowRect == null) return;
        
        if (activeArrowTarget == null || activeArrowTarget.gameObject == null || !activeArrowTarget.gameObject.activeSelf) 
        { 
            activeArrowTarget = null;
            arrowRect.gameObject.SetActive(false); 
            return; 
        }

        Vector3 playerPos = player.transform.position;
        Vector3 toTarget = activeArrowTarget.position - playerPos;
        
        if (toTarget.magnitude > 5f)
        {
            arrowRect.gameObject.SetActive(true);
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            arrowRect.localRotation = Quaternion.Euler(0, 0, angle);
            arrowRect.anchoredPosition = new Vector2(toTarget.x, toTarget.y).normalized * 350f;
        }
        else arrowRect.gameObject.SetActive(false);
    }


    private void CheckVictimProximity()
    {
        PlayerController pCtrl2 = Object.FindAnyObjectByType<PlayerController>();
        GameObject player = pCtrl2 != null ? pCtrl2.gameObject : null;
        if (player == null) { Debug.LogWarning("Player nicht gefunden!"); return; }

        if (player.GetComponent<FootstepsEffect>() == null)
        {
            player.AddComponent<FootstepsEffect>();
        }

        Vector3 p = player.transform.position;
        float dist = triggerDistance;

        currentNearbyVictim = null;
        isNearMedkit = false;

        // Use the currently spawned mission victim (covers all mission types incl. new ones)
        if (gameManager != null)
        {
            float nearestDist = dist;
            foreach (var mission in gameManager.availableMissions)
            {
                if (mission.activeInstance == null) continue;
                float d = Vector3.Distance(p, mission.activeInstance.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    currentNearbyVictim = mission.type;
                }
            }
        }

        float dMedkit = medkitTransform != null ? Vector3.Distance(p, medkitTransform.position) : float.MaxValue;
        if (dMedkit < dist && !gameManager.hasMedkit && medkitTransform.gameObject.activeSelf) isNearMedkit = true;

        if (interactionPromptRect != null)
        {
            if (isNearMedkit)
            {
                interactionPromptRect.gameObject.SetActive(true);
                interactionPromptText.text = "[E] Notfallkoffer nehmen";
            }
            else if (currentNearbyVictim != null)
            {
                interactionPromptRect.gameObject.SetActive(true);
                if (!gameManager.hasMedkit)
                {
                    interactionPromptText.text = "Du brauchst einen Notfallkoffer!";
                }
                else if (IsVictimBlockedByOnlookers(currentNearbyVictim.Value))
                {
                    interactionPromptText.text = "⚠️ Gaffer blockieren den Unfallort!\nZiehe sie mit [E] weg!";
                }
                else
                {
                    interactionPromptText.text = "[E] Untersuchen";
                }
            }
            else
            {
                interactionPromptRect.gameObject.SetActive(false);
            }

            // Follow player using FloatingUI UpdateBasePosition for smoothness
            FloatingUI fUI = interactionPromptRect.GetComponent<FloatingUI>();
            Vector2 screenPos = Camera.main.WorldToScreenPoint(p + new Vector3(0, 1.4f, 0));
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                interactionPromptRect.parent as RectTransform, 
                screenPos, 
                null, 
                out canvasPos
            );

            if (fUI != null)
            {
                fUI.UpdateBasePosition(canvasPos);
            }
            else
            {
                interactionPromptRect.anchoredPosition = canvasPos;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isNearMedkit)
            {
                gameManager.hasMedkit = true;
                medkitTransform.gameObject.SetActive(false);
                interactionPromptRect.gameObject.SetActive(false);
                // Koffer-Tutorial nur beim ersten Mal — danach frei mit Opfern interagieren
                bool kitTutorialDone = ScoreManager.Instance != null && ScoreManager.Instance.emergencyKitPerfect;
                if (!kitTutorialDone)
                    gameManager.StartEmergencyKitPhase();
            }
            else if (currentNearbyVictim != null && gameManager.hasMedkit)
            {
                if (IsVictimBlockedByOnlookers(currentNearbyVictim.Value))
                {
                    if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound);
                    }
                }
                else
                {
                    interactionPromptRect.gameObject.SetActive(false);
                    gameManager.TriggerAccident(currentNearbyVictim.Value);
                }
            }
        }
    }

    private bool IsVictimBlockedByOnlookers(GameManager.VictimType type)
    {
        if (gameManager == null) return false;
        GameObject victimObj = null;
        foreach (var m in gameManager.availableMissions)
        {
            if (m.type == type && m.activeInstance != null)
            {
                victimObj = m.activeInstance;
                break;
            }
        }
        if (victimObj == null) return false;

        NPCWander[] npcs = Object.FindObjectsByType<NPCWander>(FindObjectsSortMode.None);
        foreach (var npc in npcs)
        {
            if (npc.isPanicking && !npc.isCleared)
            {
                float d = Vector3.Distance(npc.transform.position, victimObj.transform.position);
                if (d <= 4.0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ShakeScreen(float duration, float magnitude)
    {
        StartCoroutine(ScreenShakeRoutine(duration, magnitude));
    }

    private IEnumerator ScreenShakeRoutine(float duration, float magnitude)
    {
        if (Camera.main == null) yield break;
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude / 100f;
            float y = Random.Range(-1f, 1f) * magnitude / 100f;
            Camera.main.transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = originalPos;
    }
    IEnumerator PulseEffect(GameObject obj)
    {
        while (obj != null)
        {
            float s = 1f + Mathf.PingPong(Time.unscaledTime * 2f, 0.1f);
            obj.transform.localScale = new Vector3(s, s, 1);
            yield return null;
        }
    }

    // ─── NEW MISSION PANELS ────────────────────────────────────────────────────

    private GameObject newMissionPanel;

    public void ShowNewMissionPanel(GameManager.VictimType type)
    {
        // Clean up old panel
        if (newMissionPanel != null) Destroy(newMissionPanel);

        Canvas canvas = gameCanvas.GetComponent<Canvas>();
        if (canvas == null) return;

        // Full-screen overlay panel
        newMissionPanel = UIFactory.CreateUIElement(canvas.GetComponent<RectTransform>(), "NewMissionOverlay",
            Vector2.zero, new Vector2(1000, 700));
        UIFactory.SetupImage(newMissionPanel, winBaseSprite, false);

        switch (type)
        {
            case GameManager.VictimType.BoneFracture:   BuildBoneFractureUI(newMissionPanel, type); break;
            case GameManager.VictimType.AllergicShock:  BuildAllergicShockUI(newMissionPanel, type); break;
            case GameManager.VictimType.DrowningVictim: BuildDrowningUI(newMissionPanel, type);      break;
            case GameManager.VictimType.DiabeticShock:  BuildDiabeticShockUI(newMissionPanel, type); break;
            case GameManager.VictimType.PanicAttack:    BuildPanicAttackUI(newMissionPanel, type);   break;
            case GameManager.VictimType.DogPoisoning:   BuildDogPoisoningUI(newMissionPanel, type);  break;
            case GameManager.VictimType.HeartAttack:    BuildHeartAttackUI(newMissionPanel, type);   break;
            case GameManager.VictimType.Snakebite:      BuildSnakebiteUI(newMissionPanel, type);     break;
        }

        // Spawn 2-4 Gaffers randomly to block the UI (Crowd Control mechanic)
        // Disabled UI-based gaffers for physical dragging mechanic
        /*
        if (GafferManager.Instance != null)
        {
            GafferManager.Instance.SpawnUIGaffers(newMissionPanel.transform, Random.Range(2, 5));
        }
        */
    }

    // ── HeartAttack: Defibrillator ──────────
    void BuildHeartAttackUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "💔 Herzinfarkt", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Setze den Defibrillator ein! Warte bis er aufgeladen ist.", new Vector2(0, 220), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        GameObject statusTxtObj = UIFactory.CreateText(panel.transform, "Status", "LÄDT...", new Vector2(0, 70), 24, TextAnchor.MiddleCenter).gameObject;
        Text statusTxt = statusTxtObj.GetComponent<Text>();
        statusTxt.color = new Color(0.8f, 0.6f, 0.1f);

        GameObject shockBtnObj = CreateWindowsButton(panel.transform, "ShockBtn", "SCHOCK AUSLÖSEN", new Vector2(0, -50), new Vector2(300, 80));
        Button shockBtn = shockBtnObj.GetComponent<Button>();
        shockBtn.interactable = false;

        bool isCharged = false;
        int shocks = 0;
        
        IEnumerator ChargeRoutine()
        {
            while (shocks < 3)
            {
                isCharged = false;
                shockBtn.interactable = false;
                statusTxt.color = new Color(0.8f, 0.6f, 0.1f);
                for (int i = 0; i <= 100; i += 10)
                {
                    statusTxt.text = $"LÄDT... {i}%";
                    yield return new WaitForSecondsRealtime(0.15f);
                }
                statusTxt.text = "GELADEN! JETZT SCHOCKEN!";
                statusTxt.color = Color.red;
                shockBtn.interactable = true;
                isCharged = true;
                
                while (isCharged) yield return null;
            }
        }
        StartCoroutine(ChargeRoutine());

        shockBtn.onClick.AddListener(() => {
            if (!isCharged) return;
            shocks++;
            isCharged = false;
            ShakeScreen(0.2f, 15f);
            
            if (shocks >= 3)
            {
                statusTxt.text = "HERZSCHLAG STABIL!";
                statusTxt.color = new Color(0.1f, 0.6f, 0.2f);
                shockBtn.interactable = false;
                StopCoroutine("ChargeRoutine");
                StartCoroutine(DelayedComplete(type, 100, panel));
            }
        });
    }

    // ── Snakebite: Tourniquet abbinden ──────────
    void BuildSnakebiteUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "🐍 Schlangenbiss", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Lege ein Tourniquet an, bevor sich das Gift ausbreitet!\nZieh den Regler ganz nach rechts.", new Vector2(0, 220), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        GameObject statusTxtObj = UIFactory.CreateText(panel.transform, "Status", "GIFT BREITET SICH AUS!", new Vector2(0, 100), 24, TextAnchor.MiddleCenter).gameObject;
        Text statusTxt = statusTxtObj.GetComponent<Text>();
        statusTxt.color = Color.red;

        // Create a Slider manually
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(panel.transform, false);
        RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.anchoredPosition = new Vector2(0, 0);
        sliderRT.sizeDelta = new Vector2(400, 40);

        GameObject bg = UIFactory.CreateUIElement(sliderRT, "Background", Vector2.zero, new Vector2(400, 20));
        bg.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderRT, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5, 0);
        fillAreaRT.offsetMax = new Vector2(-15, 0);

        GameObject fill = UIFactory.CreateUIElement(fillAreaRT, "Fill", Vector2.zero, Vector2.zero);
        fill.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.zero; // driven by slider
        fillRT.sizeDelta = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderRT, false);
        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = new Vector2(0, 0);
        handleAreaRT.anchorMax = new Vector2(1, 1);
        handleAreaRT.offsetMin = new Vector2(10, 0);
        handleAreaRT.offsetMax = new Vector2(-10, 0);

        GameObject handle = UIFactory.CreateUIElement(handleAreaRT, "Handle", Vector2.zero, new Vector2(30, 40));
        handle.GetComponent<Image>().color = Color.white;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;

        bool complete = false;

        slider.onValueChanged.AddListener((val) => {
            if (complete) return;
            if (val >= 0.95f)
            {
                complete = true;
                slider.interactable = false;
                statusTxt.text = "ABGEBUNDEN!";
                statusTxt.color = new Color(0.1f, 0.6f, 0.2f);
                if (GameManager.Instance != null && GameManager.Instance.sfxSource != null)
                {
                    // Use standard click sound
                    GameManager.Instance.sfxSource.Play();
                }
                StartCoroutine(DelayedComplete(type, 100, panel));
            }
            else
            {
                statusTxt.text = $"ZIEHEN... {(int)(val * 100)}%";
                statusTxt.color = new Color(1f, 0.5f, 0f);
            }
        });
    }

    // ── DogPoisoning: Tier-Notfall (Giftköder) ──────────
    void BuildDogPoisoningUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "🐶 Tier-Notfall (Vergiftung)", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Ein Hund hat wahrscheinlich einen Giftköder gefressen!\nBefolge die Schritte, um Erste Hilfe zu leisten.", new Vector2(0, 220), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        int[] order = { 0, 1, 2 };
        string[] labels = { "1. MAULKORB ANLEGEN\n(Beißschutz)", "2. KOHLETABLETTEN GEBEN\n(Gift binden)", "3. TIERARZT RUFEN\n(Transport)" };
        Vector2[] positions = { new Vector2(-260, 20), new Vector2(0, 20), new Vector2(260, 20) };
        int[] nextExpected = { 0 };

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            GameObject btn = CreateWindowsButton(panel.transform, "Step" + idx, labels[idx], positions[idx], new Vector2(240, 150));
            Button b = btn.GetComponent<Button>();
            
            b.onClick.AddListener(() => {
                if (nextExpected[0] == order[idx])
                {
                    nextExpected[0]++;
                    btn.GetComponentInChildren<Text>().color = Color.gray;
                    b.interactable = false;
                    if (AudioManager.Instance != null && AudioManager.Instance.uiClickSound != null)
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.uiClickSound);

                    if (nextExpected[0] == 3)
                    {
                        StartCoroutine(DelayedComplete(type, 40, panel));
                    }
                }
                else
                {
                    if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound);
                    ShakeScreen(0.3f, 15f);
                }
            });
        }
    }

    // ── BoneFracture: click 3 targets in order to 'splint' the limb ──────────
    void BuildBoneFractureUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "🦴 Knochenbruch – Schiene anlegen!", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Klicke die 3 Punkte in der richtigen Reihenfolge:\nOBER-SCHIENE → UNTER-SCHIENE → FIXIERUNG", new Vector2(0, 220), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        int[] order = { 0, 1, 2 };
        string[] labels = { "OBER-SCHIENE", "UNTER-SCHIENE", "FIXIERUNG" };
        string[] spriteNames = { "splint_upper", "splint_lower", "splint_fixation" };
        Vector2[] positions = { new Vector2(-260, 20), new Vector2(0, 20), new Vector2(260, 20) };
        int[] nextExpected = { 0 };  // ref-style via array

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            // Larger button size to elegantly fit beautiful pixel-art sprites
            GameObject btn = CreateWindowsButton(panel.transform, "Step" + idx, labels[idx], positions[idx], new Vector2(180, 180));
            Button b = btn.GetComponent<Button>();
            
            // Apply custom transparent medical sprite
            Sprite medSprite = Resources.Load<Sprite>(spriteNames[idx]);
            if (medSprite != null)
            {
                Image img = btn.GetComponent<Image>();
                img.sprite = medSprite;
                img.color = Color.white; // Keep original colored image!
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
            }

            // Adjust label text below the button
            Text btnTxt = btn.GetComponentInChildren<Text>();
            if (btnTxt != null)
            {
                btnTxt.fontSize = 15;
                btnTxt.color = Color.black;
                RectTransform txtRT = btnTxt.GetComponent<RectTransform>();
                txtRT.anchoredPosition = new Vector2(0, -115); // Move text below the button
                
                Shadow sh = btnTxt.GetComponent<Shadow>();
                if (sh != null) Destroy(sh);
            }

            b.onClick.AddListener(() =>
            {
                if (nextExpected[0] == idx)
                {
                    nextExpected[0]++;
                    btn.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
                    b.interactable = false;
                    if (nextExpected[0] >= 3)
                    {
                        StartCoroutine(DelayedComplete(type, 80, newMissionPanel));
                    }
                }
                else
                {
                    // Wrong order – flash red, reset
                    StartCoroutine(FlashRed(btn));
                    nextExpected[0] = 0;
                    // Reset all buttons
                    foreach (Transform child in panel.transform)
                    {
                        Button cb2 = child.GetComponent<Button>();
                        if (cb2 != null && child.name.StartsWith("Step"))
                        {
                            child.GetComponent<Image>().color = Color.white;
                            cb2.interactable = true;
                        }
                    }
                }
            });
        }

        UIFactory.CreateText(panel.transform, "Hint", "Falsche Reihenfolge → von vorne!", new Vector2(0, -200), 16, TextAnchor.MiddleCenter).color = new Color(0.5f, 0.1f, 0.1f);
    }

    // ── AllergicShock: hold the button for exactly 2-3 seconds ───────────────
    void BuildAllergicShockUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "🐝 Anaphylaxie – EpiPen injizieren!", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Halte den Knopf für genau 2–3 Sekunden gedrückt,\ndann loslassen um den EpiPen zu injizieren!", new Vector2(0, 220), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        float[] holdTime = { 0f };
        bool[] holding = { false };
        bool[] done = { false };

        // Progress bar background
        GameObject barBg = UIFactory.CreateUIElement(panel.transform as RectTransform, "BarBg", new Vector2(0, 20), new Vector2(500, 50));
        UIFactory.SetupImage(barBg, winInnerFrameSprite, false);
        barBg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

        // Fill bar
        GameObject barFill = UIFactory.CreateUIElement(barBg.transform as RectTransform, "Fill", new Vector2(-250, 0), new Vector2(0, 46));
        Image fillImg = barFill.GetComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 0.2f);
        RectTransform fillRT = barFill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0);
        fillRT.anchorMax = new Vector2(0, 1);
        fillRT.pivot = new Vector2(0, 0.5f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = new Vector2(0, 0);

        // Zone indicator
        GameObject zoneObj = UIFactory.CreateUIElement(barBg.transform as RectTransform, "Zone", Vector2.zero, new Vector2(100, 46));
        Image zoneImg = zoneObj.GetComponent<Image>();
        zoneImg.color = new Color(0.9f, 0.85f, 0.1f, 0.5f);
        RectTransform zoneRT = zoneObj.GetComponent<RectTransform>();
        zoneRT.anchorMin = new Vector2(2f/3f, 0);
        zoneRT.anchorMax = new Vector2(1f, 1);

        // Hold button
        GameObject holdBtn = CreateWindowsButton(panel.transform, "HoldBtn", "DRÜCKEN & HALTEN", new Vector2(0, -80), new Vector2(340, 90));

        // Add EventTrigger for holding
        UnityEngine.EventSystems.EventTrigger trigger = holdBtn.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        UnityEngine.EventSystems.EventTrigger.Entry entryDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => { holding[0] = true; });
        trigger.triggers.Add(entryDown);
        
        UnityEngine.EventSystems.EventTrigger.Entry entryUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => { holding[0] = false; });
        trigger.triggers.Add(entryUp);

        // Coroutine-based update
        StartCoroutine(EpiPenRoutine(holdBtn, fillRT, holding, holdTime, done, panel, type));
    }

    IEnumerator EpiPenRoutine(GameObject holdBtn, RectTransform fillRT, bool[] holding, float[] holdTime, bool[] done, GameObject panel, GameManager.VictimType type)
    {
        Button b = holdBtn.GetComponent<Button>();
        b.onClick.RemoveAllListeners();

        while (!done[0])
        {
            if (Input.GetKeyDown(KeyCode.Space)) holding[0] = true;
            if (Input.GetKeyUp(KeyCode.Space)) holding[0] = false;

            if (holding[0])
            {
                holdTime[0] += Time.deltaTime;
                float pct = Mathf.Clamp01(holdTime[0] / 3f);
                fillRT.anchorMax = new Vector2(pct, 1);
                fillRT.GetComponent<Image>().color = pct > 0.66f ? new Color(0.1f, 0.8f, 0.2f) : new Color(0.3f, 0.7f, 0.2f);

                if (holdTime[0] > 4f)
                {
                    // Too long – fail
                    StartCoroutine(FlashRed(holdBtn));
                    holdTime[0] = 0f;
                    holding[0] = false;
                    fillRT.anchorMax = new Vector2(0, 1);
                }
            }
            else if (!holding[0] && holdTime[0] > 0)
            {
                // Released – check if in zone (2-3s)
                if (holdTime[0] >= 2f && holdTime[0] <= 3f)
                {
                    done[0] = true;
                    StartCoroutine(DelayedComplete(type, 90, panel));
                }
                else
                {
                    StartCoroutine(FlashRed(holdBtn));
                    holdTime[0] = 0f;
                    fillRT.anchorMax = new Vector2(0, 1);
                }
            }
            yield return null;
        }
    }

    // ── DrowningVictim: rhythmic spacebar presses for CPR ────────────────────
    void BuildDrowningUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "🌊 Ertrinkung – Notfall-CPR!", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Drücke SPACE im Takt des Herzschlags!\nTreffe 15 Drücke im grünen Fenster.", new Vector2(0, 220), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        Text counterTxt = UIFactory.CreateText(panel.transform, "Counter", "0 / 15", Vector2.zero, 48, TextAnchor.MiddleCenter);
        counterTxt.color = Color.black;

        Text rhythmTxt = UIFactory.CreateText(panel.transform, "Rhythm", "⏸", new Vector2(0, -80), 60, TextAnchor.MiddleCenter);
        rhythmTxt.color = new Color(0.2f, 0.6f, 0.9f);

        StartCoroutine(CPRRhythmRoutine(counterTxt, rhythmTxt, panel, type));
    }

    IEnumerator CPRRhythmRoutine(Text counterTxt, Text rhythmTxt, GameObject panel, GameManager.VictimType type)
    {
        int presses = 0;
        float beatInterval = 0.6f;   // 100 bpm
        float window = 0.18f;
        float nextBeat = Time.time + beatInterval;

        while (presses < 15)
        {
            if (panel == null) yield break;

            bool inWindow = Mathf.Abs(Time.time - nextBeat) < window;

            // Visual pulse
            rhythmTxt.text = inWindow ? "❤️" : "🫀";
            rhythmTxt.color = inWindow ? Color.red : new Color(0.5f, 0.1f, 0.1f);

            if (Time.time >= nextBeat)
                nextBeat += beatInterval;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (inWindow) { presses++; counterTxt.text = presses + " / 15"; }
                else { StartCoroutine(FlashRed(counterTxt.gameObject)); }
            }

            yield return null;
        }

        StartCoroutine(DelayedComplete(type, 100, panel));
    }

    // ── DiabeticShock: multiple-choice quiz ──────────────────────────────────
    void BuildDiabeticShockUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "🍬 Diabetischer Schock!", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Q", "Der Blutzucker des Patienten ist zu niedrig.\nWas tust du ZUERST?", new Vector2(0, 200), 20, TextAnchor.MiddleCenter).color = Color.black;

        string[] options = {
            "Insulin spritzen",
            "Zuckerlösung / Traubenzucker geben",
            "Sofort 112 anrufen und warten",
            "Kaltes Wasser trinken lassen"
        };
        int correct = 1;
        Vector2[] positions = { new Vector2(0, 90), new Vector2(0, 10), new Vector2(0, -70), new Vector2(0, -150) };

        for (int i = 0; i < options.Length; i++)
        {
            int idx = i;
            GameObject btn = CreateWindowsButton(panel.transform, "Opt" + idx, options[idx], positions[idx], new Vector2(560, 65));
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (idx == correct)
                {
                    btn.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
                    StartCoroutine(DelayedComplete(type, 75, panel));
                }
                else
                {
                    StartCoroutine(FlashRed(btn));
                    btn.GetComponent<Button>().interactable = false;
                }
            });
        }
    }

    // ── PanicAttack: breathing exercise timer ────────────────────────────────
    void BuildPanicAttackUI(GameObject panel, GameManager.VictimType type)
    {
        UIFactory.CreateText(panel.transform, "H", "😰 Panikattacke – Atemübung!", Vector2.up * 290, 28, TextAnchor.MiddleCenter).color = Color.black;
        UIFactory.CreateText(panel.transform, "Sub", "Folge der Atemübung:\nEinatmen (4s) → Halten (4s) → Ausatmen (6s)\n3 Runden nötig.", new Vector2(0, 200), 18, TextAnchor.MiddleCenter).color = new Color(0.2f, 0.2f, 0.2f);

        Text phaseTxt = UIFactory.CreateText(panel.transform, "Phase", "Drücke START um zu beginnen", new Vector2(0, 60), 28, TextAnchor.MiddleCenter);
        phaseTxt.color = new Color(0.1f, 0.4f, 0.8f);

        Text countTxt = UIFactory.CreateText(panel.transform, "Count", "", new Vector2(0, -30), 64, TextAnchor.MiddleCenter);
        countTxt.color = Color.black;

        Text roundTxt = UIFactory.CreateText(panel.transform, "Rounds", "Runde 0 / 3", new Vector2(0, -120), 20, TextAnchor.MiddleCenter);
        roundTxt.color = new Color(0.3f, 0.3f, 0.3f);

        GameObject startBtn = CreateWindowsButton(panel.transform, "StartBtn", "START", new Vector2(0, -200), new Vector2(220, 65));
        startBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            startBtn.SetActive(false);
            StartCoroutine(BreathingRoutine(phaseTxt, countTxt, roundTxt, panel, type));
        });
    }

    IEnumerator BreathingRoutine(Text phaseTxt, Text countTxt, Text roundTxt, GameObject panel, GameManager.VictimType type)
    {
        string[] phaseNames = { "EINATMEN 🌬️", "HALTEN 😶", "AUSATMEN 💨" };
        float[] durations   = { 4f,             4f,            6f };
        Color[] colours     = { new Color(0.2f, 0.6f, 1f), new Color(0.8f, 0.7f, 0.1f), new Color(0.2f, 0.7f, 0.3f) };

        for (int round = 1; round <= 3; round++)
        {
            if (panel == null) yield break;
            roundTxt.text = "Runde " + round + " / 3";

            for (int phase = 0; phase < 3; phase++)
            {
                if (panel == null) yield break;
                phaseTxt.text  = phaseNames[phase];
                phaseTxt.color = colours[phase];
                float elapsed  = 0f;
                while (elapsed < durations[phase])
                {
                    if (panel == null) yield break;
                    countTxt.text = Mathf.Ceil(durations[phase] - elapsed).ToString("0");
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        countTxt.text = "✓";
        StartCoroutine(DelayedComplete(type, 70, panel));
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    IEnumerator DelayedComplete(GameManager.VictimType type, int coins, GameObject panel)
    {
        yield return new WaitForSeconds(1.2f);
        if (panel != null) Destroy(panel);
        gameManager.CompleteNewMission(type, coins);
    }

    IEnumerator FlashRed(GameObject target)
    {
        if (target == null) yield break;
        Image img = target.GetComponent<Image>();
        if (img == null) yield break;
        Color orig = img.color;
        img.color = new Color(0.9f, 0.2f, 0.2f);
        yield return new WaitForSeconds(0.35f);
        if (img != null) img.color = orig;
    }

    public void LocalizeMenuUI()
    {
        bool isDe = LocalizationManager.Instance == null || LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.DE;
        
        // --- Main View ---
        GameObject startBtnTxt = GameObject.Find("StartButton/Text");
        if (startBtnTxt != null)
        {
            startBtnTxt.GetComponent<Text>().text = isDe ? 
                (gameManager != null && gameManager.gameStarted ? "ZURÜCK ZUM SPIEL" : "SPIEL STARTEN") : 
                (gameManager != null && gameManager.gameStarted ? "BACK TO GAME" : "START GAME");
        }
        
        GameObject missionsBtnTxt = GameObject.Find("MissionsButton/Text");
        if (missionsBtnTxt != null) missionsBtnTxt.GetComponent<Text>().text = isDe ? "MISSIONEN" : "MISSIONS";
        
        GameObject settingsBtnTxt = GameObject.Find("SettingsButton/Text");
        if (settingsBtnTxt != null) settingsBtnTxt.GetComponent<Text>().text = isDe ? "EINSTELLUNGEN" : "SETTINGS";
        
        GameObject creditsBtnTxt = GameObject.Find("CreditsButton/Text");
        if (creditsBtnTxt != null) creditsBtnTxt.GetComponent<Text>().text = isDe ? "MITWIRKENDE" : "CREDITS";
        
        GameObject quitBtnTxt = GameObject.Find("QuitButton/Text");
        if (quitBtnTxt != null) quitBtnTxt.GetComponent<Text>().text = isDe ? "BEENDEN" : "QUIT";
        
        // --- Settings View ---
        GameObject settingsHeader = GameObject.Find("SettingsHeader");
        if (settingsHeader != null) settingsHeader.GetComponent<Text>().text = isDe ? "SYSTEM-EINSTELLUNGEN" : "SYSTEM SETTINGS";
        
        GameObject bgmLabel = GameObject.Find("BGMFrame/Label");
        if (bgmLabel != null) bgmLabel.GetComponent<Text>().text = isDe ? "Musik Lautstärke" : "Music Volume";
        
        GameObject sfxLabel = GameObject.Find("SFXFrame/Label");
        if (sfxLabel != null) sfxLabel.GetComponent<Text>().text = isDe ? "Effekt Lautstärke" : "SFX Volume";
        
        GameObject resetBtnTxt = GameObject.Find("ResetButton/Text");
        if (resetBtnTxt != null) resetBtnTxt.GetComponent<Text>().text = isDe ? "ZURÜCKSETZEN" : "RESET PROGRESS";
        
        GameObject dayNightBtnTxt = GameObject.Find("DayNightBtn/Text");
        if (dayNightBtnTxt != null) dayNightBtnTxt.GetComponent<Text>().text = isDe ? "TAG/NACHT" : "DAY/NIGHT";
        
        GameObject settingsBackBtnTxt = GameObject.Find("SettingsView/BackButton/Text");
        if (settingsBackBtnTxt == null) settingsBackBtnTxt = GameObject.Find("BackButton/Text");
        if (settingsBackBtnTxt != null) settingsBackBtnTxt.GetComponent<Text>().text = isDe ? "ZURÜCK" : "BACK";
        
        GameObject langBtnTxt = GameObject.Find("LanguageToggle/Text");
        if (langBtnTxt != null) langBtnTxt.GetComponent<Text>().text = isDe ? "SPRACHE: DE" : "LANGUAGE: EN";
        
        // Tutorial Box Content
        GameObject tutTxt = GameObject.Find("TutorialContent");
        if (tutTxt != null)
        {
            tutTxt.GetComponent<Text>().text = isDe ? 
                "<b>SPIEL-ANLEITUNG & STEUERUNG</b>\n" +
                "• <b>WASD / Pfeiltasten:</b> Bewege deine Spielfigur durch den Park.\n" +
                "• <b>E / Enter:</b> Interagiere mit verletzten Personen oder Gegenständen.\n" +
                "• <b>M-Taste:</b> Öffne dein Tablet für Badges, Checkliste und den Shop.\n" +
                "• <b>ESC / P-Taste:</b> Pausiere das Spiel und öffne das Hauptmenü.\n" +
                "• <b>Medizin-Taschen (+):</b> Sammle sie im Park, um Erste-Hilfe-Ausrüstung aufzufüllen.\n" +
                "• <b>Das Haus (Innenbereich):</b> Gehe zur Haustür unten im Park und drücke <b>E</b>, um das Haus zu betreten. Dort warten weitere Indoor-Szenarien (wie z.B. Verbrennung oder Vergiftung) auf dich!" 
                :
                "<b>GAME MANUAL & CONTROLS</b>\n" +
                "• <b>WASD / Arrow Keys:</b> Move your player character through the park.\n" +
                "• <b>E / Enter:</b> Interact with injured persons or equipment items.\n" +
                "• <b>M Key:</b> Open your tablet for badges, checklist, and the shop.\n" +
                "• <b>ESC / P Key:</b> Pause the game and open the main operating system.\n" +
                "• <b>Medkits (+):</b> Collect them in the park to refill your first aid equipment.\n" +
                "• <b>The House (Indoors):</b> Walk to the front door at the bottom of the park and press <b>E</b> to enter. More indoor scenarios (such as burns or poisoning) await you!";
        }
        
        // --- Missions View ---
        GameObject missionsHeader = GameObject.Find("MissionsHeader");
        if (missionsHeader != null) missionsHeader.GetComponent<Text>().text = isDe ? "VERFÜGBARE MISSIONEN" : "AVAILABLE MISSIONS";
        
        GameObject missionsBackBtnTxt = GameObject.Find("MissionsView/BackButton/Text");
        if (missionsBackBtnTxt != null) missionsBackBtnTxt.GetComponent<Text>().text = isDe ? "ZURÜCK" : "BACK";
        
        // --- Credits View ---
        GameObject creditsHeader = GameObject.Find("CreditsHeader");
        if (creditsHeader != null) creditsHeader.GetComponent<Text>().text = isDe ? "SYSTEM-INFORMATION (README)" : "SYSTEM INFORMATION (README)";
        
        GameObject credsTxt = GameObject.Find("CreditsContent");
        if (credsTxt != null)
        {
            credsTxt.GetComponent<Text>().text = isDe ? 
                "<b>FIRST AID SIMULATOR - PARK EDITION</b>\n" +
                "Ein interaktiver Simulator für Erste Hilfe in einer Parkumgebung.\n\n" +
                "<b>SCHÖPFER & MITWIRKENDE:</b>\n" +
                "• <b>Artjom Becker</b> (Projektleitung & Programmierung)\n" +
                "• <b>AG Serious Games</b> (TU Darmstadt)\n\n" +
                "<b>ASSET-CREDITS:</b>\n" +
                "• <b>Spieler & Map:</b> Cainos (cainos.itch.io)\n" +
                "• <b>Windows UI:</b> Comp-3 Interactive (comp3.itch.io)\n" +
                "• <b>Sounds UI:</b> Noah Kühne (Menu Sounds V2)\n" +
                "• <b>Musik & Medien:</b> Generiert von Gemini AI\n\n" +
                "<i>Erstellt für Bildungszwecke. Jede Sekunde zählt!</i>"
                :
                "<b>FIRST AID SIMULATOR - PARK EDITION</b>\n" +
                "An interactive simulator for first aid in a park environment.\n\n" +
                "<b>CREATOR & CONTRIBUTORS:</b>\n" +
                "• <b>Artjom Becker</b> (Project Lead & Programming)\n" +
                "• <b>AG Serious Games</b> (TU Darmstadt)\n\n" +
                "<b>ASSET CREDITS:</b>\n" +
                "• <b>Player & Map:</b> Cainos (cainos.itch.io)\n" +
                "• <b>Windows UI:</b> Comp-3 Interactive (comp3.itch.io)\n" +
                "• <b>UI Sounds:</b> Noah Kühne (Menu Sounds V2)\n" +
                "• <b>Music & Media:</b> Generated by Gemini AI\n\n" +
                "<i>Created for educational purposes. Every second counts!</i>";
        }
        
        GameObject credsBackBtnTxt = GameObject.Find("CreditsBackButton/Text");
        if (credsBackBtnTxt != null) credsBackBtnTxt.GetComponent<Text>().text = isDe ? "ZURÜCK" : "BACK";
        


        // --- Desktop Shortcuts ---
        GameObject certShortcutTxt = GameObject.Find("CertShortcut/Text");
        if (certShortcutTxt != null) certShortcutTxt.GetComponent<Text>().text = isDe ? "Zertifikat.exe" : "Certificate.exe";

        GameObject handbookShortcutTxt = GameObject.Find("HandbookShortcut/Text");
        if (handbookShortcutTxt != null) handbookShortcutTxt.GetComponent<Text>().text = isDe ? "Handbuch.exe" : "Handbook.exe";

        GameObject trackerShortcutTxt = GameObject.Find("TrackerShortcut/Text");
        if (trackerShortcutTxt != null) trackerShortcutTxt.GetComponent<Text>().text = isDe ? "Lernfortschritt.exe" : "Progress.exe";

        GameObject binShortcutTxt = GameObject.Find("BinShortcut/Text");
        if (binShortcutTxt != null) binShortcutTxt.GetComponent<Text>().text = isDe ? "Papierkorb" : "Recycle Bin";

        GameObject examShortcutTxt = GameObject.Find("ExamShortcut/Text");
        if (examShortcutTxt != null) examShortcutTxt.GetComponent<Text>().text = isDe ? "Prüfung.exe" : "Exam.exe";

        GameObject mineShortcutTxt = GameObject.Find("MineShortcut/Text");
        if (mineShortcutTxt != null) mineShortcutTxt.GetComponent<Text>().text = "Minesweeper.exe";

        // --- Menu Shortcuts ---
        GameObject certMenuBtnTxt = GameObject.Find("CertMenuBtn/Label");
        if (certMenuBtnTxt != null) certMenuBtnTxt.GetComponent<Text>().text = isDe ? "📜 Zertifikat" : "📜 Certificate";

        GameObject handbookMenuBtnTxt = GameObject.Find("HandbookMenuBtn/Label");
        if (handbookMenuBtnTxt != null) handbookMenuBtnTxt.GetComponent<Text>().text = isDe ? "📖 Handbuch" : "📖 Handbook";

        GameObject trackerMenuBtnTxt = GameObject.Find("TrackerMenuBtn/Label");
        if (trackerMenuBtnTxt != null) trackerMenuBtnTxt.GetComponent<Text>().text = isDe ? "📈 Fortschritt" : "📈 Progress";

        GameObject examMenuBtnTxt = GameObject.Find("ExamMenuBtn/Label");
        if (examMenuBtnTxt != null) examMenuBtnTxt.GetComponent<Text>().text = isDe ? "✍️ Prüfung" : "✍️ Exam";

        GameObject binMenuBtnTxt = GameObject.Find("BinMenuBtn/Label");
        if (binMenuBtnTxt != null) binMenuBtnTxt.GetComponent<Text>().text = isDe ? "🗑 Papierkorb" : "🗑 Recycle Bin";

        // --- Taskbar & Start Menu ---
        GameObject taskbarLangText = GameObject.Find("TaskbarLanguageToggle/Text");
        if (taskbarLangText != null) taskbarLangText.GetComponent<Text>().text = isDe ? "🌐 DE" : "🌐 EN";

        string[] smKeysDe = { "📖 Handbuch.exe", "📈 Lernfortschritt.exe", "🏆 Erfolge.exe", "✍️ Prüfung.exe", "📜 Zertifikat.exe", "🖼️ Darstellung.exe", "🐍 Snake.exe", "💣 Minesweeper.exe", "📧 BossMail.exe", "🏗️ ParkArchitect.exe", "💻 cmd.exe", "🗑️ Papierkorb", "🚪 Beenden" };
        string[] smKeysEn = { "📖 Handbook.exe", "📈 Progress.exe", "🏆 Achievements.exe", "✍️ Exam.exe", "📜 Certificate.exe", "🖼️ Display.exe", "🐍 Snake.exe", "💣 Minesweeper.exe", "📧 BossMail.exe", "🏗️ ParkArchitect.exe", "💻 cmd.exe", "🗑️ Recycle Bin", "🚪 Quit Game" };
        for (int i = 0; i < smKeysDe.Length; i++)
        {
            GameObject smItemText = GameObject.Find("StartMenuItem_" + i + "/Text");
            if (smItemText != null)
            {
                smItemText.GetComponent<Text>().text = isDe ? smKeysDe[i] : smKeysEn[i];
            }
        }

        // Refresh Missions
        MenuManager menuManager = Object.FindFirstObjectByType<MenuManager>();
        if (menuManager != null) menuManager.RefreshMissionsUI();
    }

    public GameObject CreateDesktopIcon(Transform parent, string name, string label, string emoji, Vector2 pos, System.Action onClick)
    {
        GameObject iconObj = new GameObject(name);
        iconObj.transform.SetParent(parent, false);

        RectTransform rt = iconObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(110, 100);

        // Clickable image with transparent background
        Image img = iconObj.AddComponent<Image>();
        img.color = Color.clear;

        Button btn = iconObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            onClick();
            if (buttonClickSound != null && gameManager != null && gameManager.sfxSource != null)
            {
                gameManager.sfxSource.PlayOneShot(buttonClickSound);
            }
        });

        // Emoji Icon (Text)
        GameObject emojiObj = new GameObject("Emoji");
        emojiObj.transform.SetParent(iconObj.transform, false);
        RectTransform emojiRT = emojiObj.AddComponent<RectTransform>();
        emojiRT.anchoredPosition = new Vector2(0, 15);
        emojiRT.sizeDelta = new Vector2(80, 50);

        Text emojiTxt = emojiObj.AddComponent<Text>();
        emojiTxt.raycastTarget = false;
        emojiTxt.text = emoji;
        emojiTxt.fontSize = 32;
        emojiTxt.alignment = TextAnchor.MiddleCenter;
        emojiTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Label Text (Named "Text" so GameObject.Find(name + "/Text") finds it)
        GameObject labelObj = new GameObject("Text");
        labelObj.transform.SetParent(iconObj.transform, false);
        RectTransform labelRT = labelObj.AddComponent<RectTransform>();
        labelRT.anchoredPosition = new Vector2(0, -25);
        labelRT.sizeDelta = new Vector2(100, 35);

        Text labelTxt = labelObj.AddComponent<Text>();
        labelTxt.raycastTarget = false;
        labelTxt.text = label;
        labelTxt.fontSize = 12;
        labelTxt.alignment = TextAnchor.UpperCenter;
        labelTxt.color = Color.white;
        labelTxt.fontStyle = FontStyle.Normal;
        labelTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelTxt.horizontalOverflow = HorizontalWrapMode.Wrap;

        // Shadow / Outline for readability on sky background
        Shadow shadow = labelObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(1, -1);

        // Hover Effect
        UIFactory.AddHoverEffect(iconObj, buttonHoverSound, gameManager?.sfxSource);
        iconObj.AddComponent<DesktopIconHighlight>().bgImage = img;

        return iconObj;
    }
}

public class DesktopIconHighlight : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public Image bgImage;
    private Color hoverColor = new Color(0.3f, 0.6f, 0.9f, 0.25f);

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = hoverColor;
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (bgImage != null) bgImage.color = Color.clear;
    }
    
    private void OnDisable()
    {
        if (bgImage != null) bgImage.color = Color.clear;
    }
}
