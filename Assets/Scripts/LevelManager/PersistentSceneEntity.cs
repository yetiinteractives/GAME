using UnityEngine;
using System;

public class PersistentSceneEntity : MonoBehaviour, ILevelEntity
{
    [SerializeField] private string guid;
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
        FindFirstObjectByType<LevelRegistry>()?.Register(this);
    }

    private void Start()
    {
        CheckAndDestroyIfRemoved();
    }

    public void LoadState()
    {
        CheckAndDestroyIfRemoved();
    }

    private void CheckAndDestroyIfRemoved()
    {
        if (string.IsNullOrWhiteSpace(guid)) return;

        if (LevelManager.Instance != null && LevelManager.Instance.IsEntityRemoved(guid))
        {
            Destroy(gameObject);
        }
    }

   
    public void MarkRemoved(bool destroy = true)
    {
        if (string.IsNullOrWhiteSpace(guid))
        {
            if (destroy) Destroy(gameObject);
            return;
        }

        LevelManager.Instance?.MarkEntityRemoved(guid);
        if (destroy) Destroy(gameObject);
    }
}