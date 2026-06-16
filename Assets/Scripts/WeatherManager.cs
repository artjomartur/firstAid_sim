using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    public enum WeatherType { Clear, Rain, Fog }
    public WeatherType currentWeather = WeatherType.Clear;

    private GameObject weatherOverlay;
    private Image overlayImage;
    private ParticleSystem rainSystem;

    private float timer = 0f;
    private float weatherChangeInterval = 45f; // Change weather every 45 seconds

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        CreateWeatherUI();
        StartCoroutine(WeatherRoutine());
    }

    private void CreateWeatherUI()
    {
        // Add to main canvas
        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (canvasObj == null) canvasObj = Object.FindFirstObjectByType<Canvas>()?.gameObject;
        if (canvasObj == null) return;

        weatherOverlay = new GameObject("WeatherOverlay");
        weatherOverlay.transform.SetParent(canvasObj.transform, false);
        
        // Put it behind Windows but above Map (first sibling so it renders behind the windows)
        weatherOverlay.transform.SetAsFirstSibling();

        RectTransform rt = weatherOverlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        overlayImage = weatherOverlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0); // Transparent by default
        overlayImage.raycastTarget = false; // Never block clicks

        // Basic Rain System
        GameObject rainObj = new GameObject("RainParticles");
        rainObj.transform.SetParent(weatherOverlay.transform, false);
        RectTransform rainRT = rainObj.AddComponent<RectTransform>();
        rainRT.anchorMin = Vector2.zero;
        rainRT.anchorMax = Vector2.one;
        rainRT.sizeDelta = Vector2.zero;

        rainSystem = rainObj.AddComponent<ParticleSystem>();
        rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        var main = rainSystem.main;
        main.playOnAwake = false;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = 1000f; // Fast moving
        main.startSize = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 500;

        var emission = rainSystem.emission;
        emission.rateOverTime = 0; // Off by default

        var shape = rainSystem.shape;
        shape.shapeType = ParticleSystemShapeType.BoxEdge;
        shape.scale = new Vector3(800, 1, 1);
        shape.position = new Vector3(0, 500, 0);
        shape.rotation = new Vector3(10, 0, 0); // slight angle

        var renderer = rainSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.2f;
        renderer.lengthScale = 2f;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        rainSystem.Stop();
    }

    private IEnumerator WeatherRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(weatherChangeInterval);
            
            // Do not change weather during minigames
            if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.GamePhase.Intro)
            {
                ChangeWeatherRandomly();
            }
        }
    }

    private void ChangeWeatherRandomly()
    {
        int r = Random.Range(0, 3);
        currentWeather = (WeatherType)r;

        StopAllCoroutines();
        StartCoroutine(WeatherRoutine()); // Restart timer
        StartCoroutine(TransitionWeather());
    }

    private IEnumerator TransitionWeather()
    {
        Color targetColor = new Color(0, 0, 0, 0);
        float targetEmission = 0f;

        if (currentWeather == WeatherType.Fog)
        {
            targetColor = new Color(0.8f, 0.85f, 0.9f, 0.4f); // Fog color
            targetEmission = 0f;
        }
        else if (currentWeather == WeatherType.Rain)
        {
            targetColor = new Color(0.2f, 0.25f, 0.35f, 0.2f); // Darker
            targetEmission = 200f;
            if (!rainSystem.isPlaying) rainSystem.Play();
        }
        else
        {
            // Clear
            targetColor = new Color(0, 0, 0, 0);
            targetEmission = 0f;
        }

        Color startColor = overlayImage.color;
        var emission = rainSystem.emission;
        float startEmission = emission.rateOverTime.constant;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f; // 2 second transition
            
            if (overlayImage != null)
                overlayImage.color = Color.Lerp(startColor, targetColor, t);
                
            emission.rateOverTime = Mathf.Lerp(startEmission, targetEmission, t);
            
            yield return null;
        }

        if (currentWeather != WeatherType.Rain)
        {
            rainSystem.Stop();
        }
    }
}
