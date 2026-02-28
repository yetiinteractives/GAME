using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight singleton that broadcasts sound events to registered ISoundListeners.
/// Attach to an empty GameObject in the scene or let it auto-create via Instance.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private readonly List<ISoundListener> listeners = new List<ISoundListener>(64);
    private readonly List<MonoBehaviour> listenerBehaviours = new List<MonoBehaviour>(64);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ──────────── Registration ────────────

    /// <summary>Register a listener. Call from OnEnable.</summary>
    public void Register(ISoundListener listener)
    {
        if (listener == null) return;
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
            listenerBehaviours.Add(listener as MonoBehaviour);
        }
    }

    /// <summary>Unregister a listener. Call from OnDisable.</summary>
    public void Unregister(ISoundListener listener)
    {
        int index = listeners.IndexOf(listener);
        if (index >= 0)
        {
            listeners.RemoveAt(index);
            listenerBehaviours.RemoveAt(index);
        }
    }

    // ──────────── Broadcasting ────────────

    /// <summary>
    /// Emit a sound into the world. Every registered listener within
    /// <paramref name="stimulus"/>.Loudness range will receive it.
    /// </summary>
    public void EmitSound(SoundStimulus stimulus)
    {
        float sqrLoudness = stimulus.Loudness * stimulus.Loudness;

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            MonoBehaviour mb = listenerBehaviours[i];

            // Skip destroyed or disabled listeners
            if (mb == null || !mb.isActiveAndEnabled)
                continue;

            float sqrDist = (mb.transform.position - stimulus.Position).sqrMagnitude;

            if (sqrDist <= sqrLoudness)
                listeners[i].HearSound(stimulus);
        }
    }
}
