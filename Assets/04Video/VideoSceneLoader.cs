using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class VideoSceneLoader : MonoBehaviour
{
    public string videoSceneName = "VideoScene";
    public string mainSceneName = "MainScene";

    public event Action VideoFinished;

    public void LoadVideoScene()
    {
        SceneManager.LoadScene(videoSceneName, LoadSceneMode.Additive);
        StartCoroutine(SetVideoSceneActive());
    }

    private System.Collections.IEnumerator SetVideoSceneActive()
    {
        yield return null; // 다음 프레임까지 대기

        Scene videoScene = SceneManager.GetSceneByName(videoSceneName);
        SceneManager.SetActiveScene(videoScene);

        // 현재 씬 카메라 비활성화
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }
    }

    // 이 메서드는 비디오 재생이 끝났을 때 호출됩니다
    public void OnVideoFinished()
    {
        StartCoroutine(UnloadVideoScene());
        VideoFinished?.Invoke();
    }

    private System.Collections.IEnumerator UnloadVideoScene()
    {
        // 메인 씬으로 
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainSceneName));

        // 메인 카메라 다시 활성화
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        // 비디오 언로드
        yield return SceneManager.UnloadSceneAsync(videoSceneName);
    }
}