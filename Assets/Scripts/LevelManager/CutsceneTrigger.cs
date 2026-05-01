using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour, ILevelEntity
{
    [SerializeField] private string guid;
    [SerializeField] private TriggerID trigger;
    [SerializeField] private PlayableDirector director;

    private bool played;

    public string Guid => guid;

    private void Awake()
    {
        FindFirstObjectByType<LevelRegistry>().Register(this);
    }

    private void Start()
    {
        LevelManager.Instance.OnTriggerFired += OnTrigger;
    }

    public void LoadState()
    {
        if (LevelManager.Instance.HasTriggered(trigger))
            played = true;
    }

    private void OnTrigger(TriggerID t)
    {
        if (t == trigger && !played)
            PlayCutscene();
    }

    private void PlayCutscene()
    {
        played = true;
        director.Play();
    }
}