#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Ensures story videos are transcoded for WebGL (required for browser playback).
/// Unity 6 uses GetTargetSettings(string) / SetTargetSettings(string), not GetPlatformOverride.
/// </summary>
public class WebGLVideoImportFix : AssetPostprocessor
{
    private const string WebGLPlatform = "WebGL";

    private void OnPreprocessAsset()
    {
        if (assetImporter is not VideoClipImporter videoImporter)
            return;

        if (!assetPath.StartsWith("Assets/Video/"))
            return;

        ApplyWebGLSettings(videoImporter);
    }

    private static void ApplyWebGLSettings(VideoClipImporter videoImporter)
    {
        try
        {
            VideoImporterTargetSettings settings = videoImporter.GetTargetSettings(WebGLPlatform);
            settings.enableTranscoding = true;
            settings.codec = VideoCodec.VP8;
            videoImporter.SetTargetSettings(WebGLPlatform, settings);
        }
        catch
        {
            // Unity 6 may label the platform "Web" on some versions
            try
            {
                VideoImporterTargetSettings settings = videoImporter.GetTargetSettings("Web");
                settings.enableTranscoding = true;
                settings.codec = VideoCodec.VP8;
                videoImporter.SetTargetSettings("Web", settings);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"WebGLVideoImportFix: Could not set WebGL video import on {videoImporter.assetPath}: {ex.Message}");
            }
        }
    }

    [MenuItem("Tools/First Aid/Fix WebGL Video Import Settings")]
    private static void ReimportStoryVideos()
    {
        string[] guids = AssetDatabase.FindAssets("t:VideoClip", new[] { "Assets/Video" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as VideoClipImporter;
            if (importer == null) continue;

            ApplyWebGLSettings(importer);
            importer.SaveAndReimport();
        }
        Debug.Log($"Reimported {guids.Length} video clip(s) with WebGL VP8 settings.");
    }
}
#endif
