using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class VideoPlayerManager : MonoBehaviour
{
    public string videoFileName = "your_video.mp4";
    public RawImage videoDisplay;

    private VideoPlayer videoPlayer;
    private VideoSceneLoader sceneLoader;

    void Start()
    {
        sceneLoader = FindObjectOfType<VideoSceneLoader>();

        
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // ���� ���� ��� 
        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);

        if (File.Exists(videoPath))
        {
            // ���� �÷��̾� ����
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.url = videoPath;
            videoPlayer.isLooping = false;

            // ���� ��� �Ϸ� �̺�Ʈ ������
            videoPlayer.loopPointReached += OnVideoFinished;

            // ������ �غ� �̺�Ʈ�� �޼��� ����
            videoPlayer.prepareCompleted += PrepareCompleted;

            // ���� �غ� ����
            videoPlayer.Prepare();
        }
        else
        {
            Debug.LogError("Video file not found: " + videoPath);
            OnVideoFinished(null);
        }
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        
        if (videoDisplay != null)
        {
            videoDisplay.texture = videoPlayer.texture;
        }

        // ���� ��� ����
        videoPlayer.Play();
        Debug.Log("Video playback started");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video playback finished");
        if (sceneLoader != null)
        {
            sceneLoader.OnVideoFinished();
        }
    }
}