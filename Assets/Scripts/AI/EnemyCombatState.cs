using UnityEngine;

public class EnemyCombatState : MonoBehaviour
{
    private bool inCombat;

    public void SetCombatState(bool combat)
    {
        if (inCombat == combat) return;

        inCombat = combat;

        if (inCombat)
            CombatTracker.Instance.RegisterCombat(this);
        else
            CombatTracker.Instance.UnregisterCombat(this);
    }

    private void OnDisable()
    {
        if (inCombat && CombatTracker.Instance != null)
            CombatTracker.Instance.UnregisterCombat(this);
    }
}