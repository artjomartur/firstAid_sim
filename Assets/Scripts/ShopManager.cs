using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public Text coinsText;
    public GameManager gameManager;
    
    // Power-up states
    public bool steadyRhythmActive = false;
    public bool sharpEyeActive = false;
    public bool proBandageActive = false;
    public Color playerColor = Color.white;

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (coinsText != null && ScoreManager.Instance != null)
        {
            coinsText.text = "Coins: " + ScoreManager.Instance.lifeCoins;
        }
    }

    public void BuySteadyRhythm()
    {
        if (steadyRhythmActive) return;
        if (ScoreManager.Instance != null && ScoreManager.Instance.SpendCoins(50))
        {
            steadyRhythmActive = true;
            UpdateUI();
            Debug.Log("Bought Steady Rhythm!");
        }
    }

    public void BuySharpEye()
    {
        if (sharpEyeActive) return;
        if (ScoreManager.Instance != null && ScoreManager.Instance.SpendCoins(30))
        {
            sharpEyeActive = true;
            UpdateUI();
            Debug.Log("Bought Sharp Eye!");
        }
    }

    public void BuyProBandage()
    {
        if (proBandageActive) return;
        if (ScoreManager.Instance != null && ScoreManager.Instance.SpendCoins(40))
        {
            proBandageActive = true;
            UpdateUI();
            Debug.Log("Bought Pro Bandage!");
        }
    }

    public void BuyColor(string colorHex)
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.SpendCoins(100))
        {
            Color newCol;
            if (ColorUtility.TryParseHtmlString(colorHex, out newCol))
            {
                playerColor = newCol;
                UpdatePlayerColor();
                UpdateUI();
                Debug.Log("Bought New Color: " + colorHex);
            }
        }
    }

    private void UpdatePlayerColor()
    {
        PlayerController pCtrl = Object.FindAnyObjectByType<PlayerController>();
        GameObject player = pCtrl != null ? pCtrl.gameObject : null;
        if (player != null)
        {
            SpriteRenderer[] srs = player.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.color = playerColor;
            }
        }
    }
}
