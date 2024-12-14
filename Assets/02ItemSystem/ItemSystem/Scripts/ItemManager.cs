using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public Camera mainCamera;
    public Vector3 holdPositionOffset = new Vector3(0.4f, -0.3f, 1f);
    public float alphaWhenSelected = 0.5f;
    public Material transparentMaterial;

    private GameObject selectedItem;
    private GameObject copiedItem;
    private Material originalMaterial;
    private Color originalColor;
    private Key heldKey = null;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found!");
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                InteractWithObject(hit.collider.gameObject);
            }
        }

        if (copiedItem != null)
        {
            copiedItem.transform.position = mainCamera.transform.position +
                                            mainCamera.transform.right * holdPositionOffset.x +
                                            mainCamera.transform.up * holdPositionOffset.y +
                                            mainCamera.transform.forward * holdPositionOffset.z;
            copiedItem.transform.rotation = mainCamera.transform.rotation;
        }
    }

    void InteractWithObject(GameObject obj)
    {
        Key key = obj.GetComponent<Key>();
        Lockable lockable = obj.GetComponent<Lockable>();

        if (key != null)
        {
            ToggleItemSelection(obj);
            heldKey = selectedItem != null ? key : null;
        }
        else if (lockable != null && heldKey != null)
        {
            TryUnlock(lockable);
        }
        else if (obj.CompareTag("Item"))
        {
            ToggleItemSelection(obj);
        }
    }

    void TryUnlock(Lockable lockable)
    {
        if (heldKey == null) return;

        if (heldKey.keyId == lockable.requiredKeyId)
        {
            lockable.Unlock();
            ReturnItemToOriginalState();
            heldKey = null;
        }
    }
    void ToggleItemSelection(GameObject item)
    {
        if (selectedItem == item)
        {
            ReturnItemToOriginalState();
        }
        else
        {
            if (selectedItem != null)
            {
                ReturnItemToOriginalState();
            }
            SelectItem(item);
        }
    }

    void SelectItem(GameObject item)
    {
        selectedItem = item;
        Renderer itemRenderer = selectedItem.GetComponent<Renderer>();
        originalMaterial = itemRenderer.sharedMaterial;
        originalColor = originalMaterial.color;

        if (transparentMaterial != null)
        {
            itemRenderer.material = transparentMaterial;
            transparentMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaWhenSelected);
        }
        else
        {
            Debug.LogWarning("Transparent material is not assigned in the inspector!");
        }

        copiedItem = Instantiate(selectedItem, mainCamera.transform.position, mainCamera.transform.rotation);
        copiedItem.transform.localScale = selectedItem.transform.localScale * 0.2f;
        copiedItem.GetComponent<Renderer>().material = originalMaterial;

        Collider copiedCollider = copiedItem.GetComponent<Collider>();
        if (copiedCollider != null)
        {
            copiedCollider.enabled = false;
        }
    }

    void ReturnItemToOriginalState()
    {
        if (selectedItem != null)
        {
            selectedItem.GetComponent<Renderer>().material = originalMaterial;
            if (copiedItem != null)
            {
                Destroy(copiedItem);
            }
            selectedItem = null;
            copiedItem = null;
        }
    }
}