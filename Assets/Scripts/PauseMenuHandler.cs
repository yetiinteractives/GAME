using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuHandler : MonoBehaviour
{
    public static event Action<bool> OnPauseMenuToggled;

    [SerializeField] private Image pauseMenu;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenu == null)
        {
            Debug.LogError("PauseMenuHandler: Pause menu GameObject is not assigned.");
            return;
        }
        pauseMenu.gameObject.SetActive(false);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                DisablePauseMenu();
            else
                EnablePauseMenu();
        }
    }



    public void EnablePauseMenu()
    {
        pauseMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        isPaused = true;
        OnPauseMenuToggled?.Invoke(true);
    }

    public void DisablePauseMenu()
    {
        pauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        OnPauseMenuToggled?.Invoke(false);
    }

    public void OnMainMenuPressed()
    {
        SceneLoader.Load("MainMenu");
    }

    public void OnReplayPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        DisablePauseMenu() ;
    }


    private void OnDisable()
    {
        DisablePauseMenu();
    }
}
