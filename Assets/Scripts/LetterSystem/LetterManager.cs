using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using Unity.Cinemachine;


public class LetterManager : MonoBehaviour
{
    public static LetterManager Instance;
    [Header("Camera")]
    public CinemachineVirtualCamera LetterVCam;
    public float zoomDuration = 0.8f;
    [Header("UI")]
    public LetterUIController letterUI;

    [Header("Player")]
    public GameObject playerInput;

    private LetterData currentLetter;
    void Awake() => Instance = this;
    public void OpenLetter(LetterData data, Transform focusPoint)
    {
        currentLetter = data;
        StartCoroutine(OpenSequence(focusPoint));

    }
    IEnumerator OpenSequence(Transform focusPoint)
    {
        playerInput.SetActive(false);
        LetterVCam.Follow = focusPoint;
        LetterVCam.Priority = 20;
        yield return new WaitForSeconds(zoomDuration);

        letterUI.ShowLetter(currentLetter);
    }
    public void CloseLetter()
    {
        StartCoroutine(CloseSequence());

    }
    IEnumerator CloseSequence()
    {
        letterUI.HideLetter();
        LetterVCam.Priority = 0;
        yield return new WaitForSeconds(zoomDuration);

        playerInput.SetActive(true);
        currentLetter = null;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
