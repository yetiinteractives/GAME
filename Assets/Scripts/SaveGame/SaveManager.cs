using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string newGameSceneName = "Level_01";

    private string SavePath => Path.Combine(Application.persistentDataPath, "save_slot_0.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public void SaveGame()
    {
        // capture latest runtime state first
        PlayerStateManager.Instance?.CaptureFromScene();

        var data = new SaveData
        {
            saveVersion = 1,
            savedAtUtc = DateTime.UtcNow.ToString("o"),
            currentScene = SceneManager.GetActiveScene().name
        };

        if (ResourceManager.Instance != null)
            data.resources = ResourceManager.Instance.ExportSaveData();

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

        StartCoroutine(LoadFlow(data));
    }

    public void StartNewGameFromMainMenu()
    {
        DeleteSave();

        // reset runtime managers
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SetHealth(100f);
            // no transform yet on fresh new game
        }

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ResetToDefaults(); 

        SceneManager.LoadScene(newGameSceneName);
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

    private IEnumerator LoadFlow(SaveData data)
    {
        // 1) load scene first
        yield return SceneManager.LoadSceneAsync(data.currentScene);

        // 2) apply runtime player state into PlayerStateManager
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SetHealth(data.player.health);
            if (data.player.hasTransform)
                PlayerStateManager.Instance.SetTransform(data.player.position, data.player.eulerRotation);
        }

        // 3) apply resources
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ImportSaveData(data.resources);

        // 4) force apply player runtime now 
        PlayerStateManager.Instance?.CaptureFromScene();
    }
}