using UnityEngine;
using TMPro;
using System.Collections;

public class Door : MonoBehaviour, ILevelEntity, IInteractable
{
    [SerializeField] private string guid;

    [Header("Flags")]
    [SerializeField] private LevelFlag requiredFlag;
    [SerializeField] private LevelFlag openFlag;

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    [Header("Locked Message")]
    [SerializeField] private string lockedMessage = "Needs Rusty Key";
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private float messageDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;

    public string Guid => guid;

    private bool isOpen;

    private TMP_Text messageText;
    private Coroutine messageRoutine;

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

        if (messagePanel != null)
        {
            messageText = messagePanel.GetComponentInChildren<TMP_Text>();
            messagePanel.SetActive(false);
        }

        if (LevelManager.Instance != null)
            LevelManager.Instance.OnFlagChanged += OnFlagChanged;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnFlagChanged -= OnFlagChanged;
    }

    public void ShowInteractableIcon()
    {
        if (isOpen) return;

        if (icon != null)
            icon.SetActive(true);
    }

    public void HideInteractableIcon()
    {
        if (icon != null)
            icon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (isOpen) return;

        if (prompt != null)
            prompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null)
            prompt.SetActive(false);
    }

    public void Interact()
    {
        if (isOpen) return;

        if (CanOpen())
            Open();
        else
            ShowLockedMessage();
    }

    private bool CanOpen()
    {
        return requiredFlag == LevelFlag.None ||
               LevelManager.Instance.GetFlag(requiredFlag);
    }

    private void OnFlagChanged(LevelFlag flag)
    {
        if (isOpen) return;

        if (openFlag != LevelFlag.None && flag == openFlag)
            Open();
    }

    private void Open()
    {
        if (isOpen) return;

        isOpen = true;

        HideInteractableIcon();
        HideInteractionPrompt();

        LevelManager.Instance.MarkEntityOpened(guid);

        Animator anim = GetComponentInChildren<Animator>();

        if (anim != null)
            anim.SetTrigger("OpenDoor");
        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        AutoSave.Instance.TrySave();
    }

    public void LoadState()
    {
        if (LevelManager.Instance.IsEntityOpened(guid))
            Open();
    }

    private void ShowLockedMessage()
    {
        if (messagePanel == null || messageText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine());
        
        if(lockedSound != null)
        {
            AudioSource.PlayClipAtPoint(lockedSound, transform.position);
        }
    }

    private IEnumerator ShowMessageRoutine()
    {
        messageText.text = lockedMessage;

        messagePanel.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messagePanel.SetActive(false);
    }
}