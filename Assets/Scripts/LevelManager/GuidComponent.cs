using UnityEngine;
using System;

public class GuidComponent : MonoBehaviour
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
}