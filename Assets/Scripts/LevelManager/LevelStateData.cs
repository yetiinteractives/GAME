using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class LevelStateData
{
    public List<FlagEntry> flags = new();
    public List<TriggerEntry> triggers = new();

    public LevelStateData() { }

    public LevelStateData(Dictionary<LevelFlag, bool> f, Dictionary<TriggerID, bool> t)
    {
        flags = f.Select(kv => new FlagEntry(kv.Key, kv.Value)).ToList();
        triggers = t.Select(kv => new TriggerEntry(kv.Key, kv.Value)).ToList();
    }

    public void ApplyTo(Dictionary<LevelFlag, bool> f, Dictionary<TriggerID, bool> t)
    {
        foreach (var entry in flags) f[entry.flag] = entry.value;
        foreach (var entry in triggers) t[entry.trigger] = entry.value;
    }
}

[Serializable]
public struct FlagEntry
{
    public LevelFlag flag;
    public bool value;
    public FlagEntry(LevelFlag f, bool v) { flag = f; value = v; }
}

[Serializable]
public struct TriggerEntry
{
    public TriggerID trigger;
    public bool value;
    public TriggerEntry(TriggerID t, bool v) { trigger = t; value = v; }
}