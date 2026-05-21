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

    
    private bool isReadingThisLetter = false;

    private void Awake()
    {
        FindFirstObjectByType<LevelRegistry>().Register(this);
    }

    private void OnEnable()
    {
        if (LetterManager.Instance != null)
        {
            LetterManager.Instance.OnLetterOpened += OnAnyLetterOpened;
            LetterManager.Instance.OnLetterClosed += OnAnyLetterClosed;
        }
    }

    private void OnDisable()
    {
        if (LetterManager.Instance != null)
        {
            LetterManager.Instance.OnLetterOpened -= OnAnyLetterOpened;
            LetterManager.Instance.OnLetterClosed -= OnAnyLetterClosed;
        }
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
        
        isReadingThisLetter = true;

        // Hide while reading
        HideInteractableIcon();
        HideInteractionPrompt();

        Transform focus = cameraFocusPoint != null ? cameraFocusPoint : transform;
        LetterManager.Instance.OpenLetter(letterData, focus);

        // Persistence mark 
        if (LevelManager.Instance != null)
            LevelManager.Instance.MarkEntityOpened(guid);

        
    }

    private void OnAnyLetterOpened()
    {
        
        HideInteractableIcon();
        HideInteractionPrompt();
    }

    private void OnAnyLetterClosed()
    {
        
        if (!isReadingThisLetter) return;
        isReadingThisLetter = false;

        
        if (destroyAfterRead)
        {
            Destroy(gameObject);
            return;
        }

        
        ShowInteractableIcon();
        ShowInteractionPrompt();
    }

    public void LoadState()
    {
        if (LevelManager.Instance == null) return;

        bool wasOpened = LevelManager.Instance.IsEntityOpened(guid);

        if (destroyAfterRead && wasOpened)
            Destroy(gameObject);
    }
}