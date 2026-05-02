using UnityEngine;
using System;

public class Puzzle : MonoBehaviour, ILevelEntity, IInteractable
{
    [SerializeField] private string guid;

    [Header("Requirements")]
    [SerializeField] private LevelFlag[] requiredFlags; 
    [SerializeField] private LevelFlag flagToUnlock;    

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    public string Guid => guid;

    private bool isSolved;

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
        if (isSolved) return;
        if (icon != null) icon.SetActive(true);
    }

    public void HideInteractableIcon()
    {
        if (icon != null) icon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (isSolved) return;
        if (prompt != null) prompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    public void Interact()
    {
        if (isSolved) return;
        if (!HasAllRequiredFlags()) return;

        Solve();
    }

    private bool HasAllRequiredFlags()
    {
        if (requiredFlags == null || requiredFlags.Length == 0) return true;

        foreach (var flag in requiredFlags)
        {
            if (!LevelManager.Instance.GetFlag(flag))
                return false;
        }

        return true;
    }

    private void Solve()
    {
        isSolved = true;

        HideInteractableIcon();
        HideInteractionPrompt();

        LevelManager.Instance.MarkEntityOpened(guid);

        if (flagToUnlock != LevelFlag.None)
            LevelManager.Instance.TriggerFlag(flagToUnlock);

        // TODO: solved visuals
    }

    public void LoadState()
    {
        if (LevelManager.Instance.IsEntityOpened(guid))
        {
            isSolved = true;
            HideInteractableIcon();
            HideInteractionPrompt();
            // TODO: solved visuals
        }
    }
}