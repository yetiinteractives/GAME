using UnityEngine;
using System.Collections.Generic;

public class CombatTracker : MonoBehaviour
{
    public static CombatTracker instance {  get; private set; }
    private readonly HashSet<EnemyCombatState> combatEnemies = new HashSet<EnemyCombatState>();

    public bool IsInCombat => combatEnemies.Count > 0;
     private void Awake()
    {
        instance = this;
    }
    public void RegisterCombat(EnemyCombatState enemy)
    {
        if (enemy != null)
            combatEnemies.Add(enemy);
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
