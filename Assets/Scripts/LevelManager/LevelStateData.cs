using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class LevelStateData
{
    public List<FlagEntry> flags = new();
    public List<TriggerEntry> triggers = new();
    public List<EntityEntry> entities = new();
    public List<EntityEntry> opened = new(); // NEW

    public LevelStateData() { }

    public LevelStateData(
        Dictionary<LevelFlag, bool> f,
        Dictionary<TriggerID, bool> t,
        Dictionary<string, bool> e,
        Dictionary<string, bool> o)
    {
        flags = f.Select(kv => new FlagEntry(kv.Key, kv.Value)).ToList();
        triggers = t.Select(kv => new TriggerEntry(kv.Key, kv.Value)).ToList();
        entities = e.Select(kv => new EntityEntry(kv.Key, kv.Value)).ToList();
        opened = o.Select(kv => new EntityEntry(kv.Key, kv.Value)).ToList();
    }

    public void ApplyTo(
        Dictionary<LevelFlag, bool> f,
        Dictionary<TriggerID, bool> t,
        Dictionary<string, bool> e,
        Dictionary<string, bool> o)
    {
        foreach (var entry in flags) f[entry.flag] = entry.value;
        foreach (var entry in triggers) t[entry.trigger] = entry.value;
        foreach (var entry in entities) e[entry.guid] = entry.removed;
        foreach (var entry in opened) o[entry.guid] = entry.removed;
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

[Serializable]
public struct EntityEntry
{
    public string guid;
    public bool removed;
    public EntityEntry(string g, bool r) { guid = g; removed = r; }
}