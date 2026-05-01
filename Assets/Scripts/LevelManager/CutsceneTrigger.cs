using UnityEngine;
using UnityEngine.Playables;
using System;

public class CutsceneTrigger : MonoBehaviour, ILevelEntity
{
    [SerializeField] private string guid;
    [SerializeField] private TriggerID trigger;
    [SerializeField] private PlayableDirector director;

    private bool played;

    public string Guid => guid;

#if UNITY_EDITOR
    [ContextMenu("Generate GUID")]
    private void GenerateGuid()
    {
        guid = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

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