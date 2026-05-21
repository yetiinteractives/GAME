using UnityEngine;

public class LetterInteractable : MonoBehaviour
{
    public LetterData letterData;
    public Transform CameraFocusPoint;
    public float interactRadius= 2f;

    private bool playerInRange;
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            LetterManager.Instance.OpenLetter(letterData, CameraFocusPoint);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            HUDManager.Instance?.ShowPrompt("Press F to Read");

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HUDManger.Instance?.HidePrompt();
        }
    }

}
