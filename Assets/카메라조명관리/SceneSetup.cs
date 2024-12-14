using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneSetup : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupCamerasAndLights();
    }

    void SetupCamerasAndLights()
    {
        // ��� MainCamera �±׸� ���� ī�޶� ã�Ƽ� ���� ���� �͸� Ȱ��ȭ
        Camera[] cameras = GameObject.FindGameObjectsWithTag("MainCamera")
                                     .Select(go => go.GetComponent<Camera>())
                                     .Where(c => c != null)
                                     .ToArray();
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(cam.gameObject.scene == SceneManager.GetActiveScene());
        }

        // ��� MainLight �±׸� ���� ����Ʈ�� ã�Ƽ� ���� ���� �͸� Ȱ��ȭ
        Light[] lights = GameObject.FindGameObjectsWithTag("MainLight")
                                   .Select(go => go.GetComponent<Light>())
                                   .Where(l => l != null)
                                   .ToArray();
        foreach (Light light in lights)
        {
            light.gameObject.SetActive(light.gameObject.scene == SceneManager.GetActiveScene());
        }
    }
}