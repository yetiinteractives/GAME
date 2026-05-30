using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource normalSource;
    [SerializeField] private AudioSource combatSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip normalClip;
    [SerializeField] private AudioClip combatClip;

    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private float combatExitDelay = 5f;

    private Coroutine fadeRoutine;
    private Coroutine exitDelayRoutine;

    private bool isCombatMusicActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

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

        isCombatMusicActive = false;
    }

    private void Update()
    {
        bool combat =
            CombatTracker.Instance != null &&
            CombatTracker.Instance.IsInCombat;

        if (combat)
        {
            // Cancel delayed return to normal music
            if (exitDelayRoutine != null)
            {
                StopCoroutine(exitDelayRoutine);
                exitDelayRoutine = null;
            }

            // Switch to combat music only once
            if (!isCombatMusicActive)
            {
                isCombatMusicActive = true;
                FadeTo(combatSource, normalSource);
            }
        }
        else
        {
            // Start delayed return only once
            if (isCombatMusicActive && exitDelayRoutine == null)
            {
                exitDelayRoutine = StartCoroutine(ReturnToNormalAfterDelay());
            }
        }
    }

    private IEnumerator ReturnToNormalAfterDelay()
    {
        yield return new WaitForSeconds(combatExitDelay);

        isCombatMusicActive = false;

        FadeTo(normalSource, combatSource);

        exitDelayRoutine = null;
    }

    private void FadeTo(AudioSource targetOn, AudioSource targetOff)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(targetOn, targetOff));
    }

    private IEnumerator FadeRoutine(AudioSource fadeIn, AudioSource fadeOut)
    {
        while (
            Mathf.Abs(fadeIn.volume - 1f) > 0.01f ||
            Mathf.Abs(fadeOut.volume - 0f) > 0.01f
        )
        {
            fadeIn.volume = Mathf.MoveTowards(
                fadeIn.volume,
                1f,
                fadeSpeed * Time.deltaTime
            );

            fadeOut.volume = Mathf.MoveTowards(
                fadeOut.volume,
                0f,
                fadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        fadeIn.volume = 1f;
        fadeOut.volume = 0f;

        fadeRoutine = null;
    }
}