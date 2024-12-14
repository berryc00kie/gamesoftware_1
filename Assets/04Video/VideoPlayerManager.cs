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

        // 비디오 파일 경로 
        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);

        if (File.Exists(videoPath))
        {
            // 비디오 플레이어 설정
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.url = videoPath;
            videoPlayer.isLooping = false;

            // 비디오 재생 완료 이벤트 리스너
            videoPlayer.loopPointReached += OnVideoFinished;

            // 프레임 준비 이벤트에 메서드 연결
            videoPlayer.prepareCompleted += PrepareCompleted;

            // 비디오 준비 시작
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

        // 비디오 재생 시작
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