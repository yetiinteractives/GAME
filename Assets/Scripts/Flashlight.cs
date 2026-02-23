using System;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] GameObject flashlightOn;
    [SerializeField] GameObject flashlightOff;

    bool isFlashlightOn = true;

    private void Start()
    {
        flashlightOff.SetActive(false);
        flashlightOn.SetActive(true);
        isFlashlightOn=true;
    }

    private void Update()
    {
        if( Input.GetKeyDown(KeyCode.T))
        {
            ToggleFlashlight();

        }
    }

    private void ToggleFlashlight()
    {
        if (isFlashlightOn)
        {
            flashlightOn.SetActive(false);
            flashlightOff.SetActive(true);
            isFlashlightOn = false;
        }
        else
        {
            flashlightOn.SetActive(true);
            flashlightOff.SetActive(false);
            isFlashlightOn = true;
        }

    }

}
