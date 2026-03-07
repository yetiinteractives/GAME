using System;
using UnityEngine;
using Invector.vCharacterController;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField]private vThirdPersonController playerController;
    [SerializeField]private vThirdPersonInput playerInput;
    [SerializeField]private Image deathScreen;

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
    }

    private void ActivateDeathScreen() 
    {

        deathScreen.gameObject.SetActive(true);
        Time.timeScale = 0.4f; 
    }
}
