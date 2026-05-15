using UnityEngine;
using TMPro;
using System.Collections;

public class GeneratorPuzzle : MonoBehaviour, ILevelEntity, IInteractable
{
    [SerializeField] private string guid;

    [Header("Flags")]
    [SerializeField] private LevelFlag[] requiredFlags;
    [SerializeField] private LevelFlag outputFlag = LevelFlag.IsGeneratorOn;

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    [Header("Message")]
    [SerializeField] private string missingRequirementsMessage = "Needs Fuel";
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private float messageDuration = 2f;

    public string Guid => guid;

    private bool isOn;
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
        if (isOn) return;
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
        if (isOn) return;
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
        if (isOn) return;

        if (AllConditionsMet())
            StartGenerator();
        else
            ShowMissingRequirementsMessage();
    }

    private bool AllConditionsMet()
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
            return true;

        foreach (var flag in requiredFlags)
        {
            if (flag == LevelFlag.None)
                continue;
            if (!LevelManager.Instance.GetFlag(flag))
                return false;
        }
        return true;
    }

    private void OnFlagChanged(LevelFlag changedFlag)
    {
      
    }

    private void StartGenerator()
    {
        if (isOn) return;
        isOn = true;

        HideInteractableIcon();
        HideInteractionPrompt();

        LevelManager.Instance.TriggerFlag(outputFlag); 
        LevelManager.Instance.MarkEntityOpened(guid);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("StartGenerator");
    }

    public void LoadState()
    {
        if (LevelManager.Instance.IsEntityOpened(guid))
            StartGenerator();
    }

    private void ShowMissingRequirementsMessage()
    {
        if (messagePanel == null || messageText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine());
    }

    private IEnumerator ShowMessageRoutine()
    {
        messageText.text = missingRequirementsMessage;

        messagePanel.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messagePanel.SetActive(false);
    }
}