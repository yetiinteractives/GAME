using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public sealed class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    [Header("Runtime Player State (Cross-Scene)")]
    [SerializeField] private float savedHealth = 100f;
    [SerializeField] private Vector3 savedPosition;
    [SerializeField] private Vector3 savedEulerRotation;
    [SerializeField] private bool hasTransformState = false;

    private Coroutine applyRoutine;

    public float SavedHealth => savedHealth;
    public Vector3 SavedPosition => savedPosition;
    public Vector3 SavedEulerRotation => savedEulerRotation;
    public bool HasTransformState => hasTransformState;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only apply HEALTH on scene load. Transform is owned by SaveManager.
        if (applyRoutine != null) StopCoroutine(applyRoutine);
        applyRoutine = StartCoroutine(ApplyHealthAfterSceneLoad());
    }

    private IEnumerator ApplyHealthAfterSceneLoad()
    {
        // Let scene initialize
        yield return null;
        yield return null;

        var playerHealth = FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
            playerHealth.SetHealthFromSave(savedHealth);

        applyRoutine = null;
    }

    public void CaptureFromScene()
    {
        var playerHealth = FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
            savedHealth = playerHealth.CurrentHealth;

        Transform player = FindPlayerRoot();
        if (player != null)
        {
            savedPosition = player.position;
            savedEulerRotation = player.eulerAngles;
            hasTransformState = true;
        }
    }

    public void SetHealth(float value)
    {
        savedHealth = Mathf.Clamp(value, 0f, 100f);
    }

    public void SetTransform(Vector3 position, Vector3 eulerRotation)
    {
        savedPosition = position;
        savedEulerRotation = eulerRotation;
        hasTransformState = true;
    }

    public void ClearSavedTransform()
    {
        hasTransformState = false;
        savedPosition = Vector3.zero;
        savedEulerRotation = Vector3.zero;
    }

    private static Transform FindPlayerRoot()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.transform : null;
    }
}