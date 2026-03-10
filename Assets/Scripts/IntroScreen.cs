using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScreen : MonoBehaviour
{
    public void LoadMainMenu()
    { 
        SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            LoadMainMenu();
        }
    }
}
