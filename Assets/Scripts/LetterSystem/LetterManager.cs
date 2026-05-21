// LetterManager.cs — updated for Cinemachine 3 / Unity 6
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System; // ← Cinemachine 3 namespace (not "Cinemachine")

public class LetterManager : MonoBehaviour
{
    public static LetterManager Instance;

    [Header("Camera")]
    public CinemachineCamera letterVCam; // ← CinemachineCamera, not CinemachineVirtualCamera
    public float zoomDuration = 0.8f;

    [Header("UI")]
    public LetterUIController letterUI;

    [Header("Player")]
    public GameObject playerInput;

    private LetterData currentLetter;

    public event Action OnLetterOpened;
    public event Action OnLetterClosed;



    void Awake() => Instance = this;

    public void OpenLetter(LetterData data, Transform focusPoint)
    {
        currentLetter = data;
        StartCoroutine(OpenSequence(focusPoint));
        OnLetterOpened?.Invoke();
    }

    IEnumerator OpenSequence(Transform focusPoint)
    {
        playerInput.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Position the VCam at the focus point
        letterVCam.transform.position = focusPoint.position;
        letterVCam.transform.rotation = focusPoint.rotation;

        // Set look target and enable it
        letterVCam.LookAt = focusPoint;
        letterVCam.gameObject.SetActive(true); // ← Cinemachine 3: enable/disable GO instead of priority

        yield return new WaitForSeconds(zoomDuration);

        letterUI.ShowLetter(currentLetter);
    }

    public void CloseLetter()
    {
        
        StartCoroutine(CloseSequence());
        OnLetterClosed?.Invoke();
    }

    IEnumerator CloseSequence()
    {
        letterUI.HideLetter();

        letterVCam.gameObject.SetActive(false); // ← hand control back to main cam
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(zoomDuration);

        playerInput.SetActive(true);
        currentLetter = null;
    }

}