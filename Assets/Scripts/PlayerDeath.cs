using Invector.vCharacterController;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField]private vThirdPersonController playerController;
    [SerializeField]private vThirdPersonInput playerInput;
    [SerializeField]private Image deathScreen;
    bool isRespawnScreenOn = false;

    private void Start()
    {
        PlayerHealth.OnPlayerDie += HandlePlayerDeath;

        if (playerController == null)
        {
            playerController = GetComponentInParent<vThirdPersonController>();
        }
        if (playerInput == null)
        {
            playerInput = GetComponentInParent<vThirdPersonInput>();
        }

        deathScreen.gameObject.SetActive(false);
    }

    private void HandlePlayerDeath()
    {
        GameInput.Instance.OnPlayerDeath();
        playerController.enabled = false;
        playerInput.enabled = false;
        ActivateDeathScreen();
        StartCoroutine(OnPlayerDie());
    }

    private void ActivateDeathScreen() 
    {

        deathScreen.gameObject.SetActive(true);
        Time.timeScale = 0.4f; 
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space)) && isRespawnScreenOn)
        {
            Respawn();
        }

    }

    private void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
        playerController.enabled = true;
        playerInput.enabled = true;
        GameInput.Instance.OnPlayerRespawn();

    }

    IEnumerator OnPlayerDie()
    {
        yield return new WaitForSeconds(1.5f);
        isRespawnScreenOn = true;
        Time.timeScale = 0f;
    }


    void OnDestroy()
    {
        PlayerHealth.OnPlayerDie -= HandlePlayerDeath;
    }
}
