using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public enum TimeMode { Cycle, AlwaysDay, AlwaysNight }

    [Header("Settings")]
    public TimeMode timeMode = TimeMode.Cycle;
    public float cycleDurationInMinutes = 4f;

    [Header("Parallax Backgrounds (Optional)")]
    [Tooltip("Zieh hier die Tag-Hintergrund-Ebenen rein")]
    public SpriteRenderer[] dayBackgroundLayers;
    [Tooltip("Zieh hier die Nacht-Hintergrund-Ebenen rein")]
    public SpriteRenderer[] nightBackgroundLayers;

    [Header("State")]
    [Range(0f, 1f)]
    public float timeOfDay = 0.5f; // 0 = day, 0.5 = night, 1 = day
    public bool isTimePassing = true;
    public bool isIndoors = false;

    private Camera cam;
    
    // Expanded arrays including left/right clones
    private SpriteRenderer[] allDayLayers;
    private SpriteRenderer[] allNightLayers;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (timeMode == TimeMode.Cycle && isTimePassing)
        {
            float speed = 1f / (cycleDurationInMinutes * 60f);
            timeOfDay += speed * Time.deltaTime;
            if (timeOfDay > 1f) timeOfDay = 0f;
        }
        else if (timeMode == TimeMode.AlwaysDay)
        {
            timeOfDay = 0f;
        }
        else if (timeMode == TimeMode.AlwaysNight)
        {
            timeOfDay = 0.5f;
        }

        // Day-Night intensity: 0 = full day, 1 = full night
        float nightIntensity = (Mathf.Sin((timeOfDay - 0.25f) * Mathf.PI * 2f) + 1f) / 2f;

        // Use FindObjectsOfType to grab all clones created by ParallaxBackground dynamically
        if (allDayLayers == null) allDayLayers = FindObjectsOfType<SpriteRenderer>();

        if (!isIndoors)
        {
            if (dayBackgroundLayers != null)
            {
                foreach (var sr in allDayLayers)
                {
                    if (sr != null && (sr.name.Contains("day_layer") || sr.name.Contains("demo01_PixelSky"))) 
                    {
                        Color c = sr.color; c.a = 1f - nightIntensity; sr.color = c;
                    }
                }
            }
            if (nightBackgroundLayers != null)
            {
                foreach (var sr in allDayLayers)
                {
                    if (sr != null && sr.name.Contains("night_layer"))
                    {
                        Color c = sr.color; c.a = nightIntensity; sr.color = c;
                    }
                }
            }
        }
    }


}
