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
        yield return null; // ���� �����ӱ��� ���

        Scene videoScene = SceneManager.GetSceneByName(videoSceneName);
        SceneManager.SetActiveScene(videoScene);

        // ���� �� ī�޶� ��Ȱ��ȭ
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }
    }

    // �� �޼���� ���� ����� ������ �� ȣ��˴ϴ�
    public void OnVideoFinished()
    {
        StartCoroutine(UnloadVideoScene());
        VideoFinished?.Invoke();
    }

    private System.Collections.IEnumerator UnloadVideoScene()
    {
        // ���� ������ 
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainSceneName));

        // ���� ī�޶� �ٽ� Ȱ��ȭ
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        // ���� ��ε�
        yield return SceneManager.UnloadSceneAsync(videoSceneName);
    }
}