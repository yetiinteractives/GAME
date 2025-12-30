using UnityEngine;
using UnityEngine.UI;

public class CrossHairVisiliblity : MonoBehaviour
{
    public Image _crossHair;

    void Update()
    {
        //to show crosshair when right mouse button is held
        _crossHair.gameObject.SetActive(Input.GetMouseButton(1));

        //to hide cursor 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
}
