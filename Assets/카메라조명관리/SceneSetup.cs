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
        // 모든 MainCamera 태그를 가진 카메라를 찾아서 현재 씬의 것만 활성화
        Camera[] cameras = GameObject.FindGameObjectsWithTag("MainCamera")
                                     .Select(go => go.GetComponent<Camera>())
                                     .Where(c => c != null)
                                     .ToArray();
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(cam.gameObject.scene == SceneManager.GetActiveScene());
        }

        // 모든 MainLight 태그를 가진 라이트를 찾아서 현재 씬의 것만 활성화
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