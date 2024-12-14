using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class StartImageSlideshow : MonoBehaviour, IImageSlideshow
{
    public Image displayImage;
    public Sprite[] images;
    public AudioClip clickSound;
    public string nextSceneName;
    private int currentImageIndex = 0;
    private AudioSource audioSource;

    void start()
    {
        if (SceneManager.GetActiveScene().name != "Start")  // "Start"를 해당 씬 이름으로 변경
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Start")  // "Start"를 해당 씬 이름으로 변경
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("Starting StartImageSlideshow");
        InitializeSlideshow();
    }

    void InitializeSlideshow()
    {
        if (images.Length > 0)
        {
            displayImage.sprite = images[0];
        }
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            NextImage();
        }
    }

    void NextImage()
    {
        currentImageIndex++;
        if (currentImageIndex >= images.Length)
        {
            LoadNextScene();
        }
        else
        {
            displayImage.sprite = images[currentImageIndex];
            PlayClickSound();
        }
    }

    void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading next scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not set!");
        }
    }
}
