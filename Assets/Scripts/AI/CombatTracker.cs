using System.Collections.Generic;
using UnityEngine;

public class CombatTracker : MonoBehaviour
{
    public static CombatTracker Instance { get; private set; }

    private readonly HashSet<EnemyCombatState> combatEnemies = new HashSet<EnemyCombatState>();

    public bool IsInCombat => combatEnemies.Count > 0;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterCombat(EnemyCombatState enemy)
    {
        if (enemy != null)
            combatEnemies.Add(enemy);
    }

    public void UnregisterCombat(EnemyCombatState enemy)
    {
        if (enemy != null)
            combatEnemies.Remove(enemy);
    }
}