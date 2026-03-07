using UnityEngine;

/// <summary>
/// Modular enemy sound system — plays state-based audio with random variation.
///
/// DESIGN PRINCIPLES:
/// ● Separate from AI brain — brain calls PlayChaseSound(), this handles the rest.
/// ● Does NOT emit SoundStimulus — enemy audio never triggers ISoundListener,
///   preventing self-trigger loops and cascading alerts across the horde.
/// ● Performance-first — cooldown gates, no per-frame allocations, staggered delays.
/// ● Reusable — works with ZombieBrain, LarvaBrain, ProfactorBrain, or any future enemy.
///
/// INSPECTOR SETUP:
/// 1. Add this component to the enemy prefab.
/// 2. Assign an AudioSource (or leave null to auto-create one).
/// 3. Populate each SoundGroup's clips array with AudioClips.
/// 4. Tune cooldowns, volume, pitch per group.
///
/// INTEGRATION:
///   soundController = GetComponent&lt;EnemySoundController&gt;();
///   soundController.PlayChaseSound();   // from OnStateEnter
///   soundController.PlayHurtSound();    // from TakeDamage
///   soundController.PlayDeathSound();   // from Die
/// </summary>
public class EnemySoundController : MonoBehaviour
{
    // ──────────── Sound Group Definition ────────────

    [System.Serializable]
    public class SoundGroup
    {
        [Tooltip("Clips to randomly pick from. Leave empty to skip.")]
        public AudioClip[] clips;

        [Range(0f, 1f)]
        public float volume = 0.7f;

        [Tooltip("± random volume spread. 0.1 = volume ± 10%.")]
        [Range(0f, 0.3f)]
        public float volumeVariation = 0.05f;

        [Range(0.5f, 2f)]
        public float pitch = 1f;

        [Tooltip("± random pitch spread. 0.15 = pitch ± 0.15.")]
        [Range(0f, 0.5f)]
        public float pitchVariation = 0.1f;

        [Tooltip("Minimum seconds between plays. Prevents spam.")]
        [Range(0f, 30f)]
        public float cooldown = 1f;

        // internal tracking — not serialized
        [System.NonSerialized] public float lastPlayTime = -9999f;
        [System.NonSerialized] public int lastClipIndex = -1;
    }

    // ──────────── Inspector: Sound Groups ────────────

    [Header("Sound Groups")]
    [SerializeField] private SoundGroup idle = new SoundGroup { volume = 0.4f, cooldown = 4f };
    [SerializeField] private SoundGroup investigate = new SoundGroup { volume = 0.6f, cooldown = 2f };
    [SerializeField] private SoundGroup chase = new SoundGroup { volume = 0.7f, cooldown = 2f };
    [SerializeField] private SoundGroup attack = new SoundGroup { volume = 0.8f, cooldown = 0.5f };
    [SerializeField] private SoundGroup hurt = new SoundGroup { volume = 0.8f, cooldown = 0.3f };
    [SerializeField] private SoundGroup death = new SoundGroup { volume = 1f, cooldown = 0f };

    // ──────────── Inspector: Audio Source ────────────

    [Header("Audio Source")]
    [Tooltip("Assign an existing AudioSource, or leave null to auto-create.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Spatial blend: 0 = 2D, 1 = full 3D.")]
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 40f;

    // ──────────── Inspector: Global Throttle ────────────

    [Header("Global Throttle")]
    [Tooltip("Minimum seconds between ANY sound from this enemy. Prevents overlapping barks.")]
    [Range(0f, 2f)]
    [SerializeField] private float globalCooldown = 0.15f;

    [Tooltip("Random delay range (seconds) added before playback. Staggers groups of enemies.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float maxRandomDelay = 0.1f;

    private float lastAnySoundTime = -9999f;

    // ──────────── Public API ────────────

    /// <summary>Groans, breathing — subject to long cooldown.</summary>
    public void PlayIdleSound()       => Play(idle);

    /// <summary>Alert bark, sniffing — moderate cooldown.</summary>
    public void PlayInvestigateSound() => Play(investigate);

    /// <summary>Aggressive growl — moderate cooldown.</summary>
    public void PlayChaseSound()      => Play(chase);

    /// <summary>Swing, bite — short cooldown.</summary>
    public void PlayAttackSound()     => Play(attack);

    /// <summary>Pain yelp — very short cooldown, can overlap attacks.</summary>
    public void PlayHurtSound()       => Play(hurt);

    /// <summary>Death cry — no cooldown, highest priority.</summary>
    public void PlayDeathSound()      => Play(death, ignoreCooldown: true);

    /// <summary>Stop all sounds and reset cooldowns.</summary>
    public void StopAll()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    // ──────────── Core Playback ────────────

    private void Play(SoundGroup group, bool ignoreCooldown = false)
    {
        if (group == null || group.clips == null || group.clips.Length == 0)
            return;

        float now = Time.time;

        // Per-group cooldown gate
        if (!ignoreCooldown && now - group.lastPlayTime < group.cooldown)
            return;

        // Global cooldown gate — prevents two groups firing on the same frame
        if (!ignoreCooldown && now - lastAnySoundTime < globalCooldown)
            return;

        // Pick a random clip, avoiding the clip that just played
        int index = GetRandomClipIndex(group);
        AudioClip clip = group.clips[index];
        if (clip == null)
            return;

        // Record timestamps
        group.lastPlayTime = now;
        group.lastClipIndex = index;
        lastAnySoundTime = now;

        // Randomize pitch and volume
        float randomPitch = group.pitch + Random.Range(-group.pitchVariation, group.pitchVariation);
        float randomVolume = Mathf.Clamp01(group.volume + Random.Range(-group.volumeVariation, group.volumeVariation));

        // Small stagger delay so clustered enemies don't all bark at once.
        // For death sounds we skip the delay so it's instant.
        float delay = ignoreCooldown ? 0f : Random.Range(0f, maxRandomDelay);

        if (delay > 0.01f)
        {
            StartCoroutine(PlayDelayed(clip, randomPitch, randomVolume, delay));
        }
        else
        {
            PlayImmediate(clip, randomPitch, randomVolume);
        }
    }

    private void PlayImmediate(AudioClip clip, float pitch, float volume)
    {
        if (audioSource == null) return;

        audioSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        // PlayOneShot respects spatialBlend and uses its own volume param,
        // so we don't overwrite audioSource.volume (other clips might still be decaying).
        audioSource.PlayOneShot(clip, volume);
    }

    private System.Collections.IEnumerator PlayDelayed(AudioClip clip, float pitch, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayImmediate(clip, pitch, volume);
    }

    // ──────────── Helpers ────────────

    /// <summary>
    /// Returns a random index into group.clips.
    /// Avoids repeating the last clip when there are 2+ options.
    /// </summary>
    private static int GetRandomClipIndex(SoundGroup group)
    {
        int count = group.clips.Length;
        if (count == 1) return 0;

        int index;
        do
        {
            index = Random.Range(0, count);
        }
        while (index == group.lastClipIndex);

        return index;
    }

    // ──────────── Lifecycle ────────────

    private void Awake()
    {
        EnsureAudioSource();
    }

    /// <summary>
    /// Creates and configures an AudioSource if one wasn't assigned.
    /// </summary>
    private void EnsureAudioSource()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;  // no Doppler — cleaner gameplay audio
    }
}
