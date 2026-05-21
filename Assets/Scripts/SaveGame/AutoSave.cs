using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoSave : MonoBehaviour
{
    [SerializeField] private Image autoSaveIcon;
    [SerializeField] private float fadeDuration = 5f;

    private void Start()
    {
        autoSaveIcon.gameObject.SetActive(false);
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
        if(SaveManager.Instance != null)
        SaveManager.Instance.SaveGame();
        if (autoSaveIcon != null)
            StartCoroutine(AutoSaveIconAnimation());
        Debug.Log("Game auto-saved.");

    }
    IEnumerator AutoSaveIconAnimation()
    {
        autoSaveIcon.gameObject.SetActive(true);
        yield return new WaitForSeconds(fadeDuration);
        autoSaveIcon.gameObject.SetActive(false);
    }
}
