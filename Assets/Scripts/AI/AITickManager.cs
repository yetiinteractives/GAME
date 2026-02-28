using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized manager that distributes slow AI ticks across frames.
/// Instead of every enemy running expensive logic every frame, the manager
/// spreads the work evenly: each agent gets ticked ~once per <see cref="tickInterval"/>
/// seconds, but the ticks are staggered so only a few agents update per frame.
///
/// At 60 fps with 100 agents and a 0.2s interval:
///   ticksPerFrame = ceil(100 * 0.0167 / 0.2) = 9 agents per frame
///   → zero spikes, each agent updates every ~0.18s
///
/// Attach to an empty GameObject alongside SoundManager.
/// </summary>
public class AITickManager : MonoBehaviour
{
    public static AITickManager Instance { get; private set; }

    [Tooltip("Target seconds between slow ticks for each agent. Lower = more responsive but more expensive.")]
    [SerializeField] private float tickInterval = 0.2f;

    private readonly List<ITickableAI> agents = new List<ITickableAI>(128);
    private int cursor;

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

    public void Register(ITickableAI agent)
    {
        if (agent != null && !agents.Contains(agent))
            agents.Add(agent);
    }

    public void Unregister(ITickableAI agent)
    {
        int index = agents.IndexOf(agent);
        if (index < 0) return;

        // Swap-remove for O(1) without shifting the list
        int last = agents.Count - 1;
        if (index < last)
            agents[index] = agents[last];

        agents.RemoveAt(last);

        // Keep cursor valid
        if (cursor > index)
            cursor--;
        if (cursor >= agents.Count)
            cursor = 0;
    }

    // ──────────── Tick Distribution ────────────

    private void Update()
    {
        int count = agents.Count;
        if (count == 0) return;

        // How many agents to tick this frame to maintain the target interval.
        // Formula auto-adapts to any frame rate.
        int ticksThisFrame = Mathf.CeilToInt(count * Time.deltaTime / tickInterval);
        ticksThisFrame = Mathf.Clamp(ticksThisFrame, 1, count);

        for (int i = 0; i < ticksThisFrame; i++)
        {
            if (cursor >= count)
                cursor = 0;

            agents[cursor].SlowAITick();
            cursor++;
        }
    }
}
