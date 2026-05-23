using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoSave : MonoBehaviour
{
    public static AutoSave Instance { get; private set; }

    [SerializeField] private Image autoSaveIcon;
    [SerializeField] private float fadeDuration = 5f;

    [Header("Reliability")]
    [SerializeField] private float minSecondsBetweenAutosaves = 1.0f;

    private LevelManager subscribedLevelManager;
    private Coroutine bindRoutine;

    private float lastSaveTime = -999f;
    private bool saveInProgress;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

    }
    private void OnEnable()
    {
        if (autoSaveIcon != null)
            autoSaveIcon.gameObject.SetActive(false);

        bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }

        Unbind();
    }

    private IEnumerator BindWhenReady()
    {
        // Wait until LevelManager exists
        while (LevelManager.Instance == null)
            yield return null;

        // Bind once
        subscribedLevelManager = LevelManager.Instance;
        subscribedLevelManager.OnTriggerFired += OnTriggerFired;
        subscribedLevelManager.OnFlagChanged += OnFlagChanged;
    }

    private void Unbind()
    {
        if (subscribedLevelManager == null) return;

        subscribedLevelManager.OnTriggerFired -= OnTriggerFired;
        subscribedLevelManager.OnFlagChanged -= OnFlagChanged;
        subscribedLevelManager = null;
    }

    private void OnTriggerFired(TriggerID triggerID) => TrySave();
    private void OnFlagChanged(LevelFlag levelFlag) => TrySave();

    public void TrySave()
    {
        if (SaveManager.Instance == null) return;
        if (saveInProgress) return;

        if (Time.unscaledTime - lastSaveTime < minSecondsBetweenAutosaves)
            return;

        StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        saveInProgress = true;

        
        yield return new WaitForSeconds(1f);

        SaveManager.Instance.SaveGame();
        lastSaveTime = Time.unscaledTime;

        if (autoSaveIcon != null)
            StartCoroutine(AutoSaveIconAnimation());

        Debug.Log("Game auto-saved.");
        saveInProgress = false;
    }

    private IEnumerator AutoSaveIconAnimation()
    {
        autoSaveIcon.gameObject.SetActive(true);
        yield return new WaitForSeconds(fadeDuration);
        autoSaveIcon.gameObject.SetActive(false);
    }
}