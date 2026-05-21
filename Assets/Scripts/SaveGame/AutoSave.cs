using UnityEngine;
using UnityEngine.UI;

public class AutoSave : MonoBehaviour
{
    [SerializeField] private Image autoSaveIcon;
    [SerializeField] private float fadeDuration = 2f;

    private void OnEnable()
    {
        LevelManager.Instance.OnTriggerFired += OnTriggerFired;
        LevelManager.Instance.OnFlagChanged += OnFlagChanged;
    }

    private void OnDisable()
    {
        LevelManager.Instance.OnTriggerFired -= OnTriggerFired;
        LevelManager.Instance.OnFlagChanged -= OnFlagChanged;
    }

    private void OnTriggerFired(TriggerID triggerID)
    {
        Save();
    }
    private void OnFlagChanged(LevelFlag levelFlag)
    {
        Save();
    }

    private void Save()
    {
        SaveManager.Instance.SaveGame();
    }
}
