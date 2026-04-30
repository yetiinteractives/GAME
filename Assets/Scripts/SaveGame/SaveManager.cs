using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Invector.vCharacterController;

public sealed class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string newGameSceneName = "Level_01";

    [Header("Continue-load pipeline")]
    [SerializeField, Min(0)] private int settleFramesAfterSceneLoad = 3;
    [SerializeField, Min(0.1f)] private float playerFindTimeoutSeconds = 8f;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save_slot_0.json");

    private SaveData pendingLoadData;
    private bool hasPendingLoad;

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

    public bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    public void SaveGame()
    {
        // capture latest runtime state
        PlayerStateManager.Instance?.CaptureFromScene();

        var data = new SaveData
        {
            saveVersion = 1,
            savedAtUtc = DateTime.UtcNow.ToString("o"),
            currentScene = SceneManager.GetActiveScene().name
        };

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.CaptureRuntimeAmmoFromWeapons();
            data.resources = ResourceManager.Instance.ExportSaveData();
        }

        if (PlayerStateManager.Instance != null)
        {
            data.player.health = PlayerStateManager.Instance.SavedHealth;
            data.player.position = PlayerStateManager.Instance.SavedPosition;
            data.player.eulerRotation = PlayerStateManager.Instance.SavedEulerRotation;
            data.player.hasTransform = PlayerStateManager.Instance.HasTransformState;
        }

        string json = JsonUtility.ToJson(data, true);

        string tempPath = SavePath + ".tmp";
        File.WriteAllText(tempPath, json);
        if (File.Exists(SavePath)) File.Delete(SavePath);
        File.Move(tempPath, SavePath);

        Debug.Log($"[SaveManager] Saved: {SavePath}");
    }

    public void ContinueGameFromMainMenu()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveManager] No save found. Starting new game.");
            StartNewGameFromMainMenu();
            return;
        }

        SaveData data = LoadFromDisk();
        if (data == null)
        {
            Debug.LogError("[SaveManager] Save corrupted. Starting new game.");
            StartNewGameFromMainMenu();
            return;
        }

        pendingLoadData = data;
        hasPendingLoad = true;

        // Use your LoadingScene pipeline
        SceneLoader.Load(data.currentScene);
    }

    public void StartNewGameFromMainMenu()
    {
        DeleteSave();

        // Reset managers; DO NOT teleport
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SetHealth(100f);
            PlayerStateManager.Instance.ClearSavedTransform();
        }

        ResourceManager.Instance?.ResetToDefaults();

        SceneLoader.Load(newGameSceneName);
    }

    public void SaveAndQuit()
    {
        SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private SaveData LoadFromDisk()
    {
        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e}");
            return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingLoad || pendingLoadData == null) return;
        if (scene.name != pendingLoadData.currentScene) return;

        StartCoroutine(ContinueLoadPipeline(pendingLoadData));
    }

    private IEnumerator ContinueLoadPipeline(SaveData data)
    {
        // Phase 0: let the scene & player initialize (Invector Start/FixedUpdate)
        for (int i = 0; i < settleFramesAfterSceneLoad; i++)
            yield return null;

        // Phase 1: apply resources first
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.ImportSaveData(data.resources);
            ResourceManager.Instance.ForceResyncAllRuntimeUsers();
        }

        // Phase 2: apply health into state manager + scene component
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SetHealth(data.player.health);
            if (data.player.hasTransform)
                PlayerStateManager.Instance.SetTransform(data.player.position, data.player.eulerRotation);
        }

        var playerHealth = FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
            playerHealth.SetHealthFromSave(data.player.health);

        // Phase 3: teleport only if transform exists in save
        if (data.player.hasTransform)
            yield return TeleportPlayerInvectorSafe(data.player.position, data.player.eulerRotation);

        // Phase 4: final resync (UI binds)
        yield return null;
        ResourceManager.Instance?.ForceResyncAllRuntimeUsers();
        InventoryHandler.Instance?.SyncFromResourceManagerForUI();
        FindAnyObjectByType<SwitchWeapons>(FindObjectsInactive.Include)?.SyncFromResourceManager();

        pendingLoadData = null;
        hasPendingLoad = false;

        Debug.Log("[SaveManager] Load complete.");
    }

    private IEnumerator TeleportPlayerInvectorSafe(Vector3 pos, Vector3 euler)
    {
        float deadline = Time.realtimeSinceStartup + playerFindTimeoutSeconds;

        GameObject player = null;
        while (player == null && Time.realtimeSinceStartup < deadline)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) yield return null;
        }

        if (player == null)
        {
            Debug.LogWarning("[SaveManager] Player(tag=Player) not found; skipping teleport.");
            yield break;
        }

        var input = player.GetComponent<vThirdPersonInput>();
        var controller = player.GetComponent<vThirdPersonController>();
        var animator = player.GetComponentInChildren<Animator>(true);

        bool inputWasEnabled = input != null && input.enabled;
        bool controllerWasEnabled = controller != null && controller.enabled;
        bool animatorWasEnabled = animator != null && animator.enabled;

        // Pause anything that can drive transform/root motion
        if (input != null) input.enabled = false;
        if (controller != null) controller.enabled = false;
        if (animator != null) animator.enabled = false;

        // align with physics frame
        yield return new WaitForFixedUpdate();

        player.transform.SetPositionAndRotation(pos, Quaternion.Euler(euler));
        Physics.SyncTransforms();

        // allow one more fixed step with things disabled (prevents snap-back)
        yield return new WaitForFixedUpdate();
        yield return null;

        // Re-enable in safe order
        if (animator != null) animator.enabled = animatorWasEnabled;
        if (controller != null) controller.enabled = controllerWasEnabled;
        if (input != null) input.enabled = inputWasEnabled;
    }
}