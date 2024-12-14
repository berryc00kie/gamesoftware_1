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
            if (selectedItem == null)
            {
                SelectItem(obj);
                heldKey = key;
            }
            else if (selectedItem == obj)
            {
                ReturnItemToOriginalState();
                heldKey = null;
            }
        }
        else if (lockable != null && heldKey != null)
        {
            TryUnlock(lockable);
        }
    }

    void TryUnlock(Lockable lockable)
    {
        if (heldKey == null) return;
        if (heldKey.keyId == lockable.requiredKeyId)
        {
            lockable.Unlock();
            // 열쇠로 문을 열어도 들고 있는 아이템을 유지
        }
    }

    void SelectItem(GameObject item)
    {
        if (selectedItem != null)
        {
            ReturnItemToOriginalState();
        }

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