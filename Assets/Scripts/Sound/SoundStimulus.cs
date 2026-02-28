using UnityEngine;

/// <summary>
/// Immutable data packet describing a single sound event in the world.
/// Passed from SoundManager to every ISoundListener in range.
/// </summary>
public readonly struct SoundStimulus
{
    /// <summary>World position where the sound originated.</summary>
    public readonly Vector3 Position;

    /// <summary>Category of the sound (gunshot, footstep, etc.).</summary>
    public readonly SoundType Type;

    /// <summary>
    /// How far the sound can travel (in world units).
    /// Listeners beyond this radius will not be notified.
    /// </summary>
    public readonly float Loudness;

    /// <summary>
    /// Optional reference to the GameObject that caused the sound
    /// (e.g. the player, an exploding barrel). Can be null.
    /// </summary>
    public readonly GameObject Source;

    public SoundStimulus(Vector3 position, SoundType type, float loudness, GameObject source = null)
    {
        Position = position;
        Type = type;
        Loudness = loudness;
        Source = source;
    }
}
