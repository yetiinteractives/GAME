using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Identity")]
    public string levelID = "level_01";

    private readonly Dictionary<LevelFlag, bool> flagState = new();
    private readonly Dictionary<TriggerID, bool> triggerState = new();

    public event Action<LevelFlag> OnFlagChanged;
    public event Action<TriggerID> OnTriggerFired;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ---- FLAGS ----
    public void SetFlag(LevelFlag flag, bool value)
    {
        flagState[flag] = value;
        OnFlagChanged?.Invoke(flag);
    }

    public bool GetFlag(LevelFlag flag)
    {
        return flagState.TryGetValue(flag, out var v) && v;
    }

    public void TriggerFlag(LevelFlag flag) => SetFlag(flag, true);

    // ---- TRIGGERS ----
    public void FireTrigger(TriggerID trigger)
    {
        triggerState[trigger] = true;
        OnTriggerFired?.Invoke(trigger);
    }

    public bool HasTriggered(TriggerID trigger)
    {
        return triggerState.TryGetValue(trigger, out var v) && v;
    }

    // ---- SAVE/LOAD ----
    public LevelStateData ExportState()
    {
        return new LevelStateData(flagState, triggerState);
    }

    public void ImportState(LevelStateData data)
    {
        flagState.Clear();
        triggerState.Clear();

        data.ApplyTo(flagState, triggerState);

        // re-broadcast so listeners can update visuals
        foreach (var f in flagState.Keys) OnFlagChanged?.Invoke(f);
        foreach (var t in triggerState.Keys) OnTriggerFired?.Invoke(t);
    }
}