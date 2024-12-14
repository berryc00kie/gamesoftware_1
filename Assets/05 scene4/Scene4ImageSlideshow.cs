using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scene4ImageSlideshow : MonoBehaviour, IImageSlideshow
{
    public Image displayImage;
    public Sprite[] images;
    public AudioClip clickSound;
    public string nextSceneName;
    private int currentImageIndex = 0;
    private AudioSource audioSource;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != "4")  
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
        if (scene.name != "4")  
        {
            Destroy(gameObject);
        }
    }
    void InitializeSlideshow()
    {
        if (displayImage == null)
        {
            displayImage = GetComponent<Image>();
        }

        if (displayImage == null)
        {
            Debug.LogError("Display Image is not set and couldn't be found!");
            return;
        }

        if (images == null || images.Length == 0)
        {
            Debug.LogError("No images are set in the inspector!");
            return;
        }

        displayImage.sprite = images[0];
        audioSource = gameObject.AddComponent<AudioSource>();

        // 디버그 로그 
        Debug.Log($"Initialized Scene4ImageSlideshow with {images.Length} images.");
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
        Debug.Log($"Moving to image index: {currentImageIndex}"); // 디버그 로그 

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
        if (clickSound != null && audioSource != null)
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