using System.Collections.Generic;
using UnityEngine;

public class ArmyActivator : MonoBehaviour
{
    [Header("Army Root")]
    [SerializeField] private GameObject armyRoot;

    [Header("Activation Conditions")]
    [SerializeField] private List<LevelFlag> requiredFlags = new();
    [SerializeField] private List<TriggerID> requiredTriggers = new();

    private bool activated;

    private void OnEnable()
    {
        StartCoroutine(InitWhenReady());

    }

    private void OnDisable()
    {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnFlagChanged -= OnFlagChanged;
        LevelManager.Instance.OnTriggerFired -= OnTriggerFired;
    }

    private void OnFlagChanged(LevelFlag _) => TryActivate();
    private void OnTriggerFired(TriggerID _) => TryActivate();

    private void TryActivate()
    {
        if (activated) return;
        if (armyRoot == null) return;
        if (LevelManager.Instance == null) return;

        foreach (var f in requiredFlags)
            if (!LevelManager.Instance.GetFlag(f)) return;

        foreach (var t in requiredTriggers)
            if (!LevelManager.Instance.HasTriggered(t)) return;

        activated = true;
        armyRoot.SetActive(true);

        // Force persistence check for children that were inactive
        var persistents = armyRoot.GetComponentsInChildren<PersistentSceneEntity>(true);
        foreach (var p in persistents)
        {
            p.LoadState(); 
        }
    }

    

    private System.Collections.IEnumerator InitWhenReady()
    {
        while (LevelManager.Instance == null)
            yield return null;

        LevelManager.Instance.OnFlagChanged += OnFlagChanged;
        LevelManager.Instance.OnTriggerFired += OnTriggerFired;

        TryActivate();
    }

}