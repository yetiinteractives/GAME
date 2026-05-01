using System.Collections.Generic;
using UnityEngine;

public sealed class LevelRegistry : MonoBehaviour
{
    private readonly List<ILevelEntity> entities = new();

    public void Register(ILevelEntity entity)
    {
        if (!entities.Contains(entity))
            entities.Add(entity);
    }

    public void LoadAll()
    {
        foreach (var e in entities)
            e.LoadState();
    }
}