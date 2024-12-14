using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace StartScene
{
    public class ImageSlideshow : MonoBehaviour
    {
        public Image displayImage;
        public Sprite[] images;
        public AudioClip clickSound;
        public string nextSceneName;
        private int currentImageIndex = 0;
        private AudioSource audioSource;

        void Start()
        {
            Debug.Log($"Starting ImageSlideshow in scene: {SceneManager.GetActiveScene().name}");
            InitializeSlideshow();
        }

        void InitializeSlideshow()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (displayImage == null)
            {
                displayImage = GetComponent<Image>();
            }

            if (displayImage != null && images.Length > 0)
            {
                currentImageIndex = 0;
                displayImage.sprite = images[currentImageIndex];
                Debug.Log($"Initialized slideshow with {images.Length} images. First image: {images[0].name}");
            }
            else
            {
                Debug.LogError("Display Image or Images array is not set properly!");
            }
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
            if (audioSource != null && clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            currentImageIndex++;
            if (currentImageIndex < images.Length)
            {
                if (displayImage != null)
                {
                    displayImage.sprite = images[currentImageIndex];
                    Debug.Log($"Showing image: {images[currentImageIndex].name}");
                }
            }
            else
            {
                Debug.Log($"Loading next scene: {nextSceneName}");
                LoadNextScene();
            }
        }

        void LoadNextScene()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("Next scene name is not set!");
            }
        }
    }
}