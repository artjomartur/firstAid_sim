using UnityEngine;

/// <summary>
/// Pixel-Skies-Hintergrund (Parallax oder Einzelbild), folgt der Kamera und liegt hinter der Map.
/// </summary>
[DisallowMultipleComponent]
public class PixelSkyBackground : MonoBehaviour
{
    [Header("Parallax (Demo 02 empfohlen)")]
    public bool useParallax = true;
    public Sprite[] parallaxLayers;
    [Tooltip("Je höher, desto stärker bewegt sich die Ebene mit der Kamera.")]
    public float[] parallaxFactors = { 0.02f, 0.05f, 0.1f };

    [Header("Einzel-Himmel (Full HD)")]
    public Sprite singleSkySprite;

    [Header("Darstellung")]
    public string sortingLayerName = "Default";
    public int sortingOrder = -100;
    public float customScale = 1f;
    public Vector2 skyOffset = Vector2.zero;

    private Transform skyRoot;
    private Transform[] layerTransforms;
    private Vector3 cameraStartPos;
    private Camera targetCamera;
    private float coverScale = 1f;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
            targetCamera = GetComponentInParent<Camera>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogWarning("[PixelSkyBackground] Keine Kamera gefunden.");
            enabled = false;
            return;
        }

        cameraStartPos = targetCamera.transform.position;

        GameObject rootObj = new GameObject("PixelSky");
        rootObj.transform.SetParent(targetCamera.transform, false);
        rootObj.transform.localPosition = new Vector3(0f, 0f, 10f);
        skyRoot = rootObj.transform;

        BuildSky();
    }

    void LateUpdate()
    {
        if (targetCamera == null || skyRoot == null || layerTransforms == null) return;

        Vector3 camPos = targetCamera.transform.position;
        skyRoot.position = new Vector3(camPos.x, camPos.y, targetCamera.transform.position.z + 10f);

        if (!useParallax || parallaxLayers == null || parallaxLayers.Length == 0) return;

        Vector3 delta = camPos - cameraStartPos;
        for (int i = 0; i < layerTransforms.Length; i++)
        {
            if (layerTransforms[i] == null) continue;
            float factor = i < parallaxFactors.Length ? parallaxFactors[i] : 0.05f;
            layerTransforms[i].localPosition = new Vector3(
                delta.x * factor + skyOffset.x,
                delta.y * factor + skyOffset.y,
                (i + 1) * 0.01f
            );
        }
    }

    void BuildSky()
    {
        if (skyRoot == null) return;

        for (int i = skyRoot.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(skyRoot.GetChild(i).gameObject);
            else
                DestroyImmediate(skyRoot.GetChild(i).gameObject);
        }

        if (useParallax && parallaxLayers != null && parallaxLayers.Length > 0)
            BuildParallaxLayers();
        else if (singleSkySprite != null)
            BuildSingleLayer(singleSkySprite, "PixelSky_Single", 0f);
        else
            Debug.LogWarning("[PixelSkyBackground] Keine Sky-Sprites zugewiesen.");
    }

    void BuildParallaxLayers()
    {
        layerTransforms = new Transform[parallaxLayers.Length];
        coverScale = CalculateCoverScale(parallaxLayers[0]);

        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            if (parallaxLayers[i] == null) continue;
            float z = (i + 1) * 0.01f;
            layerTransforms[i] = CreateLayer(
                parallaxLayers[i],
                "PixelSky_Layer" + (i + 1),
                sortingOrder + i,
                z
            );
        }
    }

    void BuildSingleLayer(Sprite sprite, string objName, float localZ)
    {
        coverScale = CalculateCoverScale(sprite);
        layerTransforms = new Transform[1];
        layerTransforms[0] = CreateLayer(sprite, objName, sortingOrder, localZ);
    }

    Transform CreateLayer(Sprite sprite, string objName, int order, float localZ)
    {
        GameObject go = new GameObject(objName);
        go.transform.SetParent(skyRoot, false);
        go.transform.localPosition = new Vector3(skyOffset.x, skyOffset.y, localZ);
        go.transform.localScale = Vector3.one * coverScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = order;
        sr.drawMode = SpriteDrawMode.Simple;

        return go.transform;
    }

    float CalculateCoverScale(Sprite sprite)
    {
        return customScale;
    }

#if UNITY_EDITOR
    void Reset()
    {
        if (parallaxLayers != null && parallaxLayers.Length > 0) return;

        parallaxLayers = new Sprite[3];
        parallaxLayers[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Pixel Skies DEMO/Parallax Pixel Skies 240x135px/Demo 02 Parallax Pixel Sky/demo02_PixelSky_layer01.png");
        parallaxLayers[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Pixel Skies DEMO/Parallax Pixel Skies 240x135px/Demo 02 Parallax Pixel Sky/demo02_PixelSky_layer02.png");
        parallaxLayers[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Pixel Skies DEMO/Parallax Pixel Skies 240x135px/Demo 02 Parallax Pixel Sky/demo02_PixelSky_layer03.png");
        singleSkySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Pixel Skies DEMO/Pixel Skies 1920x1080px (Full HD)/demo02_PixelSky_1920x1080.png");
    }
#endif
}
