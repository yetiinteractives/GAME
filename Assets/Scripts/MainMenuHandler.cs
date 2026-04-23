using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{

    public void OnClickContinue()
    {
        SaveManager.Instance?.ContinueGameFromMainMenu();
    }

    public void OnClickNewGame()
    {
        SaveManager.Instance?.StartNewGameFromMainMenu();
    }

    public void OnQuitToDesktopPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnRampageModePressed()
    {
        SceneLoader.Load("RampageMode1");
    }

    



}
