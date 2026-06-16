using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int quizScore = 0;
    public int maxQuizScore = 3;
    public int bandageScore = 0;
    public int maxBandageScore = 1;
    public int cprScore = 0;
    public int maxCprScore = 30;
    public int lifeCoins = 0;
    public bool emergencyKitPerfect = false;
    
    [Header("Mission Action Stats")]
    public int currentMissionCorrect = 0;
    public int currentMissionErrors = 0;

    public void ResetMissionStats()
    {
        currentMissionCorrect = 0;
        currentMissionErrors = 0;
    }

    public void RecordSuccess()
    {
        currentMissionCorrect++;
    }

    public void RecordError()
    {
        currentMissionErrors++;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        lifeCoins += amount;
        SaveProgress();
        Debug.Log($"Coins added: {amount}. Total: {lifeCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (lifeCoins >= amount)
        {
            lifeCoins -= amount;
            SaveProgress();
            return true;
        }
        return false;
    }

    public List<string> GetBadges()
    {
        List<string> badges = new List<string>();
        if (quizScore == maxQuizScore) badges.Add("🧠 Quiz-Master");
        else if (quizScore >= maxQuizScore / 2) badges.Add("📖 Theorie-Versteher");

        if (cprScore >= maxCprScore) badges.Add("❤️ Perfekter Lebensretter");
        else if (cprScore >= maxCprScore - 10) badges.Add("👍 Solider Helfer");

        if (bandageScore >= maxBandageScore) badges.Add("🩹 Verband-Profi");

        if (emergencyKitPerfect) badges.Add("💼 Koffer-Profi");

        if (badges.Count == 0) badges.Add("📚 Erste-Hilfe-Schüler");
        
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            if (gm.boneFractureHelped) badges.Add("🦴 Knochen-Profi");
            if (gm.allergicShockHelped) badges.Add("💉 Allergie-Retter");
            if (gm.dogPoisoningHelped) badges.Add("🐶 Tierretter");
        }
        
        return badges;
    }

    public void ResetScores()
    {
        quizScore = 0;
        bandageScore = 0;
        cprScore = 0;
        emergencyKitPerfect = false;
        lifeCoins = 0;

        PlayerPrefs.DeleteKey("lifeCoins");
        PlayerPrefs.DeleteKey("quizScore");
        PlayerPrefs.DeleteKey("bandageScore");
        PlayerPrefs.DeleteKey("cprScore");
        PlayerPrefs.DeleteKey("emergencyKitPerfect");

        PlayerPrefs.DeleteKey("helped_bikeAccident");
        PlayerPrefs.DeleteKey("helped_bleedingWound");
        PlayerPrefs.DeleteKey("helped_unconscious");
        PlayerPrefs.DeleteKey("helped_burnInjury");
        PlayerPrefs.DeleteKey("helped_choking");
        PlayerPrefs.DeleteKey("helped_heatstroke");
        PlayerPrefs.DeleteKey("helped_triage");
        PlayerPrefs.DeleteKey("helped_electricShock");
        PlayerPrefs.DeleteKey("helped_poisoning");
        PlayerPrefs.DeleteKey("helped_boneFracture");
        PlayerPrefs.DeleteKey("helped_allergicShock");
        PlayerPrefs.DeleteKey("helped_drowning");
        PlayerPrefs.DeleteKey("helped_diabeticShock");
        PlayerPrefs.DeleteKey("helped_panicAttack");
        PlayerPrefs.DeleteKey("helped_stroke");
        PlayerPrefs.DeleteKey("helped_dogPoisoning");
        PlayerPrefs.DeleteKey("helped_heartAttack");
        PlayerPrefs.DeleteKey("helped_snakebite");

        PlayerPrefs.Save();
        Debug.Log("ResetScores: All player progress cleared!");
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("lifeCoins", lifeCoins);
        PlayerPrefs.SetInt("quizScore", quizScore);
        PlayerPrefs.SetInt("bandageScore", bandageScore);
        PlayerPrefs.SetInt("cprScore", cprScore);
        PlayerPrefs.SetInt("emergencyKitPerfect", emergencyKitPerfect ? 1 : 0);

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            PlayerPrefs.SetInt("helped_bikeAccident", gm.bikeAccidentHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_bleedingWound", gm.bleedingWoundHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_unconscious", gm.unconsciousHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_burnInjury", gm.burnInjuryHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_choking", gm.chokingHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_heatstroke", gm.heatstrokeHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_triage", gm.triageHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_electricShock", gm.electricShockHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_poisoning", gm.poisoningHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_boneFracture", gm.boneFractureHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_allergicShock", gm.allergicShockHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_drowning", gm.drowningHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_diabeticShock", gm.diabeticShockHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_panicAttack", gm.panicAttackHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_stroke", gm.strokeHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_dogPoisoning", gm.dogPoisoningHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_heartAttack", gm.heartAttackHelped ? 1 : 0);
            PlayerPrefs.SetInt("helped_snakebite", gm.snakebiteHelped ? 1 : 0);
        }
        PlayerPrefs.Save();
        Debug.Log("Progress saved successfully!");
    }

    public void LoadProgress()
    {
        lifeCoins = PlayerPrefs.GetInt("lifeCoins", 0);
        quizScore = PlayerPrefs.GetInt("quizScore", 0);
        bandageScore = PlayerPrefs.GetInt("bandageScore", 0);
        cprScore = PlayerPrefs.GetInt("cprScore", 0);
        emergencyKitPerfect = PlayerPrefs.GetInt("emergencyKitPerfect", 0) == 1;

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.bikeAccidentHelped = PlayerPrefs.GetInt("helped_bikeAccident", 0) == 1;
            gm.bleedingWoundHelped = PlayerPrefs.GetInt("helped_bleedingWound", 0) == 1;
            gm.unconsciousHelped = PlayerPrefs.GetInt("helped_unconscious", 0) == 1;
            gm.burnInjuryHelped = PlayerPrefs.GetInt("helped_burnInjury", 0) == 1;
            gm.chokingHelped = PlayerPrefs.GetInt("helped_choking", 0) == 1;
            gm.heatstrokeHelped = PlayerPrefs.GetInt("helped_heatstroke", 0) == 1;
            gm.triageHelped = PlayerPrefs.GetInt("helped_triage", 0) == 1;
            gm.electricShockHelped = PlayerPrefs.GetInt("helped_electricShock", 0) == 1;
            gm.poisoningHelped = PlayerPrefs.GetInt("helped_poisoning", 0) == 1;
            gm.boneFractureHelped = PlayerPrefs.GetInt("helped_boneFracture", 0) == 1;
            gm.allergicShockHelped = PlayerPrefs.GetInt("helped_allergicShock", 0) == 1;
            gm.drowningHelped = PlayerPrefs.GetInt("helped_drowning", 0) == 1;
            gm.diabeticShockHelped = PlayerPrefs.GetInt("helped_diabeticShock", 0) == 1;
            gm.panicAttackHelped = PlayerPrefs.GetInt("helped_panicAttack", 0) == 1;
            gm.strokeHelped = PlayerPrefs.GetInt("helped_stroke", 0) == 1;
            gm.dogPoisoningHelped = PlayerPrefs.GetInt("helped_dogPoisoning", 0) == 1;
            gm.heartAttackHelped = PlayerPrefs.GetInt("helped_heartAttack", 0) == 1;
            gm.snakebiteHelped = PlayerPrefs.GetInt("helped_snakebite", 0) == 1;
            gm.SyncMissionProgress();
        }
        Debug.Log("Progress loaded successfully!");
    }
}
