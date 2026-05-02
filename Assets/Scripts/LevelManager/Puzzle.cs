using UnityEngine;
using System;
using System.Collections;

public class Puzzle : MonoBehaviour, ILevelEntity, IInteractable
{
    [SerializeField] private string guid;

    [Header("Emblem Flags (match order with visuals)")]
    [SerializeField] private LevelFlag[] emblemFlags;

    [Header("Emblem Visuals (same order as flags)")]
    [SerializeField] private GameObject[] emblemObjects;

    [Header("Gate Flag")]
    [SerializeField] private LevelFlag gateOpenFlag;

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    public string Guid => guid;

    private bool isSolved;
    private bool[] insertedState;

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

        if (emblemFlags != null)
            insertedState = new bool[emblemFlags.Length];

        ApplyInsertedVisuals();
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

        if (emblemFlags == null || emblemObjects == null || emblemFlags.Length != emblemObjects.Length)
        {
            Debug.LogWarning("Puzzle: Emblem flags and visuals must match in length.");
            return;
        }

        // Insert only what player HAS (collected)
        for (int i = 0; i < emblemFlags.Length; i++)
        {
            if (LevelManager.Instance.GetFlag(emblemFlags[i]))
                insertedState[i] = true;
        }

        LevelManager.Instance.SetPuzzleInserted(guid, insertedState);
        ApplyInsertedVisuals();

        // Check if all inserted
        for (int i = 0; i < insertedState.Length; i++)
        {
            if (!insertedState[i]) return;
        }

        Solve();
    }

    private void Solve()
    {
        if (isSolved) return;
        isSolved = true;

        HideInteractableIcon();
        HideInteractionPrompt();

        LevelManager.Instance.MarkEntityOpened(guid);


        StartCoroutine(DoorOpenDelay());
    }

    IEnumerator DoorOpenDelay()
    {
        yield return new WaitForSeconds(4f);
        if (gateOpenFlag != LevelFlag.None)
            LevelManager.Instance.TriggerFlag(gateOpenFlag);
    }

    private void ApplyInsertedVisuals()
    {
        if (emblemObjects == null || insertedState == null) return;
        for (int i = 0; i < emblemObjects.Length; i++)
        {
            if (emblemObjects[i] != null)
                emblemObjects[i].SetActive(insertedState[i]);
        }
    }

    public void LoadState()
    {
        if (LevelManager.Instance.IsEntityOpened(guid))
        {
            isSolved = true;
            HideInteractableIcon();
            HideInteractionPrompt();

            if (gateOpenFlag != LevelFlag.None)
                LevelManager.Instance.TriggerFlag(gateOpenFlag);
        }

        var saved = LevelManager.Instance.GetPuzzleInserted(guid);
        if (saved != null)
            insertedState = saved;

        ApplyInsertedVisuals();
    }
}