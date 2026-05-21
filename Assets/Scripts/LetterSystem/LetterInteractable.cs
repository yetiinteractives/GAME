using UnityEngine;

public class LetterInteractable : MonoBehaviour, ILevelEntity, IInteractable
{
    [SerializeField] private string guid;

    [Header("Letter")]
    [SerializeField] private LetterData letterData;
    [SerializeField] private Transform cameraFocusPoint;

    [Header("Persistence")]
    [SerializeField] private bool destroyAfterRead = false;

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    public string Guid => guid;

#if UNITY_EDITOR
    [ContextMenu("Generate GUID")]
    private void GenerateGuid()
    {
        guid = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void Awake()
    {
        FindFirstObjectByType<LevelRegistry>().Register(this);
    }

    private void Start()
    {
        HideInteractableIcon();
        HideInteractionPrompt();
    }

    public void ShowInteractableIcon()
    {
        if (icon != null) icon.SetActive(true);
    }

    public void HideInteractableIcon()
    {
        if (icon != null) icon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (prompt != null) prompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    public void Interact()
    {
        Transform focus = cameraFocusPoint != null ? cameraFocusPoint : transform;
        LetterManager.Instance.OpenLetter(letterData, focus);

        // Persist 
        if (LevelManager.Instance != null)
            LevelManager.Instance.MarkEntityOpened(guid);

        
        if (destroyAfterRead)
        {
            HideInteractableIcon();
            HideInteractionPrompt();
            Destroy(gameObject);
        }
        
    }

    public void LoadState()
    {
        if (LevelManager.Instance == null) return;

        bool wasOpened = LevelManager.Instance.IsEntityOpened(guid);

        
        if (destroyAfterRead && wasOpened)
            Destroy(gameObject);
    }
}