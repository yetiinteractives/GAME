using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyPickUpMessage : MonoBehaviour
{
    [Serializable]
    public class KeyMessageEntry
    {
        public LevelFlag flag;
        public Sprite sprite;
        public string message;
    }

    [Header("Data (set from Inspector)")]
    [SerializeField] private List<KeyMessageEntry> entries = new List<KeyMessageEntry>();

    [Header("UI References")]
    [SerializeField] private GameObject messageRoot;   // the panel/root to show/hide
    [SerializeField] private Image iconImage;          // where the sprite goes
    [SerializeField] private TMP_Text messageText;     // TMP text for message string

    [Header("Timing")]
    [SerializeField] private float showSeconds = 2.5f;

    private LevelManager subscribedLevelManager;
    private Coroutine bindRoutine;
    private Coroutine showRoutine;

    private void OnEnable()
    {
        HideImmediate();
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

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        HideImmediate();
    }

    private IEnumerator BindWhenReady()
    {
        while (LevelManager.Instance == null)
            yield return null;

        subscribedLevelManager = LevelManager.Instance;
        subscribedLevelManager.OnFlagChanged += OnFlagChanged;
    }

    private void Unbind()
    {
        if (subscribedLevelManager == null) return;
        subscribedLevelManager.OnFlagChanged -= OnFlagChanged;
        subscribedLevelManager = null;
    }

    private void OnFlagChanged(LevelFlag flag)
    {
        // Find matching entry
        var entry = entries.Find(e => e.flag.Equals(flag));
        if (entry == null) return;

        Show(entry);
    }

    private void Show(KeyMessageEntry entry)
    {
        // Update UI
        if (iconImage != null) iconImage.sprite = entry.sprite;
        if (messageText != null) messageText.text = entry.message;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (messageRoot != null)
            messageRoot.SetActive(true);

        yield return new WaitForSecondsRealtime(showSeconds);

        HideImmediate();
        showRoutine = null;
    }

    private void HideImmediate()
    {
        if (messageRoot != null)
            messageRoot.SetActive(false);
    }
}