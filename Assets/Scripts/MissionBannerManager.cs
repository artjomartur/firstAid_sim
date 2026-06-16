using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MissionBannerManager : MonoBehaviour
{
    public RectTransform bannerRect;
    public Text bannerText;
    
    private Coroutine currentAnimation;

    public void ShowBanner(int rewardCoins, string customMessage = null)
    {
        gameObject.SetActive(true);

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        string message = "MISSION ERFOLGREICH!";
        if (rewardCoins > 0)
        {
            message += "\n+" + rewardCoins + " Coins";
        }
        
        if (!string.IsNullOrEmpty(customMessage))
        {
            message = customMessage;
        }

        bannerText.text = message;
        currentAnimation = StartCoroutine(AnimateBanner());
    }

    private IEnumerator AnimateBanner()
    {
        
        // Anchor top center, so Y represents distance from top
        Vector2 startPos = new Vector2(0, 200);  // Off-screen above
        Vector2 endPos = new Vector2(0, -80);    // On-screen below top edge
        
        // Slide in
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; // ~0.33s
            bannerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        // Hold on screen
        yield return new WaitForSeconds(2.5f);
        
        // Slide out
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            bannerRect.anchoredPosition = Vector2.Lerp(endPos, startPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
}
