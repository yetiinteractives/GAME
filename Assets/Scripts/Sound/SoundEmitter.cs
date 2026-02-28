using UnityEngine;

/// <summary>
/// Drop this on any object that should produce sounds enemies can hear.
/// Call EmitSound() from code, animation events, or UnityEvents.
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    [Header("Defaults (used when calling EmitSound() with no args)")]
    [SerializeField] private SoundType defaultSoundType = SoundType.Gunshot;
    [SerializeField] private float defaultLoudness = 30f;

    /// <summary>
    /// Emit a sound at this object's position using the inspector defaults.
    /// Useful as an animation event or UnityEvent callback.
    /// </summary>
    public void EmitSound()
    {
        EmitSound(defaultSoundType, defaultLoudness);
    }

    /// <summary>
    /// Emit a sound at this object's position with explicit parameters.
    /// </summary>
    public void EmitSound(SoundType type, float loudness)
    {
        EmitSoundAt(transform.position, type, loudness, gameObject);
    }

    /// <summary>
    /// Emit a sound at an arbitrary world position (e.g. bullet impact point).
    /// </summary>
    public void EmitSoundAt(Vector3 position, SoundType type, float loudness)
    {
        EmitSoundAt(position, type, loudness, gameObject);
    }

    /// <summary>
    /// Static helper so callers without a SoundEmitter component can still broadcast.
    /// </summary>
    public static void EmitSoundAt(Vector3 position, SoundType type, float loudness, GameObject source)
    {
        if (SoundManager.Instance == null) return;

        SoundStimulus stimulus = new SoundStimulus(position, type, loudness, source);
        SoundManager.Instance.EmitSound(stimulus);
    }
}
