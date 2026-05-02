using UnityEngine;
using System;

public class Door : MonoBehaviour, ILevelEntity, IInteractable
{
    [SerializeField] private string guid;
    [SerializeField] private LevelFlag requiredFlag;

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    public string Guid => guid;

    private bool isOpen;

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
        if (isOpen) return;
        if (icon != null) icon.SetActive(true);
    }

    public void HideInteractableIcon()
    {
        if (icon != null) icon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (isOpen) return;
        if (prompt != null) prompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    public void Interact()
    {
        if (isOpen) return;

        if (CanOpen()) Open();
        else Debug.Log("Locked");
    }

    private bool CanOpen()
    {
        return requiredFlag == LevelFlag.None || LevelManager.Instance.GetFlag(requiredFlag);
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
    }

    public void LoadState()
    {
        if (LevelManager.Instance.IsEntityOpened(guid))
            Open();
    }
}