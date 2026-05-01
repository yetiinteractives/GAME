using UnityEngine;
using System;

public class Door : MonoBehaviour, ILevelEntity
{
    [SerializeField] private string guid;
    [SerializeField] private LevelFlag requiredFlag;

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
        LevelManager.Instance.OnFlagChanged += OnFlagChanged;
    }

    public void Interact()
    {
        if (CanOpen()) Open();
        else Debug.Log("Locked");
    }

    private bool CanOpen()
    {
        return requiredFlag == LevelFlag.None || LevelManager.Instance.GetFlag(requiredFlag);
    }

    private void Open()
    {
        // animation here
    }

    private void OnFlagChanged(LevelFlag flag)
    {
        if (flag == requiredFlag) Open();
    }

    public void LoadState()
    {
        if (CanOpen()) Open();
    }
}