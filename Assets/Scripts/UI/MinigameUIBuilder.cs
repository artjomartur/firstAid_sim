using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class MinigameUIBuilder
{
public static void SetupEmergencyKitPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "EmergencyKitPanel", new Color(0, 0, 0, 0.85f));
        panel.SetActive(false);
        gameManager.emergencyKitPanel = panel;
        
        EmergencyKitManager ekm = panel.AddComponent<EmergencyKitManager>();
        gameManager.emergencyKitManager = ekm;
        ekm.gameManager = gameManager;
        ekm.emergencyKitPanel = panel;
        
        ekm.kitDropSound = bootstrap.buttonClickSound;
        ekm.binDropSound = bootstrap.buttonClickSound;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "EmergencyKitDialog", "Notfallkoffer-Planer.exe", Vector2.zero, new Vector2(850, 750));

        // Shelf Frame / Item Container (where items spawn)
        GameObject shelfFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "ShelfFrame", new Vector2(0, 140), new Vector2(750, 320));
        UIFactory.SetupImage(shelfFrame, bootstrap.winInnerFrameSprite, false);
        
        // Inside the shelf, create a container for grid spawning
        GameObject itemsGrid = UIFactory.CreateUIElement(shelfFrame.transform as RectTransform, "ItemsGrid", Vector2.zero, new Vector2(750, 320));
        ekm.itemContainer = itemsGrid.GetComponent<RectTransform>();

        // Left Target: Emergency Kit Box (Drop Zone)
        GameObject kitFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "KitFrame", new Vector2(-190, -70), new Vector2(340, 140));
        UIFactory.SetupImage(kitFrame, bootstrap.winInnerFrameSprite, false);
        Text kitLabel = UIFactory.CreateText(kitFrame.transform, "KitLabel", "💼 IN DEN KOFFER", Vector2.zero, 24, TextAnchor.MiddleCenter);
        kitLabel.color = new Color(0.1f, 0.4f, 0.1f);
        kitLabel.fontStyle = FontStyle.Bold;
        DropZone kitDZ = kitFrame.AddComponent<DropZone>();
        ekm.kitDropZone = kitDZ;

        // Right Target: Trash Bin (Drop Zone)
        GameObject binFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "BinFrame", new Vector2(190, -70), new Vector2(340, 140));
        UIFactory.SetupImage(binFrame, bootstrap.winInnerFrameSprite, false);
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
        UIFactory.SetupImage(feedbackFrame, bootstrap.winInnerFrameSprite, false);

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
        GameObject finishBtnObj = bootstrap.CreateWindowsButton(dialog.transform, "FinishBtn", "BEWERTUNG ABSCHLIESSEN", new Vector2(0, -315), new Vector2(350, 50));
        Button finishBtn = finishBtnObj.GetComponent<Button>();
        finishBtn.onClick.AddListener(() => ekm.OnFinishButtonPressed());
        ekm.finishButton = finishBtn;
    }

    public static void SetupTriagePanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "TriagePanel", new Color(0, 0, 0, 0.8f));
        panel.SetActive(false);
        gameManager.triagePanel = panel;
        
        TriageManager tm = panel.AddComponent<TriageManager>();
        gameManager.triageManager = tm;
        tm.gameManager = gameManager;
        tm.triagePanel = panel;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "TriageDialog", "Triage-System.exe", Vector2.zero, new Vector2(1000, 850));

        // Illustration in Inner Frame
        GameObject imgFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "ImageFrame", new Vector2(0, 180), new Vector2(900, 400));
        UIFactory.SetupImage(imgFrame, bootstrap.winInnerFrameSprite, false);

        GameObject groupObj = UIFactory.CreateUIElement(imgFrame.transform as RectTransform, "TriageIllustration", Vector2.zero, new Vector2(880, 380));
        groupObj.GetComponent<Image>().sprite = bootstrap.triageGroupSprite;
        groupObj.GetComponent<Image>().preserveAspect = true;

        // Info Text in Inner Frame
        GameObject infoFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InfoBox", new Vector2(0, -100), new Vector2(900, 120));
        UIFactory.SetupImage(infoFrame, bootstrap.winInnerFrameSprite, false);
        tm.victimInfoText = UIFactory.CreateText(infoFrame.transform, "InfoText", "...", Vector2.zero, 30, TextAnchor.MiddleCenter);
        tm.victimInfoText.color = Color.black;
        tm.victimInfoText.fontStyle = FontStyle.Bold;

        // Windows Buttons Container
        GameObject btnContainer = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Buttons", new Vector2(0, -250), new Vector2(900, 100));
        btnContainer.GetComponent<Image>().color = new Color(0, 0, 0, 0); // Transparent container
        
        GameObject blackBtn = bootstrap.CreateWindowsButton(btnContainer.transform, "BlackBtn", "SCHWARZ", new Vector2(-345, 0), new Vector2(210, 70));
        blackBtn.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f); // Dark gray tint
        blackBtn.GetComponentInChildren<Text>().color = Color.white; // White text for contrast
        blackBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(3));

        GameObject redBtn = bootstrap.CreateWindowsButton(btnContainer.transform, "RedBtn", "ROT (Akut)", new Vector2(-115, 0), new Vector2(210, 70));
        redBtn.GetComponent<Image>().color = new Color(1f, 0.6f, 0.6f); // Light red tint
        redBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(0));

        GameObject yellowBtn = bootstrap.CreateWindowsButton(btnContainer.transform, "YellowBtn", "GELB (Schwer)", new Vector2(115, 0), new Vector2(210, 70));
        yellowBtn.GetComponent<Image>().color = new Color(1f, 1f, 0.6f); // Light yellow tint
        yellowBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(1));

        GameObject greenBtn = bootstrap.CreateWindowsButton(btnContainer.transform, "GreenBtn", "GRÜN (Leicht)", new Vector2(345, 0), new Vector2(210, 70));
        greenBtn.GetComponent<Image>().color = new Color(0.6f, 1f, 0.6f); // Light green tint
        greenBtn.GetComponent<Button>().onClick.AddListener(() => tm.AssignTriage(2));
    }

    public static void SetupHeatstrokePanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "HeatstrokePanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.heatstrokePanel = panel;
        
        HeatstrokeManager hm = panel.AddComponent<HeatstrokeManager>();
        gameManager.heatstrokeManager = hm;
        hm.gameManager = gameManager;
        hm.heatstrokePanel = panel;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "HeatstrokeDialog", "Hitzeschlag-Assistent.exe", Vector2.zero, new Vector2(1000, 750));

        // Inner Frame for Instructions & Avatar
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InnerFrame", new Vector2(0, 160), new Vector2(900, 240));
        UIFactory.SetupImage(innerFrame, bootstrap.winInnerFrameSprite, false);

        GameObject avatarObj = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Avatar", new Vector2(-320, 0), new Vector2(200, 200));
        Image avImg = avatarObj.GetComponent<Image>();
        avImg.sprite = bootstrap.heatstrokePersonSprite;
        avImg.preserveAspect = true;
        avImg.raycastTarget = false;

        hm.instructionText = UIFactory.CreateText(innerFrame.transform, "Instructions", "...", new Vector2(100, 0), 24, TextAnchor.MiddleLeft);
        hm.instructionText.color = Color.black;
        hm.instructionText.fontStyle = FontStyle.Bold;
        hm.instructionText.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 220);
        hm.instructionText.raycastTarget = false;

        // Windows Style Buttons
        GameObject b1 = bootstrap.CreateWindowsButton(dialog.transform, "ShadowBtn", "IN DEN SCHATTEN BRINGEN", new Vector2(0, -60), new Vector2(600, 60));
        hm.shadowButton = b1.GetComponent<Button>();
        hm.shadowButton.onClick.AddListener(() => hm.MoveToShadow());

        GameObject b2 = bootstrap.CreateWindowsButton(dialog.transform, "LegsBtn", "BEINE HOCHLAGERN", new Vector2(0, -140), new Vector2(600, 60));
        hm.legsButton = b2.GetComponent<Button>();
        hm.legsButton.onClick.AddListener(() => hm.RaiseLegs());

        GameObject b3 = bootstrap.CreateWindowsButton(dialog.transform, "WaterBtn", "WASSER GEBEN (SCHLÜCKCHEN)", new Vector2(0, -220), new Vector2(600, 60));
        hm.waterButton = b3.GetComponent<Button>();
        hm.waterButton.onClick.AddListener(() => hm.GiveWater());
    }

    public static void SetupChokingPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "ChokingPanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.chokingPanel = panel;
        
        ChokingManager cm = panel.AddComponent<ChokingManager>();
        gameManager.chokingManager = cm;
        cm.gameManager = gameManager;
        cm.chokingPanel = panel;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "ChokingDialog", "Heimlich-Assistent.exe", Vector2.zero, new Vector2(1000, 750));

        // Inner Frame for Instructions & Avatar
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InnerFrame", new Vector2(0, 160), new Vector2(900, 260));
        UIFactory.SetupImage(innerFrame, bootstrap.winInnerFrameSprite, false);

        GameObject avatarObj = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Avatar", new Vector2(-320, 0), new Vector2(220, 220));
        avatarObj.GetComponent<Image>().sprite = bootstrap.chokingPersonSprite;
        avatarObj.GetComponent<Image>().preserveAspect = true;

        cm.instructionText = UIFactory.CreateText(innerFrame.transform, "Instructions", "...", new Vector2(100, 0), 28, TextAnchor.MiddleLeft);
        cm.instructionText.color = Color.black;
        cm.instructionText.fontStyle = FontStyle.Bold;

        // Slider in retro frame
        GameObject sliderFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "SliderFrame", new Vector2(0, -110), new Vector2(700, 160));
        UIFactory.SetupImage(sliderFrame, bootstrap.winInnerFrameSprite, false);

        Text sliderTxt = UIFactory.CreateText(sliderFrame.transform, "Label", "Stoßstärke (LEERTASTE im grünen Bereich drücken)", new Vector2(0, 50), 24, TextAnchor.MiddleCenter);
        sliderTxt.color = Color.black;
        sliderTxt.fontStyle = FontStyle.Bold;

        Slider fSlider = UIFactory.CreateSlider(sliderFrame.transform, "ForceSlider", new Vector2(0, -20), new Vector2(600, 45), Color.red, null, null);
        cm.forceSlider = fSlider;
        cm.fillImage = fSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();
    }

    public static void SetupPoisonPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "PoisonPanel", new Color(0, 0.08f, 0.02f, 0.92f));
        panel.SetActive(false);
        gameManager.poisonPanel = panel;

        PoisonManager pm = panel.AddComponent<PoisonManager>();
        gameManager.poisonManager = pm;
        pm.gameManager = gameManager;
        pm.poisonPanel = panel;

        // Main dialog window
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "PoisonDialog",
            "Vergiftungs-Assistent.exe", Vector2.zero, new Vector2(900, 660));

        // Shared instruction text (top)
        GameObject infoFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform,
            "InfoFrame", new Vector2(0, 235), new Vector2(840, 80));
        UIFactory.SetupImage(infoFrame, bootstrap.winInnerFrameSprite, false);
        Text instTxt = UIFactory.CreateText(infoFrame.transform, "InstructionText",
            "...", Vector2.zero, 20, TextAnchor.MiddleCenter);
        instTxt.color = Color.black;
        instTxt.fontStyle = FontStyle.Bold;
        instTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(810, 72);
        pm.instructionText = instTxt;

        // === PHASE 1: Examine button ===
        GameObject examineObj = bootstrap.CreateWindowsButton(dialog.transform, "ExamineBtn",
            "PILZE UNTERSUCHEN", new Vector2(0, 130), new Vector2(500, 65));
        pm.examineButton = examineObj.GetComponent<Button>();
        pm.examineButton.onClick.AddListener(() => pm.OnExamineClicked());

        // === PHASE 2: Phone dialer ===
        GameObject phonePanel = UIFactory.CreateUIElement(dialog.transform as RectTransform,
            "PhonePanel", new Vector2(0, 20), new Vector2(820, 430));
        UIFactory.SetupImage(phonePanel, bootstrap.winInnerFrameSprite, false);
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
        UIFactory.SetupImage(displayFrame, bootstrap.winInnerFrameSprite, false);
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
                "Key_" + d, d, keyPos[i], new Vector2(110, 62), bootstrap.winButtonSprite);
            Text kt = keyObj.GetComponentInChildren<Text>();
            if (kt != null) { kt.fontSize = 26; kt.fontStyle = FontStyle.Bold; kt.color = Color.black; }
            Button kb = keyObj.GetComponent<Button>();
            pm.digitButtons[i] = kb;
            kb.onClick.AddListener(() => pm.OnDigitPressed(d));
        }

        // Delete button
        GameObject delObj = UIFactory.CreateButton(phonePanel.GetComponent<RectTransform>(),
            "DeleteBtn", "<", new Vector2(75, -200), new Vector2(110, 62), bootstrap.winButtonSprite);
        delObj.GetComponent<Image>().color = new Color(1f, 0.88f, 0.88f);
        Text delT = delObj.GetComponentInChildren<Text>();
        if (delT != null) { delT.fontSize = 22; delT.color = new Color(0.6f, 0.1f, 0.1f); }
        pm.deleteButton = delObj.GetComponent<Button>();
        pm.deleteButton.onClick.AddListener(() => pm.OnDeletePressed());

        // Call button (green)
        GameObject callObj = UIFactory.CreateButton(phonePanel.GetComponent<RectTransform>(),
            "CallDialBtn", "ANRUFEN", new Vector2(220, -200), new Vector2(200, 62), bootstrap.winButtonSprite);
        callObj.GetComponent<Image>().color = new Color(0.7f, 1f, 0.7f);
        Text callT = callObj.GetComponentInChildren<Text>();
        if (callT != null) { callT.fontSize = 18; callT.fontStyle = FontStyle.Bold; callT.color = new Color(0f, 0.35f, 0f); }
        pm.callDialButton = callObj.GetComponent<Button>();
        pm.callDialButton.onClick.AddListener(() => pm.OnCallPressed());

        // === PHASE 3: Water panel ===
        GameObject waterPanel = UIFactory.CreateUIElement(dialog.transform as RectTransform,
            "WaterPanel", new Vector2(0, 60), new Vector2(820, 280));
        UIFactory.SetupImage(waterPanel, bootstrap.winInnerFrameSprite, false);
        waterPanel.SetActive(false);
        pm.waterPanel = waterPanel;

        Text waterInfo = UIFactory.CreateText(waterPanel.transform, "WaterInfo",
            "Der Giftnotruf raet:\nStilles Wasser oder Tee in kleinen Schlueckchen geben.\nKEIN Erbrechen ausloesen!",
            new Vector2(0, 55), 19, TextAnchor.MiddleCenter);
        waterInfo.color = new Color(0.05f, 0.2f, 0.45f);
        waterInfo.fontStyle = FontStyle.Bold;
        waterInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 120);

        GameObject waterObj = bootstrap.CreateWindowsButton(waterPanel.transform, "WaterBtn",
            "WASSER GEBEN", new Vector2(0, -80), new Vector2(400, 65));
        waterObj.GetComponent<Image>().color = new Color(0.85f, 0.93f, 1f);
        pm.waterButton = waterObj.GetComponent<Button>();
        pm.waterButton.onClick.AddListener(() => pm.OnWaterClicked());
    }

    public static void SetupElectricShockPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "ElectricShockPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        panel.SetActive(false);
        gameManager.shockPanel = panel;
        
        ElectricShockManager em = panel.AddComponent<ElectricShockManager>();
        gameManager.shockManager = em;
        em.shockPanel = panel;
        em.gameManager = gameManager;

        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "ShockDialog", "Sicherungskasten", Vector2.zero, new Vector2(800, 600));

        Text instTxt = UIFactory.CreateText(dialog.transform, "InstructionText", "Folge den Anweisungen!", new Vector2(0, 200), 28, TextAnchor.MiddleCenter);
        instTxt.color = Color.black;
        em.instructionText = instTxt;

        // Visual Layout: Fuse Box Container
        GameObject fuseContainer = UIFactory.CreateUIElement(dialog.transform as RectTransform, "FuseBox", new Vector2(0, -50), new Vector2(600, 250));
        UIFactory.SetupImage(fuseContainer, bootstrap.winInnerFrameSprite, false);

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

    public static void SetupBurnPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "BurnPanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.burnPanel = panel;
        
        BurnManager bm = panel.AddComponent<BurnManager>();
        gameManager.burnManager = bm;
        bm.gameManager = gameManager;
        bm.burnPanel = panel;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "BurnDialog", "Kühlungs-Assistent.exe", Vector2.zero, new Vector2(1000, 750));

        // Left Column: Arm Illustration in Window Frame
        GameObject leftFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "LeftFrame", new Vector2(-220, 20), new Vector2(420, 520));
        UIFactory.SetupImage(leftFrame, bootstrap.winInnerFrameSprite, false);

        GameObject arm = UIFactory.CreateUIElement(leftFrame.transform as RectTransform, "BurnArm", Vector2.zero, new Vector2(380, 480));
        arm.GetComponent<Image>().sprite = bootstrap.burnArmSprite;
        arm.GetComponent<Image>().preserveAspect = true;

        // Right Column: Instructions
        GameObject instructionsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InstructionsFrame", new Vector2(230, 210), new Vector2(420, 140));
        UIFactory.SetupImage(instructionsFrame, bootstrap.winInnerFrameSprite, false);
        bm.instructionText = UIFactory.CreateText(instructionsFrame.transform, "Instructions", "...", Vector2.zero, 26, TextAnchor.MiddleCenter);
        bm.instructionText.color = Color.black;
        bm.instructionText.fontStyle = FontStyle.Bold;

        // Temperature Control Frame
        GameObject tempFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "TempFrame", new Vector2(230, 30), new Vector2(420, 140));
        UIFactory.SetupImage(tempFrame, bootstrap.winInnerFrameSprite, false);

        Text tempTxt = UIFactory.CreateText(tempFrame.transform, "Label", "Wasser-Temperatur regeln", new Vector2(0, 40), 22, TextAnchor.MiddleCenter);
        tempTxt.color = Color.black;
        tempTxt.fontStyle = FontStyle.Bold;

        Slider tSlider = UIFactory.CreateSlider(tempFrame.transform, "TempSlider", new Vector2(0, -20), new Vector2(360, 35), Color.blue, null, null);
        bm.temperatureSlider = tSlider;
        bm.temperatureLabelText = tempTxt;
        bm.temperatureFill = tSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();

        // Progress Control Frame
        GameObject progressFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "ProgressFrame", new Vector2(230, -150), new Vector2(420, 140));
        UIFactory.SetupImage(progressFrame, bootstrap.winInnerFrameSprite, false);

        Text progressTxt = UIFactory.CreateText(progressFrame.transform, "Label", "Fortschritt Kühlung (100% Ziel)", new Vector2(0, 40), 22, TextAnchor.MiddleCenter);
        progressTxt.color = Color.black;
        progressTxt.fontStyle = FontStyle.Bold;

        Slider pSlider = UIFactory.CreateSlider(progressFrame.transform, "ProgressSlider", new Vector2(0, -20), new Vector2(360, 30), Color.green, null, null);
        bm.progressSlider = pSlider;
    }

    public static void SetupAEDPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "AEDPanel", new Color(0, 0, 0, 0.9f));
        panel.SetActive(false);
        gameManager.aedPanel = panel;
        
        AEDManager am = panel.AddComponent<AEDManager>();
        gameManager.aedManager = am;
        am.gameManager = gameManager;
        am.aedPanel = panel;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "AEDDialog", "Defibrillator-Assistent.exe", Vector2.zero, new Vector2(1000, 850));

        // Instructions inside an inner frame at the top
        GameObject infoFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InfoBox", new Vector2(0, 320), new Vector2(900, 100));
        UIFactory.SetupImage(infoFrame, bootstrap.winInnerFrameSprite, false);
        am.instructionText = UIFactory.CreateText(infoFrame.transform, "Instructions", "...", Vector2.zero, 28, TextAnchor.MiddleCenter);
        am.instructionText.color = Color.black;
        am.instructionText.fontStyle = FontStyle.Bold;

        // Victim body visualization
        GameObject victim = UIFactory.CreateUIElement(dialog.transform as RectTransform, "VictimBody", new Vector2(120, -50), new Vector2(450, 600));
        victim.GetComponent<Image>().sprite = bootstrap.aedBodySprite;
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
        p1.GetComponent<Image>().sprite = bootstrap.aedPadSprite;
        am.pad1 = p1.GetComponent<RectTransform>();

        GameObject p2 = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Pad2", new Vector2(-320, -150), new Vector2(120, 120));
        p2.GetComponent<Image>().sprite = bootstrap.aedPadSprite;
        am.pad2 = p2.GetComponent<RectTransform>();

        // Shock Button (Classic Windows Button styled in red)
        GameObject shockBtn = bootstrap.CreateWindowsButton(dialog.transform, "ShockButton", "SCHOCK AUSLÖSEN", new Vector2(-320, -320), new Vector2(240, 60));
        shockBtn.GetComponent<Image>().color = new Color(1f, 0.6f, 0.6f); // Light red tint
        am.shockButton = shockBtn.GetComponent<Button>();
        am.shockButton.onClick.AddListener(() => am.OnShockPressed());
    }

    public static void SetupCallPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "CallPanel", new Color(0, 0, 0, 0.95f));
        panel.SetActive(false);
        gameManager.callPanel = panel;
        
        EmergencyCallManager ecm = panel.AddComponent<EmergencyCallManager>();
        gameManager.callManager = ecm;
        ecm.gameManager = gameManager;
        ecm.callPanel = panel;
        ecm.telephoneSound = bootstrap.telephoneSound;
        ecm.muffledTalkSound = bootstrap.muffledTalkSound;

        // Windows Dialog Container
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "CallDialog", "112 - Leitstelle.exe", Vector2.zero, new Vector2(1000, 750));

        // Inner Frame for Dispatcher Text
        GameObject innerFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "InnerFrame", new Vector2(0, 150), new Vector2(900, 200));
        UIFactory.SetupImage(innerFrame, bootstrap.winInnerFrameSprite, false);

        // Dispatcher Avatar inside Frame (moved to the right)
        GameObject avatarObj = UIFactory.CreateUIElement(innerFrame.transform as RectTransform, "Avatar", new Vector2(350, 0), new Vector2(160, 160));
        avatarObj.GetComponent<Image>().sprite = bootstrap.dispatcherSprite;
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
            GameObject btnObj = bootstrap.CreateWindowsButton(dialog.transform, "Answer_" + i, "Antwort", new Vector2(0, -60 - (i * 90)), new Vector2(800, 70));
            ecm.answerButtons[i] = btnObj.GetComponent<Button>();
            int idx = i;
            ecm.answerButtons[idx].onClick.AddListener(() => ecm.OnAnswerSelected(idx));
        }
    }

    public static void SetupStoryPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
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
        UIFactory.SetupImage(textBox, bootstrap.winBaseSprite, false);
        ism.storyTextBox = textBox;
        
        // Inner Frame for the Text
        GameObject innerFrame = UIFactory.CreateUIElement(textBox.transform as RectTransform, "InnerFrame", Vector2.zero, new Vector2(1560, 210));
        UIFactory.SetupImage(innerFrame, bootstrap.winInnerFrameSprite, false);

        ism.storyText = UIFactory.CreateText(innerFrame.transform, "Text", "Ein neuer Tag beginnt...", Vector2.zero, 38, TextAnchor.MiddleCenter);
        ism.storyText.color = Color.black;
        ism.storyText.fontStyle = FontStyle.Bold;

        ism.storyVideoClips = bootstrap.storyVideoClips;

        // Skip Button as Windows Button (for static slideshow)
        GameObject skipBtn = bootstrap.CreateWindowsButton(textBox.transform, "SkipBtn", "ÜBERSPRINGEN", new Vector2(600, -80), new Vector2(220, 60));
        skipBtn.GetComponent<Button>().onClick.AddListener(() => ism.SkipStory());

        // Dedicated Corner Skip Button specifically for the Fullscreen Video
        GameObject videoSkipBtn = bootstrap.CreateWindowsButton(panel.transform, "VideoSkipBtn", "ÜBERSPRINGEN", new Vector2(800, -460), new Vector2(240, 70));
        videoSkipBtn.SetActive(false); // Hide by default, will be activated only when video starts
        videoSkipBtn.GetComponent<Button>().onClick.AddListener(() => ism.SkipStory());
        ism.videoSkipButton = videoSkipBtn;
    }

    public static void SetupShopPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
    {
        GameObject panel = UIFactory.CreateFullscreenPanel(parent, "ShopPanel", new Color(0, 0, 0, 0.8f));
        panel.SetActive(false);
        gameManager.shopPanel = panel;
        ShopManager shopManager = panel.AddComponent<ShopManager>();
        shopManager.gameManager = gameManager;

        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "ShopDialog", "Ausrüstung-Shop.exe", Vector2.zero, new Vector2(900, 800));

        GameObject coinsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "CoinsFrame", new Vector2(0, 300), new Vector2(300, 60));
        UIFactory.SetupImage(coinsFrame, bootstrap.winInnerFrameSprite, false);
        shopManager.coinsText = UIFactory.CreateText(coinsFrame.transform, "Text", "Coins: 0", Vector2.zero, 30, TextAnchor.MiddleCenter);
        shopManager.coinsText.color = Color.black;
        shopManager.coinsText.fontStyle = FontStyle.Bold;

        // Item Grid
        float startY = 160;
        bootstrap.CreateShopItem(dialog.transform, "Steady Rhythm", "CPR-Takt verlangsamen", "50", new Vector2(0, startY), () => shopManager.BuySteadyRhythm());
        bootstrap.CreateShopItem(dialog.transform, "Sharp Eye", "Quiz-Hilfe aktivieren", "30", new Vector2(0, startY - 120), () => shopManager.BuySharpEye());
        bootstrap.CreateShopItem(dialog.transform, "Pro Bandage", "Verband-Fehlertoleranz+", "40", new Vector2(0, startY - 240), () => shopManager.BuyProBandage());
        
        // Skin Colors (Color Pickers wrapped in Windows Buttons)
        GameObject skinRow = UIFactory.CreateUIElement(dialog.transform as RectTransform, "Skins", new Vector2(0, -220), new Vector2(600, 100));
        bootstrap.CreateColorButton(skinRow.transform, Color.red, "#FF0000", new Vector2(-150, 0));
        bootstrap.CreateColorButton(skinRow.transform, Color.blue, "#0000FF", new Vector2(-50, 0));
        bootstrap.CreateColorButton(skinRow.transform, Color.green, "#00FF00", new Vector2(50, 0));
        bootstrap.CreateColorButton(skinRow.transform, Color.yellow, "#FFFF00", new Vector2(150, 0));

        GameObject closeBtn = bootstrap.CreateWindowsButton(dialog.transform, "CloseBtn", "ZURÜCK", new Vector2(0, -340), new Vector2(300, 60));
        closeBtn.GetComponent<Button>().onClick.AddListener(() => gameManager.CloseShop());
    }

    public static void SetupBoneFracturePanel(RectTransform parent, GameManager gameManager, LevelManager lm)
    {
        GameObject panel = UIFactory.CreateUIElement(parent, "BoneFracturePanel", Vector2.zero, new Vector2(800, 600));
        UIFactory.SetupImage(panel, lm.bootstrap.winBaseSprite, true);
        panel.GetComponent<Image>().type = Image.Type.Sliced;
        
        UIFactory.CreateText(panel.transform, "Title", "KNOCHENBRUCH", new Vector2(0, 250), 36, TextAnchor.MiddleCenter);
        Text instruction = UIFactory.CreateText(panel.transform, "InstructionText", "ZIEHE DIE SCHIENE AUF DAS GEBROCHENE BEIN!", new Vector2(0, 180), 24, TextAnchor.MiddleCenter);

        // Splint Image (DragItem)
        GameObject splint = UIFactory.CreateUIElement(panel.GetComponent<RectTransform>(), "SplintItem", new Vector2(0, -150), new Vector2(100, 200));
        UIFactory.SetupImage(splint, lm.bootstrap.aedPadSprite, true); // reusing pad sprite for now
        DragItem di = splint.AddComponent<DragItem>();

        // Leg Image (DropZone)
        GameObject leg = UIFactory.CreateUIElement(panel.GetComponent<RectTransform>(), "LegDropZone", new Vector2(0, 0), new Vector2(200, 400));
        UIFactory.SetupImage(leg, lm.bootstrap.burnArmSprite, true); // reusing arm sprite as leg placeholder
        DropZone dz = leg.AddComponent<DropZone>();

        BoneFractureManager bm = panel.AddComponent<BoneFractureManager>();
        bm.boneFracturePanel = panel;
        bm.instructionText = instruction;
        bm.splintItem = di;
        bm.legDropZone = dz;

        panel.SetActive(false);
    }

    public static void SetupAllergicShockPanel(RectTransform parent, GameManager gameManager, LevelManager lm)
    {
        GameObject panel = UIFactory.CreateUIElement(parent, "AllergicShockPanel", Vector2.zero, new Vector2(800, 600));
        UIFactory.SetupImage(panel, lm.bootstrap.winBaseSprite, true);
        panel.GetComponent<Image>().type = Image.Type.Sliced;
        
        UIFactory.CreateText(panel.transform, "Title", "ALLERGISCHER SCHOCK", new Vector2(0, 250), 36, TextAnchor.MiddleCenter);
        Text instruction = UIFactory.CreateText(panel.transform, "InstructionText", "ZIEHE DEN EPIPEN AUF DEN OBERSCHENKEL!", new Vector2(0, 180), 24, TextAnchor.MiddleCenter);

        // EpiPen Image (DragItem)
        GameObject epipen = UIFactory.CreateUIElement(panel.GetComponent<RectTransform>(), "EpiPenItem", new Vector2(0, -150), new Vector2(50, 150));
        UIFactory.SetupImage(epipen, lm.bootstrap.shopIcon, true); // reusing shop icon for now
        DragItem di = epipen.AddComponent<DragItem>();

        // Thigh Image (DropZone)
        GameObject thigh = UIFactory.CreateUIElement(panel.GetComponent<RectTransform>(), "ThighDropZone", new Vector2(0, 0), new Vector2(200, 300));
        UIFactory.SetupImage(thigh, lm.bootstrap.burnArmSprite, true); // reusing arm sprite
        DropZone dz = thigh.AddComponent<DropZone>();

        AllergicShockManager am = panel.AddComponent<AllergicShockManager>();
        am.allergicShockPanel = panel;
        am.instructionText = instruction;
        am.epiPenItem = di;
        am.thighDropZone = dz;

        panel.SetActive(false);
    }
}