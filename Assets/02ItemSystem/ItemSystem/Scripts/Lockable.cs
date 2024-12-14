using UnityEngine;
using UnityEngine.SceneManagement;

public class Lockable : MonoBehaviour
{
    public string requiredKeyId;
    public GameObject unlockedObject;
    public string nextSceneName;
    public float delayBeforeNextScene = 2f;

    [Header("Audio")]
    public AudioClip unlockSound;

    private bool isUnlocked = false;
    private AudioSource audioSource;

    private void Awake()
    {
        // AudioSource 컴포넌트 추가 (없는 경우에만)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Unlock()
    {
        if (isUnlocked) return;

        if (unlockedObject != null)
        {
            // 잠금 해제 소리 재생
            // 잠금 해제 사운드 재생
            if (unlockSound != null)
            {
                AudioSource.PlayClipAtPoint(unlockSound, transform.position);
            }

            gameObject.SetActive(false);
            unlockedObject.SetActive(true);
            isUnlocked = true;
            Invoke("LoadNextScene", delayBeforeNextScene);
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}