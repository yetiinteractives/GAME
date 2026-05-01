using UnityEngine;
using System;

public class PersistentSceneEntity : MonoBehaviour
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

    private void Start()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.IsEntityRemoved(guid))
        {
            Destroy(gameObject);
        }
    }

    public void MarkRemoved()
    {
        LevelManager.Instance?.MarkEntityRemoved(guid);
        Destroy(gameObject);
    }
}