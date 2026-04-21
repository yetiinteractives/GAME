using UnityEngine;
using UnityEngine.SceneManagement;

public class demoSceneChange : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(SceneManager.GetActiveScene().name == "Testing")
            SceneLoader.Load("Testing 2");
        else if (SceneManager.GetActiveScene().name == "Testing 2")
            SceneLoader.Load("Testing");
    }
}
