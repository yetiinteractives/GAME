using UnityEngine;

public class Puzzle : MonoBehaviour, ILevelEntity
{
    [SerializeField] private string guid;
    [SerializeField] private LevelFlag flagToUnlock;

    public string Guid => guid;

    private void Awake()
    {
        FindFirstObjectByType<LevelRegistry>().Register(this);
    }

    public void Solve()
    {
        LevelManager.Instance.TriggerFlag(flagToUnlock);
    }

    public void LoadState()
    {
        if (LevelManager.Instance.GetFlag(flagToUnlock))
        {
            // already solved visuals
        }
    }
}