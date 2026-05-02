using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static string targetScene; 
    public Slider progressBar;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
           
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;

            // when loading finishes
            if (operation.progress >= 0.9f)
            {
                progressBar.value = 1f;
                
                operation.allowSceneActivation = true; 
            }

            yield return null;
        }
    }
}