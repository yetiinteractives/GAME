using UnityEngine;
using System;

public class Puzzle : MonoBehaviour, ILevelEntity
{
    [SerializeField] private string guid;
    [SerializeField] private LevelFlag flagToUnlock;

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

    public void Solve()
    {
        LevelManager.Instance.TriggerFlag(flagToUnlock);
    }

    public void LoadState()
    {
        if (LevelManager.Instance.GetFlag(flagToUnlock))
        {
            // already solved visuals
        }
    }
}