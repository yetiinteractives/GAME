using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStateManager : MonoBehaviour
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
        if (applyRoutine != null) StopCoroutine(applyRoutine);
        applyRoutine = StartCoroutine(ApplyStateAfterSceneLoad());
    }

    private IEnumerator ApplyStateAfterSceneLoad()
    {
        // wait a bit for player spawn/init
        yield return null;
        yield return null;

        var playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
            playerHealth.SetHealthFromSave(savedHealth);

        var player = FindPlayerTransform();
        if (player != null && hasTransformState)
        {
            player.position = savedPosition;
            player.rotation = Quaternion.Euler(savedEulerRotation);
        }

        applyRoutine = null;
    }

    public void CaptureFromScene()
    {
        var playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
            savedHealth = playerHealth.CurrentHealth;

        var player = FindPlayerTransform();
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

    private Transform FindPlayerTransform()
    {
        // Best: tag your root player GameObject as "Player"
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) return go.transform;

        // fallback
        var ph = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        return ph != null ? ph.transform : null;
    }
}