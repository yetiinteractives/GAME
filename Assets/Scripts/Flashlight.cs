using System;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] GameObject flashlightOn;
    [SerializeField] GameObject flashlightOff;

    bool isFlashlightOn;

    private void Start()
    {
        flashlightOff.SetActive(true);
        flashlightOn.SetActive(false);
        isFlashlightOn=false;
    }

    private void Update()
    {
        if (GameInput.FlashlightDown)
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
