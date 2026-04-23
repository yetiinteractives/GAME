using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string newGameSceneName = "Level_01";

    private string SavePath => Path.Combine(Application.persistentDataPath, "save_slot_0.json");

    // pending load payload used when we route through LoadingScene
    private SaveData pendingLoadData;
    private bool hasPendingLoadData = false;

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
        // When target scene finished loading through LoadingScene pipeline,
        // apply pending save payload.
        if (hasPendingLoadData && pendingLoadData != null && scene.name == pendingLoadData.currentScene)
        {
            StartCoroutine(ApplyPendingLoadAfterSceneReady());
        }
    }

    public bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public void SaveGame()
    {
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
            Debug.LogWarning("[SaveManager] No save found. Starting new game instead.");
            StartNewGameFromMainMenu();
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogError("[SaveManager] Save file corrupted. Starting new game.");
            StartNewGameFromMainMenu();
            return;
        }

        // Store pending payload and use your LoadingScene architecture
        pendingLoadData = data;
        hasPendingLoadData = true;

        SceneLoader.Load(data.currentScene);
    }

    public void StartNewGameFromMainMenu()
    {
        DeleteSave();

        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SetHealth(100f);
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.ResetToDefaults();
        }

        // Use loading scene pipeline
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

    private IEnumerator ApplyPendingLoadAfterSceneReady()
    {
        // wait for Awake/Start/OnEnable chains + late UI binds
        yield return null;
        yield return null;

        if (!hasPendingLoadData || pendingLoadData == null)
            yield break;

        var data = pendingLoadData;

        // Apply resource data first so weapon/explosive/UI sync can pull correct values
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.ImportSaveData(data.resources);
            ResourceManager.Instance.ForceResyncAllRuntimeUsers();
        }

        // Apply player runtime state
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SetHealth(data.player.health);

            if (data.player.hasTransform)
                PlayerStateManager.Instance.SetTransform(data.player.position, data.player.eulerRotation);
        }

        // Push to actual player object in scene
        var playerHealth = FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        if (playerHealth != null)
            playerHealth.SetHealthFromSave(data.player.health);

        if (data.player.hasTransform)
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null)
            {
                player.position = data.player.position;
                player.rotation = Quaternion.Euler(data.player.eulerRotation);
            }
        }

        // One more frame for any late OnEnable overwrite, then hard resync
        yield return null;
        ResourceManager.Instance?.ForceResyncAllRuntimeUsers();

        // clear pending payload
        pendingLoadData = null;
        hasPendingLoadData = false;

        Debug.Log("[SaveManager] Continue load complete.");
    }
}