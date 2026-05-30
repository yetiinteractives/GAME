using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [Header("Autdio field")]
    [SerializeField] private AudioSource normalSource;
    [SerializeField] private AudioSource combatSource;

    [Header("Music clip")]
    [SerializeField] private AudioClip normalClip;
    [SerializeField] private AudioClip combatClip;

    [SerializeField]
    private float fadeSpeed = 1.5f;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        normalSource.clip = normalClip;
        combatSource.clip = combatClip;

        normalSource.loop = true;
        combatSource.loop = true;

        normalSource.volume = 1f;
        combatSource.volume = 0f;

        normalSource.Play();
        combatSource.Play();
    }


    private void Update()
    {
        bool combat = CombatTracker.Instance != null && CombatTracker.Instance.IsInCombat;

        if (combat && combatSource.volume < 1f)
            FadeTo(combatSource, normalSource);
        else if (!combat && normalSource.volume < 1f)
            FadeTo(normalSource, combatSource);
    }

    private void FadeTo(AudioSource targetOn, AudioSource targetOff)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetOn, targetOff));
    }

    private IEnumerator FadeRoutine(AudioSource on, AudioSource off)
    {
        while (Mathf.Abs(on.volume - 1f) > 0.01f || Mathf.Abs(off.volume - 0f) > 0.01f)
        {
            on.volume = Mathf.MoveTowards(on.volume, 1f, fadeSpeed * Time.deltaTime);
            off.volume = Mathf.MoveTowards(off.volume, 0f, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        on.volume = 1f;
        off.volume = 0f;
    }
}

