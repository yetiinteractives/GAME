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
    private readonly Dictionary<string, bool> entityRemoved = new();
    private readonly Dictionary<string, bool> entityOpened = new();

    // NEW: puzzle inserted state
    private readonly Dictionary<string, bool[]> puzzleInserted = new();

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

    // ---- ENTITY PERSISTENCE ----
    public void MarkEntityRemoved(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return;
        entityRemoved[guid] = true;
    }

    public bool IsEntityRemoved(string guid)
    {
        return !string.IsNullOrEmpty(guid) &&
               entityRemoved.TryGetValue(guid, out var removed) && removed;
    }

    public void MarkEntityOpened(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return;
        entityOpened[guid] = true;
    }

    public bool IsEntityOpened(string guid)
    {
        return !string.IsNullOrEmpty(guid) &&
               entityOpened.TryGetValue(guid, out var opened) && opened;
    }

    // ---- PUZZLE INSERTED ----
    public void SetPuzzleInserted(string guid, bool[] inserted)
    {
        if (string.IsNullOrEmpty(guid) || inserted == null) return;
        puzzleInserted[guid] = (bool[])inserted.Clone();
    }

    public bool[] GetPuzzleInserted(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        return puzzleInserted.TryGetValue(guid, out var v) ? v : null;
    }

    // ---- SAVE/LOAD ----
    public LevelStateData ExportState()
    {
        return new LevelStateData(flagState, triggerState, entityRemoved, entityOpened, puzzleInserted);
    }

    public void ImportState(LevelStateData data)
    {
        flagState.Clear();
        triggerState.Clear();
        entityRemoved.Clear();
        entityOpened.Clear();
        puzzleInserted.Clear();

        data.ApplyTo(flagState, triggerState, entityRemoved, entityOpened, puzzleInserted);

        foreach (var f in flagState.Keys) OnFlagChanged?.Invoke(f);
        foreach (var t in triggerState.Keys) OnTriggerFired?.Invoke(t);
    }
}