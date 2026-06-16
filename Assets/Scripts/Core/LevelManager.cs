using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public GameBootstrap bootstrap;

    // Extracted Fields
    public Transform victimBikeTransform;
    public Transform victimBleedTransform;
    public Transform victimUnconsciousTransform;
    private Transform burnVictimTransform;
    private Transform chokingVictimTransform;
    private Transform heatstrokeVictimTransform;
    private Transform triageVictimTransform;
    public Transform shockVictimTransform;
    public Transform poisonVictimTransform;
    private RectTransform arrowRect;
    private RectTransform interactionPromptRect;
    private Text interactionPromptText;
    public GameManager.VictimType? currentNearbyVictim = null;
    private Transform medkitTransform;
    private bool isNearMedkit = false;
    private GameObject victimsContainer;
    private Transform activeArrowTarget;

    // Extracted Methods
    public void InitMissions()
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
            GameObject v = bootstrap.CreateWorldObject("Victim_Bike", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimBikeTransform = v.transform;
            bootstrap.SetupSprite(v, bootstrap.victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));
        gameManager.availableMissions.Add(CreateMission("m16", "Vergiftung", GameManager.VictimType.Poisoning, new Color(0.6f, 0.2f, 0.8f), false, (pos, m) => {
            GameObject v = bootstrap.CreateWorldObject("Victim_Poison", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            poisonVictimTransform = v.transform;
            bootstrap.SetupSprite(v, bootstrap.unconsciousVictimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        // (Park Aufräumen was moved to run parallel in Start())

        gameManager.availableMissions.Add(CreateMission("m2", "Schnittwunde", GameManager.VictimType.BleedingWound, Color.red, false, (pos, m) => {
            GameObject v = bootstrap.CreateWorldObject("Victim_Bleeding", pos, new Vector3(entityScale, entityScale, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimBleedTransform = v.transform;
            bootstrap.SetupSprite(v, bootstrap.victimSprite);
            m.activeInstance = v;
            SetPlayerArrow(v.transform);
        }));

        gameManager.availableMissions.Add(CreateMission("m3", "Bewusstlose Person", GameManager.VictimType.UnconsciousPerson, Color.gray, true, (pos, m) => {
            GameObject v = bootstrap.CreateWorldObject("Victim_Unconscious", pos, new Vector3(entityScale * 1.2f, entityScale * 1.2f, 1));
            v.transform.SetParent(victimsContainer.transform);
            victimUnconsciousTransform = v.transform;
            bootstrap.SetupSprite(v, bootstrap.unconsciousVictimSprite != null ? bootstrap.unconsciousVictimSprite : bootstrap.victimSprite);
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

        
        gameManager.availableMissions.Add(CreateMission("m11", "Knochenbruch", GameManager.VictimType.BoneFracture, new Color(0.8f, 0.4f, 0.1f), true, (pos, m) => {
            GameObject v = bootstrap.CreateWorldObject("Victim_BoneFracture", pos, new Vector3(0.08f, 0.08f, 1));
            bootstrap.SetupSprite(v, bootstrap.victimSprite);
            v.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.4f, 0.1f);
            m.activeInstance = v;
        }));

        gameManager.availableMissions.Add(CreateMission("m12", "Allergischer Schock", GameManager.VictimType.AllergicShock, new Color(0.2f, 0.8f, 0.2f), false, (pos, m) => {
            GameObject v = bootstrap.CreateWorldObject("Victim_AllergicShock", pos, new Vector3(0.08f, 0.08f, 1));
            bootstrap.SetupSprite(v, bootstrap.unconsciousVictimSprite);
            v.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.8f, 0.2f);
            m.activeInstance = v;
        }));

        gameManager.availableMissions.Add(CreateMission("m10", "Massenunfall (Triage)", GameManager.VictimType.TriageScene, Color.blue, false, (pos, m) => {
            GameObject v = SpawnVictim(pos, "TriageGroup", "Massenunfall mit mehreren Verletzten", GameManager.VictimType.TriageScene, Color.blue);
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
        sr.sprite = bootstrap.unconsciousVictimSprite;
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

        return v;
    }

    public void SetupHouses()
    {
        // 1. Create the Interior Room (Far away from the main map)
        Vector3 interiorPos = new Vector3(100f, 100f, 0f);
        GameObject interiorRoom = new GameObject("InteriorRoom");
        interiorRoom.transform.position = interiorPos;

        // Simple interior floor using a big black box or sprite
        GameObject floor = new GameObject("Floor");
        floor.transform.SetParent(interiorRoom.transform);
        floor.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        // Re-use a sprite (or just leave it empty if no sprite is easily accessible, we'll use a color tint)
        sr.sprite = bootstrap.unconsciousVictimSprite; // fallback sprite
        sr.color = new Color(0.1f, 0.1f, 0.1f);
        floor.transform.localScale = new Vector3(5f, 5f, 1f);

        // 2. Create the Exterior Door (on the main map, near the start)
        Vector3 exteriorPos = new Vector3(-2f, -1f, 0f);
        GameObject exteriorDoor = new GameObject("HouseExteriorDoor");
        exteriorDoor.transform.position = exteriorPos;
        BoxCollider2D extCol = exteriorDoor.AddComponent<BoxCollider2D>();
        extCol.isTrigger = true;
        extCol.size = new Vector2(2f, 2f);
        
        DoorTransition extDoor = exteriorDoor.AddComponent<DoorTransition>();
        
        // 3. Create the Interior Door (inside the room to exit)
        GameObject interiorDoor = new GameObject("HouseInteriorDoor");
        interiorDoor.transform.position = interiorPos + new Vector3(0f, -2f, 0f);
        BoxCollider2D intCol = interiorDoor.AddComponent<BoxCollider2D>();
        intCol.isTrigger = true;
        intCol.size = new Vector2(2f, 2f);
        
        DoorTransition intDoor = interiorDoor.AddComponent<DoorTransition>();

        // Link them up
        extDoor.targetPosition = interiorDoor.transform;
        intDoor.targetPosition = exteriorDoor.transform;

        // Visual indicator for doors (optional)
        SpriteRenderer extSr = exteriorDoor.AddComponent<SpriteRenderer>();
        extSr.sprite = bootstrap.victimSprite;
        extSr.color = new Color(0.6f, 0.4f, 0.2f); // brown door
        extSr.sortingOrder = 10;
        exteriorDoor.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        SpriteRenderer intSr = interiorDoor.AddComponent<SpriteRenderer>();
        intSr.sprite = bootstrap.victimSprite;
        intSr.color = new Color(0.6f, 0.4f, 0.2f); // brown door
        intSr.sortingOrder = 10;
        interiorDoor.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
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

    public void SetupArrow(GameObject canvas)
    {
        GameObject arrow = UIFactory.CreateUIElement(canvas.GetComponent<RectTransform>(), "DirectionArrow", Vector2.zero, new Vector2(60, 60));
        arrowRect = arrow.GetComponent<RectTransform>();
        UIFactory.SetupImage(arrow, bootstrap.arrowSprite, true);
    }

    public void SetupInteractionPrompt(GameObject canvas)
    {
        GameObject prompt = UIFactory.CreateUIElement(canvas.GetComponent<RectTransform>(), "InteractionPrompt", new Vector2(0, 100), new Vector2(350, 60));
        interactionPromptRect = prompt.GetComponent<RectTransform>();
        UIFactory.SetupImage(prompt, bootstrap.winInnerFrameSprite, false);
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

    public void SetupAmbulance()
    {
        GameObject ambObj = bootstrap.CreateWorldObject("Ambulance", Vector3.zero, new Vector3(0.1f, 0.1f, 1));
        ambObj.SetActive(false);
        
        AmbulanceManager am = ambObj.AddComponent<AmbulanceManager>();
        am.ambulanceTransform = ambObj.transform;
        bootstrap.SetupSprite(ambObj, bootstrap.ambulanceSprite);
        am.ambulanceRenderer = ambObj.GetComponent<SpriteRenderer>();
        
        gameManager.ambulanceManager = am;

        // Spawn Medkit near ambulance spawn point (0, 0)
        GameObject medkit = bootstrap.CreateWorldObject("Medkit", new Vector3(0, -1.5f, 0), new Vector3(0.06f, 0.06f, 1));
        bootstrap.SetupSprite(medkit, bootstrap.coinIcon); // using coin icon as placeholder for medkit
        medkit.GetComponent<SpriteRenderer>().color = Color.red;
        medkitTransform = medkit.transform;


    }

    
}