/// <summary>
/// Implement on any enemy or NPC that should react to world sounds.
/// Register / unregister through SoundManager at OnEnable / OnDisable.
/// </summary>
public interface ISoundListener
{
    /// <summary>
    /// Called by SoundManager when a sound is emitted within hearing range.
    /// </summary>
    void HearSound(SoundStimulus stimulus);
}
