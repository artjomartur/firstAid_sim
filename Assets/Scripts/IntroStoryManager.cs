using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class IntroStoryManager : MonoBehaviour
{
    public GameManager gameManager;
    public Image storyImage;
    public Text storyText;
    [Header("Video Settings")]
    public VideoClip[] storyVideoClips;
    public GameObject storyTextBox;
    public GameObject videoSkipButton;

    private const float PrepareTimeoutSeconds = 20f;

    private VideoPlayer videoPlayer;
    private RawImage videoRawImage;
    private RenderTexture videoRenderTexture;
    private GameObject videoObj;
    private bool videoFailed;

    private void OnEnable()
    {
        StartCoroutine(RunStorySequence());
    }

    private IEnumerator RunStorySequence()
    {
        if (storyVideoClips == null || storyVideoClips.Length == 0)
        {
            CleanupVideo();
            gameManager.StartCallPhase();
            yield break;
        }

        if (storyTextBox != null) storyTextBox.SetActive(false);
        if (videoSkipButton != null) videoSkipButton.SetActive(true);

        if (!SetupVideoPlayer())
        {
            Debug.LogWarning("IntroStoryManager: Video setup failed — skipping to call phase.");
            CleanupVideo();
            gameManager.StartCallPhase();
            yield break;
        }

        if (storyImage != null) storyImage.gameObject.SetActive(false);

        for (int i = 0; i < storyVideoClips.Length; i++)
        {
            VideoClip clip = storyVideoClips[i];
            if (clip == null) continue;

            videoFailed = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, clip.name + ".mp4");
#else
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = clip;
#endif
            videoPlayer.Prepare();

            float elapsed = 0f;
            while (!videoPlayer.isPrepared && !videoFailed && elapsed < PrepareTimeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!videoPlayer.isPrepared)
            {
                Debug.LogWarning($"IntroStoryManager: Video '{clip.name}' did not prepare in time (WebGL/browser codec?). Skipping.");
                continue;
            }

            videoPlayer.Play();

            while (videoPlayer.isPlaying && !videoFailed)
            {
                yield return null;
            }
        }

        CleanupVideo();
        gameManager.StartCallPhase();
    }

    private bool SetupVideoPlayer()
    {
        videoObj = new GameObject("StoryVideoPlayer");
        videoObj.transform.SetParent(transform, false);
        videoObj.transform.SetAsFirstSibling();

        RectTransform rt = videoObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        videoRawImage = videoObj.AddComponent<RawImage>();
        videoRawImage.color = Color.white;

        videoRenderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        videoRenderTexture.Create();
        videoRawImage.texture = videoRenderTexture;

        videoPlayer = videoObj.AddComponent<VideoPlayer>();
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Browser playback is more reliable with direct audio on WebGL.
        videoPlayer.SetDirectAudioVolume(0, 1f);
#endif

        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        return true;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (videoRawImage != null)
            videoRawImage.color = Color.white;
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("IntroStoryManager video error: " + message);
        videoFailed = true;
    }

    private void CleanupVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.Stop();
        }

        if (videoSkipButton != null)
            videoSkipButton.SetActive(false);

        if (videoObj != null)
        {
            Destroy(videoObj);
            videoObj = null;
        }

        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
            videoRenderTexture = null;
        }

        if (storyImage != null)
            storyImage.gameObject.SetActive(true);
    }

    public void SkipStory()
    {
        StopAllCoroutines();
        CleanupVideo();
        gameManager.StartCallPhase();
    }

    private void OnDisable()
    {
        CleanupVideo();
    }
}
