/// <summary>
/// Implement on any AI agent that should receive staggered slow-tick updates
/// from <see cref="AITickManager"/> instead of running expensive logic every frame.
/// </summary>
public interface ITickableAI
{
    /// <summary>
    /// Called by AITickManager at a reduced rate (~5 times/sec per agent),
    /// staggered across frames so not all agents tick on the same frame.
    /// Put vision checks, distance calculations, and state decisions here.
    /// </summary>
    void SlowAITick();
}
