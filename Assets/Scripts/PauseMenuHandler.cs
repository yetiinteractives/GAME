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



    private void EnablePauseMenu()
    {
        pauseMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        OnPauseMenuToggled?.Invoke(true);
    }

    private void DisablePauseMenu()
    {
        pauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        OnPauseMenuToggled?.Invoke(false);
    }

    public void OnMainMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDisable()
    {
        DisablePauseMenu();
    }
}
