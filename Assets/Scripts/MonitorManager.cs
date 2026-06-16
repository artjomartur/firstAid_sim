using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MonitorManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject monitorPanel;
    
    public Text coinsText;
    public Text statsText;
    public Text missionsText;

    private bool pausedForMonitor;

    public void ToggleMonitor()
    {
        if (monitorPanel == null) return;

        if (monitorPanel.activeSelf)
            CloseMonitor();
        else
            OpenMonitor();
    }

    public void OpenMonitor()
    {
        if (monitorPanel == null) return;

        monitorPanel.transform.SetAsLastSibling();
        monitorPanel.SetActive(true);
        UpdateUI();

        if (gameManager != null)
        {
            if (gameManager.currentPhase != GameManager.GamePhase.Intro
                && gameManager.currentPhase != GameManager.GamePhase.Shop
                && Time.timeScale > 0f)
            {
                Time.timeScale = 0f;
                pausedForMonitor = true;
            }

            if (gameManager.sfxSource != null)
            {
                if (gameManager.monitorOpenSound != null)
                    gameManager.sfxSource.PlayOneShot(gameManager.monitorOpenSound);
                else if (gameManager.unpauseSound != null)
                    gameManager.sfxSource.PlayOneShot(gameManager.unpauseSound);
            }
        }
    }

    public void CloseMonitor()
    {
        if (monitorPanel == null) return;

        monitorPanel.SetActive(false);

        if (pausedForMonitor)
        {
            Time.timeScale = 1f;
            pausedForMonitor = false;
        }

        if (gameManager != null && gameManager.sfxSource != null)
        {
            if (gameManager.monitorCloseSound != null)
                gameManager.sfxSource.PlayOneShot(gameManager.monitorCloseSound);
            else if (gameManager.pauseSound != null)
                gameManager.sfxSource.PlayOneShot(gameManager.pauseSound);
        }
    }

    public void UpdateUI()
    {
        if (coinsText == null || statsText == null || missionsText == null)
            return;

        if (ScoreManager.Instance != null)
        {
            coinsText.text = "Guthaben: " + ScoreManager.Instance.lifeCoins + " Coins";
            
            string badges = string.Join("\n", ScoreManager.Instance.GetBadges());
            statsText.text = "Aktive Abzeichen:\n" + (string.IsNullOrEmpty(badges) ? "- Keine -" : badges);
        }
        else
        {
            coinsText.text = "Guthaben: 0 Coins";
            statsText.text = "Aktive Abzeichen:\n- Keine -";
        }

        if (gameManager != null)
        {
            gameManager.SyncMissionProgress();

            List<string> missions = new List<string>();
            foreach (var mission in gameManager.availableMissions)
            {
                if (mission.hasPlayed)
                    missions.Add("[x] " + mission.title);
            }

            missionsText.text = "Einsatz-Protokoll:\n"
                + (missions.Count == 0 ? "Noch keine Einsätze absolviert." : string.Join("\n", missions));
        }
    }
}
