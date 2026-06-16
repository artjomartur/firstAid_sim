using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WebGL tweaks for itch.io: let the browser set resolution, scale UI canvases correctly.
/// </summary>
public static class WebGLRuntimeFix
{
    private static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Apply()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ApplyCanvasScalers();
        ApplyScrollRects();
        Debug.Log($"WebGLRuntimeFix: screen {Screen.width}x{Screen.height}");
#endif
    }

    private static void ApplyCanvasScalers()
    {
        CanvasScaler[] scalers = Object.FindObjectsByType<CanvasScaler>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (CanvasScaler scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static void ApplyScrollRects()
    {
        ScrollRect[] scrollRects = Object.FindObjectsByType<ScrollRect>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (ScrollRect scroll in scrollRects)
        {
            scroll.scrollSensitivity = Mathf.Max(scroll.scrollSensitivity, 80f);
            scroll.inertia = true;
        }
    }
}
