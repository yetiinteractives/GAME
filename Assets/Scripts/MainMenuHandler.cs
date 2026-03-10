using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public void OnRampageModePressed()
    {
        SceneManager.LoadScene("RampageMode1");
    }

    public void OnQuitToDesktopPressed()
    {
        Application.Quit();
    }
}
