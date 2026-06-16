using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class CoreUIBuilder
{
public static void SetupMissionBannerPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
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

        UIFactory.SetupImage(banner, bootstrap.winBaseSprite, false); // Use solid Windows base styling
        banner.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f); // Solid retro light-gray

        // Add an inner frame for aesthetics
        GameObject innerFrame = UIFactory.CreateUIElement(banner.transform as RectTransform, "InnerFrame", Vector2.zero, new Vector2(480, 80));
        UIFactory.SetupImage(innerFrame, bootstrap.winInnerFrameSprite, false);
        innerFrame.GetComponent<Image>().color = Color.white;

        mbm.bannerRect = rt;
        
        // Banner Text
        Text t = UIFactory.CreateText(innerFrame.transform, "Text", "MISSION ERFOLGREICH!", Vector2.zero, 28, TextAnchor.MiddleCenter);
        t.color = Color.black;
        t.fontStyle = FontStyle.Bold;
        mbm.bannerText = t;
    }

    public static void SetupMonitorPanel(RectTransform parent, GameManager gameManager, GameBootstrap bootstrap)
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
        GameObject dialog = bootstrap.CreateWindowsDialog(panel.transform, "MonitorDialog", "System-Monitor.exe", Vector2.zero, new Vector2(800, 700));

        // Close Button (Top Right)
        GameObject closeBtn = bootstrap.CreateWindowsButton(dialog.transform, "CloseBtn", "X", new Vector2(360, 315), new Vector2(40, 40));
        closeBtn.GetComponent<Button>().onClick.AddListener(() => mm.CloseMonitor());

        // Finances / Coins section
        GameObject coinFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "CoinFrame", new Vector2(0, 200), new Vector2(700, 80));
        UIFactory.SetupImage(coinFrame, bootstrap.winInnerFrameSprite, false);
        mm.coinsText = UIFactory.CreateText(coinFrame.transform, "Coins", "Guthaben: 0 Coins", Vector2.zero, 32, TextAnchor.MiddleCenter);
        mm.coinsText.color = Color.black;
        mm.coinsText.fontStyle = FontStyle.Bold;

        // Statistics / Badges section
        GameObject statsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "StatsFrame", new Vector2(0, 40), new Vector2(700, 200));
        UIFactory.SetupImage(statsFrame, bootstrap.winInnerFrameSprite, false);
        mm.statsText = UIFactory.CreateText(statsFrame.transform, "Stats", "Aktive Abzeichen:\n- Keine -", Vector2.zero, 28, TextAnchor.MiddleCenter);
        mm.statsText.color = new Color(0.2f, 0.2f, 0.6f); // Dark blue for retro feel
        mm.statsText.fontStyle = FontStyle.Bold;

        // Missions section
        GameObject missionsFrame = UIFactory.CreateUIElement(dialog.transform as RectTransform, "MissionsFrame", new Vector2(0, -180), new Vector2(700, 200));
        UIFactory.SetupImage(missionsFrame, bootstrap.winInnerFrameSprite, false);
        mm.missionsText = UIFactory.CreateText(missionsFrame.transform, "Missions", "Einsatz-Protokoll:\nNoch keine Einsätze absolviert.", Vector2.zero, 24, TextAnchor.MiddleCenter);
        mm.missionsText.color = new Color(0.1f, 0.4f, 0.1f); // Dark green
        mm.missionsText.fontStyle = FontStyle.Bold;

        // Start Quiz Button
        GameObject quizBtn = bootstrap.CreateWindowsButton(dialog.transform, "QuizBtn", "WISSENS-QUIZ", new Vector2(-250, -310), new Vector2(220, 60));
        quizBtn.GetComponent<Button>().onClick.AddListener(() => {
            mm.CloseMonitor();
            gameManager.StartQuizPhase();
        });

        // Start Koffer Training Button
        GameObject kitBtn = bootstrap.CreateWindowsButton(dialog.transform, "KitBtn", "KOFFER-TRAINING", new Vector2(0, -310), new Vector2(220, 60));
        kitBtn.GetComponent<Button>().onClick.AddListener(() => {
            mm.CloseMonitor();
            gameManager.StartEmergencyKitPhase();
        });

        // Open Shop Button
        GameObject shopBtn = bootstrap.CreateWindowsButton(dialog.transform, "ShopBtn", "AUSRÜSTUNG SHOP", new Vector2(250, -310), new Vector2(220, 60));
        shopBtn.GetComponent<Button>().onClick.AddListener(() => {
            mm.CloseMonitor();
            gameManager.StartShopPhase();
        });
    }

    
}
