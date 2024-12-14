using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class VideoSceneManager : MonoBehaviour
{
    public static VideoSceneManager Instance { get; private set; }

    public VideoPlayer videoPlayer;
    public string nextSceneName;
    public RenderTexture videoRenderTexture;
    public RawImage displayImage;
    public float delayAfterVideo = 2f;
    public string videoFileName;

    private Canvas videoCanvas;
    private bool shouldPlayVideo = false;

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
            return;
        }

        videoCanvas = GetComponentInChildren<Canvas>();
        if (videoCanvas == null)
        {
            Debug.LogError("Canvas component not found!");
        }

        if (displayImage != null)
        {
            displayImage.enabled = false; // 게임 시작 시 RawImage 비활성화
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void PlayVideoAndLoadScene(string nextSceneName)
    {
        this.nextSceneName = nextSceneName;
        shouldPlayVideo = true;
        if (Application.CanStreamedLevelBeLoaded("VideoScene"))
        {
            SceneManager.LoadScene("VideoScene");
        }
        else
        {
            Debug.LogError("VideoScene is not in the build settings. Please add it to the build settings.");
        }
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer component not found!");
                return;
            }
        }

        // 런타임에 RenderTexture 생성
        if (videoRenderTexture == null)
        {
            videoRenderTexture = new RenderTexture(1920, 1080, 24);
            Debug.Log("Created new RenderTexture");
        }

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;

        if (displayImage != null)
        {
            displayImage.texture = videoRenderTexture;
            Debug.Log("RawImage texture set: " + (displayImage.texture != null));
        }
        else
        {
            Debug.LogError("DisplayImage is null!");
        }

        videoPlayer.source = VideoSource.Url;
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        videoPlayer.prepareCompleted += PrepareCompleted;
        videoPlayer.loopPointReached += EndReached;

        videoPlayer.Prepare();

        Debug.Log("Video path: " + videoPath);
        Debug.Log("Video player is prepared: " + videoPlayer.isPrepared);
    }

    IEnumerator PlayVideoAndLoadSceneCoroutine()
    {
        EnableVideoPlayer();

        // RawImage 상태 확인
        if (displayImage != null)
        {
            Debug.Log("RawImage enabled: " + displayImage.enabled);
            Debug.Log("RawImage texture set: " + (displayImage.texture != null));
        }
        else
        {
            Debug.LogError("DisplayImage is null!");
        }

        videoPlayer.Play();

        Debug.Log("Video started playing");

        while (videoPlayer.isPlaying)
        {
            Debug.Log("Video time: " + videoPlayer.time);
            yield return new WaitForSeconds(1f); // 1초마다 로그 출력
        }

        Debug.Log("Video finished playing");

        yield return new WaitForSeconds(delayAfterVideo);
        DisableVideoPlayer();
        SceneManager.LoadScene(nextSceneName);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        Debug.Log("Video preparation completed. Duration: " + vp.length);
        vp.Play();
    }

    void EndReached(VideoPlayer vp)
    {
        StartCoroutine(LoadNextSceneWithDelay());
    }

    IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(delayAfterVideo);
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "VideoScene" && shouldPlayVideo)
        {
            shouldPlayVideo = false;
            StartCoroutine(PlayVideoAndLoadSceneCoroutine());
        }
        else
        {
            DisableVideoPlayer();
        }
    }

    void DisableVideoPlayer()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        if (videoCanvas != null)
        {
            videoCanvas.enabled = false;
        }
        if (displayImage != null)
        {
            displayImage.enabled = false;
        }
    }

    void EnableVideoPlayer()
    {
        if (videoCanvas != null)
        {
            videoCanvas.enabled = true;
        }
        if (displayImage != null)
        {
            displayImage.enabled = true;
            displayImage.texture = videoRenderTexture;  // 여기서 다시 설정
            Debug.Log("RawImage enabled and texture set: " + (displayImage.texture != null));
        }
        SetupVideoPlayer();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}