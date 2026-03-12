using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void Load(string sceneName)
    {
        
        LoadingScreen.targetScene = sceneName;

        
        SceneManager.LoadScene("LoadingScene");
    }
}